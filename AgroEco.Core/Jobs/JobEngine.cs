using System.Linq;
using System.Collections.Concurrent;
using AgroEco.Core.Jobs.Persistence;
using AgroEco.Core.Jobs.Runs;
using AgroEco.Core.Jobs.Runs.Persistence;
using AgroEco.Core.Jobs.Engine;
using AgroEco.Core.Triggers;
using AgroEco.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AgroEco.Core.Jobs
{
    public class JobEngine 
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TriggerEngine _triggerEngine;
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, Job>> _subscribedJobs = new();

        public JobEngine(
            IServiceScopeFactory scopeFactory,
            TriggerEngine triggerEngine)
        {
            _scopeFactory = scopeFactory;
            _triggerEngine = triggerEngine;
            _triggerEngine.ExecutionCompleted += OnTriggerExecutionCompletedAsync;
        }

        public event Action<Job>? JobExecutionCompleted;

        private async Task OnTriggerExecutionCompletedAsync(
            TriggerExecutionReport report)
        {
            await PersistCompletedJobsAsync(report);
        }

        private async Task PersistCompletedJobsAsync(
            TriggerExecutionReport report)
        {
            if (!_subscribedJobs.TryGetValue(report.TriggerId, out var jobsById))
            {
                return;
            }

            using IServiceScope scope = _scopeFactory.CreateScope();
            UpdateJob updateJob = scope.ServiceProvider
                .GetRequiredService<UpdateJob>();
            CreateJobRun createJobRun = scope.ServiceProvider
                .GetRequiredService<CreateJobRun>();
            var jobRunner = new JobRunner();

            foreach (var subscriberReport in report.SubscriberReports)
            {
                if (subscriberReport.SubscriberType != nameof(Job))
                {
                    continue;
                }

                var jobId = subscriberReport.SubscriberId;
                if (jobId is null || !jobsById.TryGetValue(jobId.Value, out Job? job))
                {
                    continue;
                }

                var jobRun = await jobRunner.RunAsync(job, report.TriggerId, TriggeredBy.Schedule);
                var createRunResult = await createJobRun.HandleAsync(jobRun);
                if (!createRunResult.Success)
                {
                    continue;
                }

                var updateResult = await updateJob.HandleAsync(job);
                if (updateResult.Success)
                {
                    JobExecutionCompleted?.Invoke(job);
                }
            }

            // Clean up completed jobs
            var completedJobIds = jobsById
                .Where(kvp => kvp.Value.IsCompleted)
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var id in completedJobIds)
            {
                jobsById.TryRemove(id, out _);
            }
            if (jobsById.IsEmpty)
            {
                _subscribedJobs.TryRemove(report.TriggerId, out _);
            }
        }

        internal void TrackSubscribedJob(int triggerId, Job job)
        {
            var jobsById = _subscribedJobs.GetOrAdd(triggerId, _ => new ConcurrentDictionary<int, Job>());
            jobsById.TryAdd(job.Id, job);
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

            TrackSubscribedJob(job.Trigger.Id, job);

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