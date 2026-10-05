using AgroEco.Core.Triggers.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace AgroEco.Core.Triggers;

public sealed class TriggerEngine
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Registry<int, Trigger> _registry = new();
        private readonly SemaphoreSlim _registryLock = new(1, 1);

        public event Func<TriggerRuntimeChange, Task>? RuntimeStatusChanged;
        public event Func<TriggerExecutionReport, Task>? ExecutionCompleted;

        public TriggerEngine(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<Result<Trigger>> GetOrCreateAsync(
            int triggerId,
            CancellationToken cancellationToken = default)
        {
            await _registryLock.WaitAsync(cancellationToken);
            try
            {
                Trigger? registeredTrigger = _registry.Get(triggerId);
                if (registeredTrigger is not null)
                {
                    if (registeredTrigger.RuntimeStatus is TriggerRuntimeStatus.Completed
                        or TriggerRuntimeStatus.Faulted
                        || (registeredTrigger.RuntimeStatus == TriggerRuntimeStatus.Stopped
                            && !registeredTrigger.HasTriggerables))
                    {
                        _registry.Unregister(triggerId);
                    }
                    else
                    {
                        return Result<Trigger>.CreateSuccess(registeredTrigger);
                    }
                }

                using IServiceScope scope = _scopeFactory.CreateScope();
                GetByIdTrigger getByIdTrigger = scope.ServiceProvider
                    .GetRequiredService<GetByIdTrigger>();

                Result<Trigger> result = await getByIdTrigger.HandleAsync(
                    triggerId,
                    cancellationToken);

                if (!result.Success || result.Value is null)
                {
                    return result;
                }

                _registry.Register(triggerId, result.Value);
                SubscribeToTriggerEvents(result.Value);
                return Result<Trigger>.CreateSuccess(result.Value);
            }
            finally
            {
                _registryLock.Release();
            }
        }

        public async Task<Result> SubscribeAsync(
            int triggerId,
            ITriggerable triggerable,
            CancellationToken cancellationToken = default)
        {
            Result<Trigger> triggerResult = await GetOrCreateAsync(
                triggerId,
                cancellationToken);

            if (!triggerResult.Success || triggerResult.Value is null)
            {
                return Result.CreateFailure(triggerResult.Message, triggerResult.Exception);
            }

            return triggerResult.Value.Subscribe(triggerable);
        }

        public async Task<Result> UnsubscribeAsync(
            int triggerId,
            ITriggerable triggerable,
            CancellationToken cancellationToken = default)
        {
            Result<Trigger> triggerResult = await GetOrCreateAsync(
                triggerId,
                cancellationToken);

            if (!triggerResult.Success || triggerResult.Value is null)
            {
                return Result.CreateFailure(triggerResult.Message, triggerResult.Exception);
            }

            Result result = triggerResult.Value.Unsubscribe(triggerable);
            RemoveIfInactive(triggerId, triggerResult.Value);
            return result;
        }

        public async Task<Result> StartAsync(
            int triggerId,
            CancellationToken cancellationToken = default)
        {
            Result<Trigger> triggerResult = await GetOrCreateAsync(
                triggerId,
                cancellationToken);

            if (!triggerResult.Success || triggerResult.Value is null)
            {
                return Result.CreateFailure(triggerResult.Message, triggerResult.Exception);
            }

            Trigger trigger = triggerResult.Value;
            if (trigger.RuntimeStatus is TriggerRuntimeStatus.Active
                or TriggerRuntimeStatus.Notifying)
            {
                return Result.CreateSuccess(
                    $"Trigger '{trigger.Name}' is already active.");
            }

            if (!trigger.HasTriggerables)
            {
                return Result.CreateFailure(
                    $"Trigger '{trigger.Name}' has no subscribers.");
            }

            Result beginResult = trigger.BeginExecution();
            if (!beginResult.Success)
            {
                return beginResult;
            }

            _ = ExecuteAsync(triggerId, trigger);
            return Result.CreateSuccess($"Trigger '{trigger.Name}' started.");
        }

        public async Task<Result> StopAsync(
            int triggerId,
            CancellationToken cancellationToken = default)
        {
            Result<Trigger> triggerResult = await GetOrCreateAsync(
                triggerId,
                cancellationToken);

            if (!triggerResult.Success || triggerResult.Value is null)
            {
                return Result.CreateFailure(triggerResult.Message, triggerResult.Exception);
            }

            Result result = triggerResult.Value.StopExecution();
            RemoveIfInactive(triggerId, triggerResult.Value);
            return result;
        }

        public IReadOnlyList<TriggerRuntimeSnapshot> GetRuntimeSnapshots()
        {
            var snapshots = new List<TriggerRuntimeSnapshot>();
            foreach (var trigger in _registry.All())
            {
                var subscribers = trigger.GetSubscriberSnapshots();
                snapshots.Add(new TriggerRuntimeSnapshot(
                    TriggerId: trigger.Id,
                    Name: trigger.Name ?? string.Empty,
                    RuntimeStatus: trigger.RuntimeStatus,
                    SubscriberCount: subscribers.Count,
                    Subscribers: subscribers));
            }
            return snapshots;
        }

        public IEnumerable<Trigger> GetActiveTriggers()
            => _registry.Where(trigger =>
                trigger.RuntimeStatus is TriggerRuntimeStatus.Active
                    or TriggerRuntimeStatus.Notifying);

        private async Task ExecuteAsync(int triggerId, Trigger trigger)
        {
            TriggerExecutionReport report;

            try
            {
                report = await trigger.ExecuteAsync();
            }
            catch (Exception exception)
            {
                var errorResult = Result.CreateFailure(
                    $"Trigger '{trigger.Name}' failed during execution.",
                    exception);
                report = new TriggerExecutionReport(
                    TriggerId: trigger.Id,
                    TriggerName: trigger.Name ?? string.Empty,
                    RuntimeStatus: TriggerRuntimeStatus.Faulted,
                    SubscriberReports: [],
                    OverallResult: errorResult);
            }

trigger.CompleteExecution(report.OverallResult);

            // Don't publish if the trigger was explicitly stopped or execution was canceled
            bool isCanceled = report.OverallResult.Message?.Contains("canceled", StringComparison.OrdinalIgnoreCase) == true;
            if (!trigger.LastExecutionWasStopped
                && !isCanceled
                && report.RuntimeStatus != TriggerRuntimeStatus.Stopped
                && trigger.RuntimeStatus != TriggerRuntimeStatus.Stopped
                && ExecutionCompleted is not null)
            {
                await InvokeEventSafelyAsync(ExecutionCompleted, report);
            }
            RemoveIfInactive(triggerId, trigger);
        }

        private void SubscribeToTriggerEvents(Trigger trigger)
        {
            trigger.RuntimeStatusChanged += OnTriggerRuntimeStatusChangedAsync;
            // Note: We do NOT subscribe to trigger.ExecutionCompleted to avoid duplicate events
            // The TriggerEngine fires its own ExecutionCompleted event based on the report from trigger.ExecuteAsync()
        }

        private async Task OnTriggerRuntimeStatusChangedAsync(TriggerRuntimeChange change)
        {
            if (RuntimeStatusChanged is not null)
            {
                await InvokeEventSafelyAsync(RuntimeStatusChanged, change);
            }
        }

        private void RemoveIfInactive(int triggerId, Trigger trigger)
        {
            if (trigger.RuntimeStatus is TriggerRuntimeStatus.Completed
                or TriggerRuntimeStatus.Faulted)
            {
                _registry.Unregister(triggerId);
                return;
            }

            if (trigger.RuntimeStatus == TriggerRuntimeStatus.Stopped)
            {
                return;
            }

            if (trigger.HasTriggerables
                || trigger.RuntimeStatus == TriggerRuntimeStatus.Notifying)
            {
                return;
            }

            if (trigger.RuntimeStatus == TriggerRuntimeStatus.Active)
            {
                trigger.StopExecution();
            }

            _registry.Unregister(triggerId);
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
    }