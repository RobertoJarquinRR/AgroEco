using System.Linq;
using AgroEco.Core.Jobs.Persistence;
using AgroEco.Core.Triggers;

namespace AgroEco.Core.Jobs
{
    public class JobEngine 
    {
        private readonly GetRunningJobs _getRunningJobs;
        private readonly GetByIdJobWithDetails _getByIdJob;
        private readonly TriggerEngine _triggerEngine;

        public JobEngine(
            GetRunningJobs getRunningJobs,
            GetByIdJobWithDetails getByIdJob,
            TriggerEngine triggerEngine)
        {
            _getRunningJobs = getRunningJobs;
            _getByIdJob = getByIdJob;
            _triggerEngine = triggerEngine;

        }

        public async Task<Result> Init()
        {
            try
            {
                var result = await _getRunningJobs.HandleAsync();

                if (result.Value == null || result.Value.Count == 0)
                {
                    return Result.CreateSuccess($"No running jobs: {result.Message} ");
                }

                foreach (Job j in result.Value)
                {
                    await RebuildInMemoryState(j);
                }
            }
            catch (Exception ex)
            {
                return Result.CreateFailure($"Error initializing job engine: {ex.Message}");
            }

            return Result.CreateSuccess("Job engine initialized successfully");
        }

        public async Task<Result> RunJob(int id)
        {
           
            try
            {
                var result = await _getByIdJob.HandleAsync(id);

                if (result.Value == null)
                {
                    return Result.CreateFailure($"No job found with id: {id}");
                }

                var (rehydrateResult, triggerResult) = await RebuildInMemoryState(result.Value);

                string detailMessage = string.Join("; ", new[] { result.Message, rehydrateResult.Message, triggerResult.Message }
                    .Where(m => !string.IsNullOrEmpty(m)));

                return Result.CreateSuccess($"Job '{result.Value.Name}' running: {detailMessage}");
            }
            catch (Exception ex)
            {
                return Result.CreateFailure($"Error running job {id}: {ex.Message}");
            }
        }

        private async Task<(Result rehydrateResult, Result triggerResult)> RebuildInMemoryState(Job job)
        {
            Result rehydrateResult = job.Rehydrate();
            if (!rehydrateResult.Success)
            {
                return (rehydrateResult, Result.CreateFailure(
                    "Trigger subscription was not started."));
            }

            Result subscribeResult = await _triggerEngine.SubscribeAsync(
                job.Trigger.Id,
                job);

            if (!subscribeResult.Success)
            {
                return (rehydrateResult, subscribeResult);
            }

            Result triggerResult = await _triggerEngine.StartAsync(job.Trigger.Id);

            return (rehydrateResult, triggerResult);
        }
    }
}