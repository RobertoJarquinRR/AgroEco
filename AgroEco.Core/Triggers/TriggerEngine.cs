using AgroEco.Core.Triggers.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace AgroEco.Core.Triggers;

public sealed class TriggerEngine
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Registry<int, Trigger> _registry = new();
    private readonly SemaphoreSlim _registryLock = new(1, 1);

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
                if ((registeredTrigger.RuntimeStatus is TriggerRuntimeStatus.Completed
                    or TriggerRuntimeStatus.Stopped
                    or TriggerRuntimeStatus.Faulted)
                    && !registeredTrigger.HasTriggerables)
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

    public IEnumerable<Trigger> GetActiveTriggers()
        => _registry.Where(trigger =>
            trigger.RuntimeStatus is TriggerRuntimeStatus.Active
                or TriggerRuntimeStatus.Notifying);

    private async Task ExecuteAsync(int triggerId, Trigger trigger)
    {
        Result executionResult;

        try
        {
            executionResult = await trigger.InitTrigger();
        }
        catch (Exception exception)
        {
            executionResult = Result.CreateFailure(
                $"Trigger '{trigger.Name}' failed during execution.",
                exception);
        }

        trigger.CompleteExecution(executionResult);
        RemoveIfInactive(triggerId, trigger);
    }

    private void RemoveIfInactive(int triggerId, Trigger trigger)
    {
        if (trigger.HasTriggerables
            || trigger.RuntimeStatus is TriggerRuntimeStatus.Notifying)
        {
            return;
        }

        if (trigger.RuntimeStatus == TriggerRuntimeStatus.Active)
        {
            trigger.StopExecution();
        }

        _registry.Unregister(triggerId);
    }
}
