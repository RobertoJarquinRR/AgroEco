using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Triggers
{
    public abstract class Trigger :IEntity
    {
        public List<ITriggerable> Triggerables { get; private set; } = new List<ITriggerable>();
        public int Id { get; private set; }

        public string? Name { get; private set; }

        protected Trigger(string name ){
            if(string.IsNullOrEmpty(name)){
                return;
            }
            ;
            Name = name;
            
        }

        public void AttachReceiver(ITriggerable triggerable)
        {
            Triggerables.Add(triggerable);    
        }

        protected async Task<List<Result>> ExecuteTriggerables(){
            List<Result> results = new();
       

            if (Triggerables == null || Triggerables.Count == 0){
                results.Add(Result.CreateFailure("No jobs attached to trigger to execute"));
                return results;
            }
            var executionTasks = Triggerables.Select(async t =>
            {
                try
                {
                    return await t.OnTrigger();
                }
                catch (Exception ex)
                {
                    return Result.CreateFailure($"Trigger failed with exception: {ex.Message}");
                }
            }).ToArray();

            Result[] resolvedResults = await Task.WhenAll(executionTasks);

            results.AddRange(resolvedResults);
            return results;

        }

        public abstract Task<Result> InitTrigger(); 


    }
}

