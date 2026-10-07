using AgroEco.Core.Triggers.Engine;
using AgroEco.Core.Triggers.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace AgroEco.Core.Triggers;

public sealed class TriggerEngine
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TriggerRegistry _registry;
    private readonly TriggerRunner _runner;

    public event Func<TriggerRuntimeChange, Task>? RuntimeStatusChanged;
    public event Func<TriggerExecutionReport, Task>? ExecutionCompleted;

    public TriggerEngine(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _registry = new TriggerRegistry();
        _runner = new TriggerRunner(_registry);
        _runner.ExecutionCompleted += OnExecutionCompletedAsync;
    }

    public async Task<Result<Trigger>> GetOrCreateAsync(
        int triggerId,
        CancellationToken cancellationToken = default)
    {
        Trigger? reusableTrigger = await _registry.TryGetReusableAsync(triggerId, cancellationToken);
        if (reusableTrigger is not null)
        {
            return Result<Trigger>.CreateSuccess(reusableTrigger);
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
        _runner.RemoveIfInactive(triggerId, triggerResult.Value);
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

        _ = _runner.ExecuteAsync(triggerId, trigger);
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
        _runner.RemoveIfInactive(triggerId, triggerResult.Value);
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

    private void SubscribeToTriggerEvents(Trigger trigger)
    {
        trigger.RuntimeStatusChanged += OnTriggerRuntimeStatusChangedAsync;
    }

    private async Task OnTriggerRuntimeStatusChangedAsync(TriggerRuntimeChange change)
    {
        if (RuntimeStatusChanged is not null)
        {
            await TriggerNotifier.InvokeEventSafelyAsync(RuntimeStatusChanged, change);
        }
    }

    private async Task OnExecutionCompletedAsync(TriggerExecutionReport report)
    {
        if (ExecutionCompleted is not null)
        {
            await TriggerNotifier.InvokeEventSafelyAsync(ExecutionCompleted, report);
        }
    }
}