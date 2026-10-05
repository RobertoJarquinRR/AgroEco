using System.Linq;
using AgroEco.Core.Jobs.Persistence;
using AgroEco.Core.Triggers;
using Microsoft.Extensions.DependencyInjection;

namespace AgroEco.Core.Jobs
{
    public class JobEngine 
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TriggerEngine _triggerEngine;

        public JobEngine(
            IServiceScopeFactory scopeFactory,
            TriggerEngine triggerEngine)
        {
            _scopeFactory = scopeFactory;
            _triggerEngine = triggerEngine;
            _triggerEngine.TriggerExecutionCompleted += OnTriggerExecutionCompletedAsync;
        }

        public event Action<Job>? JobExecutionCompleted;

        private async Task OnTriggerExecutionCompletedAsync(
            Trigger trigger,
            Result executionResult)
        {
            await PersistCompletedJobsAsync(trigger, executionResult);
        }

        private async Task PersistCompletedJobsAsync(
            Trigger trigger,
            Result executionResult)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            UpdateJob updateJob = scope.ServiceProvider
                .GetRequiredService<UpdateJob>();

            foreach (Job job in trigger.GetTriggerables().OfType<Job>())
            {
                if (!job.IsCompleted && !executionResult.Success)
                {
                    job.Results.Add(executionResult);
                    Result faultResult = job.ChangeStatus(Status.Faulted);
                    if (!faultResult.Success)
                    {
                        return;
                    }
                }

                Result persistenceResult = await updateJob.HandleAsync(job);
                if (persistenceResult.Success)
                {
                    JobExecutionCompleted?.Invoke(job);
                }
            }
        }

        public async Task<Result> Init(
            CancellationToken cancellationToken = default)
        {
            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                GetRunningJobs getRunningJobs = scope.ServiceProvider
                    .GetRequiredService<GetRunningJobs>();
                UpdateJob updateJob = scope.ServiceProvider
                    .GetRequiredService<UpdateJob>();
                var result = await getRunningJobs.HandleAsync(cancellationToken);

                if (result.Value == null || result.Value.Count == 0)
                {
                    return Result.CreateSuccess($"No running jobs: {result.Message} ");
                }

                foreach (Job j in result.Value)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await RebuildInMemoryState(j, updateJob, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                return Result.CreateFailure("Job engine initialization was canceled.");
            }
            catch (Exception ex)
            {
                return Result.CreateFailure($"Error initializing job engine: {ex.Message}");
            }

            return Result.CreateSuccess("Job engine initialized successfully");
        }

        public async Task<Result> RunJob(
            int id,
            CancellationToken cancellationToken = default)
        {
           
            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                GetByIdJobWithDetails getByIdJob = scope.ServiceProvider
                    .GetRequiredService<GetByIdJobWithDetails>();
                UpdateJob updateJob = scope.ServiceProvider
                    .GetRequiredService<UpdateJob>();
                var result = await getByIdJob.HandleAsync(id, cancellationToken);

                if (result.Value == null)
                {
                    return Result.CreateFailure($"No job found with id: {id}");
                }

                var (rehydrateResult, triggerResult) = await RebuildInMemoryState(
                    result.Value,
                    updateJob,
                    cancellationToken);

                string detailMessage = string.Join("; ", new[] { result.Message, rehydrateResult.Message, triggerResult.Message }
                    .Where(m => !string.IsNullOrEmpty(m)));

                if (!rehydrateResult.Success || !triggerResult.Success)
                {
                    return Result.CreateFailure(
                        $"Job '{result.Value.Name}' could not start: {detailMessage}");
                }

                return Result.CreateSuccess($"Job '{result.Value.Name}' running: {detailMessage}");
            }
            catch (OperationCanceledException)
            {
                return Result.CreateFailure($"Running job {id} was canceled.");
            }
            catch (Exception ex)
            {
                return Result.CreateFailure($"Error running job {id}: {ex.Message}");
            }
        }

        private async Task<(Result rehydrateResult, Result triggerResult)> RebuildInMemoryState(
            Job job,
            UpdateJob updateJob,
            CancellationToken cancellationToken)
        {
            Result rehydrateResult = job.Rehydrate();
            if (!rehydrateResult.Success)
            {
                return (rehydrateResult, Result.CreateFailure(
                    "Trigger subscription was not started."));
            }

            Result subscribeResult = await _triggerEngine.SubscribeAsync(
                job.Trigger.Id,
                job,
                cancellationToken);

            if (!subscribeResult.Success)
            {
                return (rehydrateResult, subscribeResult);
            }

            if (job.Status != Status.Running)
            {
                Result runningResult = job.ChangeStatus(Status.Running);
                if (!runningResult.Success)
                {
                    return (rehydrateResult, runningResult);
                }

                Result persistenceResult = await updateJob.HandleAsync(
                    job,
                    cancellationToken);
                if (!persistenceResult.Success)
                {
                    return (rehydrateResult, persistenceResult);
                }
            }

            Result triggerResult = await _triggerEngine.StartAsync(
                job.Trigger.Id,
                cancellationToken);

            return (rehydrateResult, triggerResult);
        }
    }
}