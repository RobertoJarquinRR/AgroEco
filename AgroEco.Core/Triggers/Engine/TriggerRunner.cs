using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgroEco.Core.Triggers.Engine;

public sealed class TriggerRunner
{
    private readonly TriggerRegistry _registry;
    private readonly ILogger<TriggerRunner>? _logger;
    private readonly TriggerNotifier _notifier;

    public event Func<TriggerExecutionReport, Task>? ExecutionCompleted;

    public TriggerRunner(TriggerRegistry registry, ILogger<TriggerRunner>? logger = null)
    {
        _registry = registry;
        _logger = logger;
        _notifier = new TriggerNotifier(logger as ILogger<TriggerNotifier> ?? NullLogger<TriggerNotifier>.Instance);
    }

    public async Task RunAsync(int triggerId, Trigger trigger, CancellationToken cancellationToken)
    {
        trigger.RuntimeStatus = TriggerRuntimeStatus.Running;

        try
        {
            while (trigger.Enabled && !cancellationToken.IsCancellationRequested)
            {
                if (trigger.HasReachedMaxExecutions)
                {
                    trigger.Disable();
                    break;
                }

                Result ready = await trigger.WaitAsync(trigger.ExecutionToken);
                if (!ready.Success)
                {
                    _logger?.LogInformation(
                        "Trigger {TriggerId} stopped waiting: {Message}",
                        triggerId,
                        ready.Message);
                    if (trigger.Enabled)
                    {
                        trigger.Disable();
                    }
                    break;
                }

                if (!trigger.Enabled)
                {
                    break;
                }

                TriggerExecutionReport report;
                try
                {
                    report = await trigger.ExecuteReadyAsync();
                }
                catch (Exception exception)
                {
                    var errorResult = Result.CreateFailure(
                        $"Trigger '{trigger.Name}' failed during execution.",
                        exception);
                    report = new TriggerExecutionReport(
                        TriggerId: trigger.Id,
                        TriggerName: trigger.Name ?? string.Empty,
                        RuntimeStatus: TriggerRuntimeStatus.Running,
                        SubscriberReports: [],
                        OverallResult: errorResult);
                }

                if (ExecutionCompleted is not null)
                {
                    await _notifier.InvokeEventSafelyAsync(ExecutionCompleted, report);
                }

                Result registered = trigger.RegisterFiring(DateTimeOffset.UtcNow);
                if (!registered.Success)
                {
                    trigger.Disable();
                    break;
                }
            }
        }
        finally
        {
            trigger.RuntimeStatus = TriggerRuntimeStatus.Idle;
        }
    }
}