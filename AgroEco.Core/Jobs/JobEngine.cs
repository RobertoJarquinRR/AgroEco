using System.Linq;
using AgroEco.Core.Jobs.Persistence;

namespace AgroEco.Core.Jobs
{
    public class JobEngine 
    {
        private readonly GetRunningJobs _getRunningJobs;
        private readonly GetByIdJobWithDetails _getByIdJob;


        private readonly Registry<int, Job> registry = new();

        public JobEngine(GetRunningJobs getRunningJobs, GetByIdJobWithDetails getByIdJob)
        {
            _getRunningJobs = getRunningJobs;
            _getByIdJob = getByIdJob;

           

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

                string mensajeDetalle = string.Join("; ", new[] { result.Message, rehydrateResult.Message, triggerResult.Message }
                    .Where(m => !string.IsNullOrEmpty(m)));

                return Result.CreateSuccess($"Job '{result.Value.Name}' running: {mensajeDetalle}");
            }
            catch (Exception ex)
            {
                return Result.CreateFailure($"Error running job {id}: {ex.Message}");
            }
        }

        private async Task<(Result rehydrateResult, Result triggerResult)> RebuildInMemoryState(Job job)
        {
            var rehydrateResult = job.Rehydrate();
            registry.Register(job.Id, job);
            var triggerResult = await job.Trigger.InitTrigger();

            return (rehydrateResult, triggerResult);
        }
    }
}