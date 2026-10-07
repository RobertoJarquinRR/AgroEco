namespace AgroEco.Core.Triggers.Engine;

public sealed class TriggerRunner
{
    private readonly TriggerRegistry _registry;

    public event Func<TriggerExecutionReport, Task>? ExecutionCompleted;

    public TriggerRunner(TriggerRegistry registry)
    {
        _registry = registry;
    }

    public async Task ExecuteAsync(int triggerId, Trigger trigger)
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

        bool isCanceled = report.OverallResult.Message?.Contains("canceled", StringComparison.OrdinalIgnoreCase) == true;
        if (!trigger.LastExecutionWasStopped
            && !isCanceled
            && report.RuntimeStatus != TriggerRuntimeStatus.Stopped
            && trigger.RuntimeStatus != TriggerRuntimeStatus.Stopped
            && ExecutionCompleted is not null)
        {
            await TriggerNotifier.InvokeEventSafelyAsync(ExecutionCompleted, report);
        }

        RemoveIfInactive(triggerId, trigger);
    }

    public void RemoveIfInactive(int triggerId, Trigger trigger)
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
}