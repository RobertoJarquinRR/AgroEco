using AgroEco.Core.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroEco.Core.Triggers
{
    public abstract class Trigger : IEntity
    {
        private readonly object _subscriptionLock = new();
        private readonly List<ITriggerable> _triggerables = new();
        private CancellationTokenSource? _executionCancellation;
        private bool _stoppedDuringExecution;
        private bool _lastExecutionWasStopped;

        public int Id { get; private set; }

        public string? Name { get; private set; }
        [NotMapped]
        public TriggerRuntimeStatus RuntimeStatus { get; private set; } = TriggerRuntimeStatus.Created;
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

        public bool LastExecutionWasStopped => _lastExecutionWasStopped;

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

        public Result Subscribe(ITriggerable triggerable)
        {
            ArgumentNullException.ThrowIfNull(triggerable);

            lock (_subscriptionLock)
            {
                if (RuntimeStatus is TriggerRuntimeStatus.Notifying
                    or TriggerRuntimeStatus.Completed
                    or TriggerRuntimeStatus.Stopped
                    or TriggerRuntimeStatus.Faulted)
                {
                    return Result.CreateFailure(
                        $"Trigger '{Name}' cannot accept subscriptions in state {RuntimeStatus}.");
                }

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

        internal Result BeginExecution()
        {
            lock (_subscriptionLock)
            {
                if (RuntimeStatus is TriggerRuntimeStatus.Active
                    or TriggerRuntimeStatus.Notifying)
                {
                    return Result.CreateFailure(
                        $"Trigger '{Name}' is already active.");
                }

                if (RuntimeStatus is TriggerRuntimeStatus.Completed
                    or TriggerRuntimeStatus.Stopped
                    or TriggerRuntimeStatus.Faulted)
                {
                    return Result.CreateFailure(
                        $"Trigger '{Name}' cannot be started from state {RuntimeStatus}.");
                }

                var previousStatus = RuntimeStatus;
                RuntimeStatus = TriggerRuntimeStatus.Active;
                _executionCancellation = new CancellationTokenSource();
                _ = NotifyRuntimeStatusChangedAsync(previousStatus, RuntimeStatus);
                return Result.CreateSuccess();
            }
        }

        internal CancellationToken ExecutionToken
            => _executionCancellation?.Token ?? CancellationToken.None;

        internal Result StopExecution()
        {
            lock (_subscriptionLock)
            {
                if (RuntimeStatus is not (TriggerRuntimeStatus.Active
                    or TriggerRuntimeStatus.Notifying))
                {
                    return Result.CreateFailure(
                        $"Trigger '{Name}' is not active.");
                }

                var previousStatus = RuntimeStatus;
                _executionCancellation?.Cancel();
                _stoppedDuringExecution = true;
                RuntimeStatus = TriggerRuntimeStatus.Stopped;
                _ = NotifyRuntimeStatusChangedAsync(previousStatus, RuntimeStatus);
                return Result.CreateSuccess($"Trigger '{Name}' stopped.");
            }
        }

        internal void CompleteExecution(Result executionResult)
        {
            lock (_subscriptionLock)
            {
                if (RuntimeStatus == TriggerRuntimeStatus.Stopped)
                {
                    _executionCancellation?.Dispose();
                    _executionCancellation = null;
                    return;
                }

                var previousStatus = RuntimeStatus;
                RuntimeStatus = executionResult.Success
                    ? TriggerRuntimeStatus.Completed
                    : TriggerRuntimeStatus.Faulted;
                _executionCancellation?.Dispose();
                _executionCancellation = null;
                _ = NotifyRuntimeStatusChangedAsync(previousStatus, RuntimeStatus);
            }
        }

        public async Task<TriggerExecutionReport> ExecuteAsync()
        {
            TriggerExecutionReport report;
            try
            {
                Result readyResult = await WaitUntilReadyAsync(ExecutionToken);
                if (!readyResult.Success)
                {
                    lock (_subscriptionLock)
                    {
                        report = BuildExecutionReport(readyResult);
                        if (RuntimeStatus == TriggerRuntimeStatus.Stopped || _stoppedDuringExecution)
                        {
                            report = report with { RuntimeStatus = TriggerRuntimeStatus.Stopped };
                            _lastExecutionWasStopped = true;
                        }
                    }
                    await PublishExecutionCompletedAsync(report);
                    return report;
                }

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
                // When canceled (stopped), don't publish ExecutionCompleted - the RuntimeStatusChanged to Stopped is sufficient
                var cancelResult = Result.CreateFailure("The trigger execution was canceled.");
                lock (_subscriptionLock)
                {
                    report = BuildExecutionReport(cancelResult);
                    if (RuntimeStatus == TriggerRuntimeStatus.Stopped || _stoppedDuringExecution)
                    {
                        report = report with { RuntimeStatus = TriggerRuntimeStatus.Stopped };
                    }
                }
                _lastExecutionWasStopped = true;
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
            finally
            {
                _stoppedDuringExecution = false;
            }
        }

        private async Task PublishExecutionCompletedAsync(TriggerExecutionReport report)
        {
            if (_stoppedDuringExecution || report.RuntimeStatus == TriggerRuntimeStatus.Stopped)
            {
                return;
            }

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
                RuntimeStatus = TriggerRuntimeStatus.Notifying;
                subscribers = _triggerables.ToArray();
            }

            var previousStatus = TriggerRuntimeStatus.Active;
            _ = NotifyRuntimeStatusChangedAsync(previousStatus, TriggerRuntimeStatus.Notifying);

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

        private async Task NotifyRuntimeStatusChangedAsync(
            TriggerRuntimeStatus previousStatus,
            TriggerRuntimeStatus newStatus)
        {
            IReadOnlyList<TriggerSubscriberSnapshot> subscribers;
            lock (_subscriptionLock)
            {
                subscribers = GetSubscriberSnapshots();
            }

            if (RuntimeStatusChanged is not null)
            {
                var change = new TriggerRuntimeChange(
                    TriggerId: Id,
                    TriggerName: Name ?? string.Empty,
                    PreviousStatus: previousStatus,
                    NewStatus: newStatus,
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
                // Observer exceptions must not break trigger execution
            }
        }

        protected abstract Task<Result> WaitUntilReadyAsync(
            CancellationToken cancellationToken);
    }
}
