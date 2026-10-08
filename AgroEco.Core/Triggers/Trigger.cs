using AgroEco.Core.Interfaces;
using AgroEco.Core.Triggers.Configuration;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroEco.Core.Triggers
{
    public abstract class Trigger : IEntity
    {
        private readonly object _subscriptionLock = new();
        private readonly List<ITriggerable> _triggerables = new();
        private CancellationTokenSource? _executionCancellation;

        public int Id { get; private set; }

        public string? Name { get; private set; }
        [NotMapped]
        public TriggerRuntimeStatus RuntimeStatus { get; internal set; } = TriggerRuntimeStatus.Idle;
        public bool Enabled { get; private set; }
        public int? MaxExecutions { get; private set; }
        public int FiredCount { get; private set; }
        public DateTimeOffset? LastFiredAt { get; private set; }
        public bool HasReachedMaxExecutions => MaxExecutions.HasValue && FiredCount >= MaxExecutions.Value;
        public bool HasTriggerables
        {
            get
            {
                lock (_subscriptionLock)
                {
                    return _triggerables.Count > 0;
                }
            }
        }

        public event Func<TriggerRuntimeChange, Task>? RuntimeStatusChanged;
        public event Func<TriggerExecutionReport, Task>? ExecutionCompleted;

        protected Trigger(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            Name = name;
        }

        public Result UpdateDetails(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.CreateFailure("Trigger name can't be empty");
            }

            Name = name.Trim();
            return Result.CreateSuccess();
        }

        public virtual TriggerConfiguration GetConfiguration()
            => throw new NotSupportedException($"{GetType().Name} does not support configuration extraction.");

        public virtual Result UpdateConfiguration(TriggerConfiguration configuration)
            => Result.CreateFailure($"{GetType().Name} does not support configuration updates.");

        public Result Subscribe(ITriggerable triggerable)
        {
            ArgumentNullException.ThrowIfNull(triggerable);

            lock (_subscriptionLock)
            {
                bool added = false;
                if (!_triggerables.Contains(triggerable))
                {
                    _triggerables.Add(triggerable);
                    added = true;
                }

                if (added)
                {
                    _ = NotifySubscriptionChangedAsync();
                }

                return Result.CreateSuccess(
                    $"Trigger '{Name}' subscription registered.");
            }
        }

        public Result Unsubscribe(ITriggerable triggerable)
        {
            ArgumentNullException.ThrowIfNull(triggerable);

            lock (_subscriptionLock)
            {
                bool removed = _triggerables.Remove(triggerable);

                if (!removed)
                {
                    return Result.CreateFailure(
                        $"Trigger '{Name}' subscription was not found.");
                }

                _ = NotifySubscriptionChangedAsync();

                return Result.CreateSuccess(
                    $"Trigger '{Name}' subscription removed.");
            }
        }

        public Result Enable()
        {
            if (Enabled)
            {
                return Result.CreateFailure($"Trigger '{Name}' is already enabled.");
            }

            Enabled = true;
            _executionCancellation = new CancellationTokenSource();
            return Result.CreateSuccess($"Trigger '{Name}' enabled.");
        }

        public Result Disable()
        {
            if (!Enabled)
            {
                return Result.CreateFailure($"Trigger '{Name}' is already disabled.");
            }

            Enabled = false;
            _executionCancellation?.Cancel();
            _executionCancellation?.Dispose();
            _executionCancellation = null;
            return Result.CreateSuccess($"Trigger '{Name}' disabled.");
        }

        public Result SetMaxExecutions(int? maxExecutions)
        {
            if (maxExecutions.HasValue && maxExecutions.Value <= 0)
            {
                return Result.CreateFailure("MaxExecutions must be positive or null for infinite.");
            }

            MaxExecutions = maxExecutions;
            return Result.CreateSuccess();
        }

        internal Result RegisterFiring(DateTimeOffset at)
        {
            if (HasReachedMaxExecutions)
            {
                return Result.CreateFailure($"Trigger '{Name}' has reached maximum executions ({MaxExecutions.Value}).");
            }

            FiredCount++;
            LastFiredAt = at;
            return Result.CreateSuccess();
        }

        internal CancellationToken ExecutionToken
            => _executionCancellation?.Token ?? CancellationToken.None;

        internal async Task<Result> WaitAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await WaitUntilReadyAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return Result.CreateFailure("The trigger execution was canceled.");
            }
        }

        public virtual async Task<TriggerExecutionReport> ExecuteAsync()
        {
            Result readyResult = await WaitUntilReadyAsync(ExecutionToken);
            if (!readyResult.Success)
            {
                TriggerExecutionReport notReadyReport = BuildExecutionReport(readyResult);
                await PublishExecutionCompletedAsync(notReadyReport);
                return notReadyReport;
            }

            return await ExecuteReadyAsync();
        }

        internal async Task<TriggerExecutionReport> ExecuteReadyAsync()
        {
            TriggerExecutionReport report;
            try
            {
                var subscriberReports = await NotifyTriggerablesAsync();
                Result overallResult = subscriberReports.Count == 0
                    ? Result.CreateFailure("No triggerable subscribers are registered.")
                    : subscriberReports.All(r => r.Result.Success)
                        ? Result.CreateSuccess()
                        : Result.CreateFailure("One or more triggerables failed during execution.");

                report = BuildExecutionReport(overallResult, subscriberReports);
                await PublishExecutionCompletedAsync(report);
                return report;
            }
            catch (OperationCanceledException)
            {
                report = BuildExecutionReport(
                    Result.CreateFailure("The trigger execution was canceled."));
                return report;
            }
            catch (Exception exception)
            {
                var errorResult = Result.CreateFailure(
                    $"Trigger '{Name}' failed during execution.",
                    exception);
                report = BuildExecutionReport(errorResult);
                await PublishExecutionCompletedAsync(report);
                return report;
            }
        }

        private async Task PublishExecutionCompletedAsync(TriggerExecutionReport report)
        {
            if (ExecutionCompleted is not null)
            {
                await InvokeEventSafelyAsync(ExecutionCompleted, report);
            }
        }

        private async Task<List<TriggerableExecutionReport>> NotifyTriggerablesAsync()
        {
            ITriggerable[] subscribers;

            lock (_subscriptionLock)
            {
                RuntimeStatus = TriggerRuntimeStatus.Running;
                subscribers = _triggerables.ToArray();
            }

            var reports = new List<TriggerableExecutionReport>();
            if (subscribers.Length == 0)
            {
                return reports;
            }

            var tasks = subscribers.Select(async triggerable =>
            {
                Result result;
                try
                {
                    result = await triggerable.OnTrigger();
                }
                catch (Exception exception)
                {
                    result = Result.CreateFailure(
                        $"Trigger subscriber failed with exception: {exception.Message}",
                        exception);
                }

                return new TriggerableExecutionReport(
                    SubscriberType: triggerable.GetType().Name,
                    SubscriberId: GetSubscriberId(triggerable),
                    SubscriberName: GetSubscriberName(triggerable),
                    Result: result);
            }).ToArray();

            var results = await Task.WhenAll(tasks);
            reports.AddRange(results);
            return reports;
        }

        private TriggerExecutionReport BuildExecutionReport(
            Result overallResult,
            IReadOnlyList<TriggerableExecutionReport>? subscriberReports = null)
        {
            lock (_subscriptionLock)
            {
                return new TriggerExecutionReport(
                    TriggerId: Id,
                    TriggerName: Name ?? string.Empty,
                    RuntimeStatus: RuntimeStatus,
                    SubscriberReports: subscriberReports ?? [],
                    OverallResult: overallResult);
            }
        }

        private static int? GetSubscriberId(ITriggerable triggerable)
        {
            return triggerable switch
            {
                IEntity entity => entity.Id,
                _ => null
            };
        }

        private static string? GetSubscriberName(ITriggerable triggerable)
        {
            var nameProperty = triggerable.GetType().GetProperty("Name");
            return nameProperty?.GetValue(triggerable) as string;
        }

        public IReadOnlyList<TriggerSubscriberSnapshot> GetSubscriberSnapshots()
        {
            lock (_subscriptionLock)
            {
                return _triggerables.Select(t => new TriggerSubscriberSnapshot(
                    Type: t.GetType().Name,
                    Id: GetSubscriberId(t),
                    Name: GetSubscriberName(t)
                )).ToArray();
            }
        }

        private async Task NotifySubscriptionChangedAsync()
        {
            IReadOnlyList<TriggerSubscriberSnapshot> subscribers;
            TriggerRuntimeStatus currentStatus;

            lock (_subscriptionLock)
            {
                subscribers = GetSubscriberSnapshots();
                currentStatus = RuntimeStatus;
            }

            if (RuntimeStatusChanged is not null)
            {
                var change = new TriggerRuntimeChange(
                    TriggerId: Id,
                    TriggerName: Name ?? string.Empty,
                    PreviousStatus: currentStatus,
                    NewStatus: currentStatus,
                    Subscribers: subscribers);
                await InvokeEventSafelyAsync(RuntimeStatusChanged, change);
            }
        }

        private static async Task InvokeEventSafelyAsync<T>(Func<T, Task> eventDelegate, T args)
        {
            var handlers = eventDelegate.GetInvocationList();
            var tasks = handlers
                .Cast<Func<T, Task>>()
                .Select(handler => InvokeHandlerSafelyAsync(handler, args));
            await Task.WhenAll(tasks);
        }

        private static async Task InvokeHandlerSafelyAsync<T>(Func<T, Task> handler, T args)
        {
            try
            {
                await handler(args);
            }
            catch
            {
            }
        }

        protected abstract Task<Result> WaitUntilReadyAsync(
            CancellationToken cancellationToken);
    }
}