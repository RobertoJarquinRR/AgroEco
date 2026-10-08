using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Jobs.Runs;
using Action = AgroEco.Core.Jobs.Actions.Action;

namespace AgroEco.Core.Jobs.Engine;

public sealed class JobRunner
{
    public async Task<JobRun> RunAsync(Job job, int? triggerId, TriggeredBy triggeredBy)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var run = new JobRun
        {
            JobId = job.Id,
            TriggerId = triggerId,
            TriggeredBy = triggeredBy,
            StartedAt = startedAt,
            Actions = []
        };

        var prepareResult = job.PrepareForRun();
        if (!prepareResult.Success)
        {
            run.FinishedAt = DateTimeOffset.UtcNow;
            run.Status = Status.Faulted;
            run.Message = prepareResult.Message;
            run.Error = prepareResult.Message;
            return run;
        }

        var runningResult = job.ChangeStatus(Status.Running);
        if (!runningResult.Success)
        {
            run.FinishedAt = DateTimeOffset.UtcNow;
            run.Status = Status.Faulted;
            run.Message = runningResult.Message;
            run.Error = runningResult.Message;
            return run;
        }

        int successfulActions = 0;
        int failedActions = 0;

        foreach (Action action in job.Actions)
        {
            var actionStartedAt = DateTimeOffset.UtcNow;
            var runAction = new JobRunAction
            {
                ActionId = action.Id,
                ActionName = action.Name,
                ActionType = action.GetType().Name,
                Status = Status.Running
            };
            run.Actions.Add(runAction);

            var startResult = action.ChangeStatus(Status.Running);
            if (!startResult.Success)
            {
                var durationMs = (long)(DateTimeOffset.UtcNow - actionStartedAt).TotalMilliseconds;
                runAction.DurationMs = durationMs;
                runAction.Status = Status.Faulted;
                runAction.Message = startResult.Message;
                runAction.Error = startResult.Message;
                failedActions++;
                continue;
            }

            Result actionResult;
            try
            {
                actionResult = await action.Execute();
            }
            catch (Exception exception)
            {
                actionResult = Result.CreateFailure(
                    $"Action '{action.Name}' failed with an exception.",
                    exception);
            }

            var durationMs2 = (long)(DateTimeOffset.UtcNow - actionStartedAt).TotalMilliseconds;
            runAction.DurationMs = durationMs2;

            if (actionResult.Success)
            {
                var completionResult = action.ChangeStatus(Status.Succeeded);
                runAction.Status = completionResult.Success ? Status.Succeeded : Status.Faulted;
                runAction.Message = actionResult.Message;
                if (!completionResult.Success)
                {
                    runAction.Error = completionResult.Message;
                }
            }
            else
            {
                var completionResult = action.ChangeStatus(Status.Faulted);
                runAction.Status = completionResult.Success ? Status.Faulted : Status.Faulted;
                runAction.Message = actionResult.Message;
                runAction.Error = actionResult.Message;
            }

            if (runAction.Status == Status.Succeeded)
            {
                successfulActions++;
            }
            else
            {
                failedActions++;
            }
        }

        run.FinishedAt = DateTimeOffset.UtcNow;

        if (successfulActions == job.Actions.Count)
        {
            run.Status = Status.Succeeded;
        }
        else if (successfulActions > 0)
        {
            run.Status = Status.CompletedWithErrors;
        }
        else
        {
            run.Status = Status.Faulted;
        }

        var completionResult2 = job.ChangeStatus(run.Status);
        if (!completionResult2.Success)
        {
            run.Status = Status.Faulted;
            run.Error = completionResult2.Message;
        }

        var messages = run.Actions
            .Select(a => a.Message)
            .Where(m => !string.IsNullOrEmpty(m));
        var actionSummary = string.Join("; ", messages);

        run.Message = $"Job '{job.Name}' executed with status {run.Status}. " +
            $"Successful actions: {successfulActions}; failed actions: {failedActions}. " +
            $"Action results: {actionSummary}";

        if (run.Status != Status.Succeeded)
        {
            run.Error = run.Message;
        }

        return run;
    }
}