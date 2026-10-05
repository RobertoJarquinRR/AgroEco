using AgroEco.Core.Interfaces;
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

        protected Trigger(string name ){
            if(string.IsNullOrEmpty(name)){
                return;
            }
            ;
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

                if (!_triggerables.Contains(triggerable))
                {
                    _triggerables.Add(triggerable);
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
                if (!_triggerables.Remove(triggerable))
                {
                    return Result.CreateFailure(
                        $"Trigger '{Name}' subscription was not found.");
                }

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

                RuntimeStatus = TriggerRuntimeStatus.Active;
                _executionCancellation = new CancellationTokenSource();
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

                _executionCancellation?.Cancel();
                RuntimeStatus = TriggerRuntimeStatus.Stopped;
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

                RuntimeStatus = executionResult.Success
                    ? TriggerRuntimeStatus.Completed
                    : TriggerRuntimeStatus.Faulted;
                _executionCancellation?.Dispose();
                _executionCancellation = null;
            }
        }

        protected async Task<List<Result>> ExecuteTriggerables()
        {
            ITriggerable[] subscribers;

            lock (_subscriptionLock)
            {
                RuntimeStatus = TriggerRuntimeStatus.Notifying;
                subscribers = _triggerables.ToArray();
            }

            List<Result> results = new();
            if (subscribers.Length == 0)
            {
                results.Add(Result.CreateFailure(
                    "No triggerable subscribers are registered."));
                return results;
            }

            Task<Result>[] executionTasks = subscribers.Select(async triggerable =>
            {
                try
                {
                    return await triggerable.OnTrigger();
                }
                catch (Exception exception)
                {
                    return Result.CreateFailure(
                        $"Trigger subscriber failed with exception: {exception.Message}",
                        exception);
                }
            }).ToArray();

            results.AddRange(await Task.WhenAll(executionTasks));
            return results;
        }

        internal IReadOnlyList<ITriggerable> GetTriggerables()
        {
            lock (_subscriptionLock)
            {
                return _triggerables.ToArray();
            }
        }

        public abstract Task<Result> InitTrigger();
    }
}
