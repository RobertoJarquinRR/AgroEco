
using System.Linq;
using AgroEco.Core.Jobs.Persistence;

namespace AgroEco.Core.Jobs
{
    public class JobEngine  
    {


        private readonly GetRunningJobs _getRunningJobs;
        private readonly GetByIdJob _getByIdJob;



        public JobEngine(GetRunningJobs getRunningJobs, GetByIdJob getByIdJob){
            _getRunningJobs = getRunningJobs;
            _getByIdJob = getByIdJob;

            
            
        }

        public async Task<Result> Init(){
            
        
        try 
        {
                var result = await _getRunningJobs.HandleAsync();

                if (result.Value == null || result.Value.Count == 0)
                {
                    return Result.CreateSuccess($"No running jobs: {result.Message} ");
                }
                
                foreach(Job j in result.Value){

                    j.Rehydrate();
                    await j.Trigger.InitTrigger();
                }
                

            }

            catch (Exception ex){

                return Result.CreateFailure($"Error initializing job engine: {ex.Message}");
            }
            
            return Result.CreateSuccess("Job engine initialized successfully");
        }

        public async Task<Result> RunJob(int id){
            
            try{
                var result = await _getByIdJob.HandleAsync(id);

                if(result.Value == null){
                    return Result.CreateFailure($"No job found with id: {id}");
                };

                var rehydrateResult = result.Value.Rehydrate();
                var triggerResult = await result.Value.Trigger.InitTrigger();

                string mensajeDetalle = string.Join("; ", new[] { result.Message, rehydrateResult.Message, triggerResult.Message }
                    .Where(m => !string.IsNullOrEmpty(m)));

                return Result.CreateSuccess($"Job '{result.Value.Name}' running: {mensajeDetalle}");
            }
            catch(Exception ex){
                return Result.CreateFailure($"Error running job {id}: {ex.Message}");
            }
        }
    }
}
