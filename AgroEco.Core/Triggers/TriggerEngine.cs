using AgroEco.Core.Triggers.Engine;
using AgroEco.Core.Triggers.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgroEco.Core.Triggers;

public sealed class TriggerEngine
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TriggerRegistry _registry;
    private readonly TriggerRunner _runner;
    private readonly ILogger<TriggerEngine>? _logger;
    private readonly CancellationTokenSource _runCancellation = new();

    public event Func<TriggerRuntimeChange, Task>? RuntimeStatusChanged;
    public event Func<TriggerExecutionReport, Task>? ExecutionCompleted;

    public TriggerEngine(IServiceScopeFactory scopeFactory, ILogger<TriggerEngine>? logger = null)
    {
        _scopeFactory = scopeFactory;
        _logger = logger ?? NullLogger<TriggerEngine>.Instance;
        _registry = new TriggerRegistry();
        var runnerLogger = _logger as ILogger<TriggerRunner> ?? NullLogger<TriggerRunner>.Instance;
        var notifierLogger = _logger as ILogger<TriggerNotifier> ?? NullLogger<TriggerNotifier>.Instance;
        _runner = new TriggerRunner(_registry, runnerLogger);
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

        return triggerResult.Value.Unsubscribe(triggerable);
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
        if (trigger.RuntimeStatus == TriggerRuntimeStatus.Running)
        {
            return Result.CreateSuccess(
                $"Trigger '{trigger.Name}' is already running.");
        }

        Result enableResult = trigger.Enable();
        if (!enableResult.Success)
        {
            return enableResult;
        }

        var task = Task.Run(() => _runner.RunAsync(triggerId, trigger, _runCancellation.Token), cancellationToken);
        _ = task.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                _logger?.LogError(t.Exception, "Trigger runner faulted for trigger {TriggerId}", triggerId);
            }
        }, TaskScheduler.Default);
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

        return triggerResult.Value.Disable();
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
            trigger.RuntimeStatus == TriggerRuntimeStatus.Running);

    private void SubscribeToTriggerEvents(Trigger trigger)
    {
        trigger.RuntimeStatusChanged += OnTriggerRuntimeStatusChangedAsync;
    }

    private async Task OnTriggerRuntimeStatusChangedAsync(TriggerRuntimeChange change)
    {
        if (RuntimeStatusChanged is not null)
        {
            var notifierLogger = _logger as ILogger<TriggerNotifier> ?? NullLogger<TriggerNotifier>.Instance;
            var notifier = new TriggerNotifier(notifierLogger);
            await notifier.InvokeEventSafelyAsync(RuntimeStatusChanged, change);
        }
    }

    private async Task OnExecutionCompletedAsync(TriggerExecutionReport report)
    {
        if (ExecutionCompleted is not null)
        {
            var notifierLogger = _logger as ILogger<TriggerNotifier> ?? NullLogger<TriggerNotifier>.Instance;
            var notifier = new TriggerNotifier(notifierLogger);
            await notifier.InvokeEventSafelyAsync(ExecutionCompleted, report);
        }
    }
}