using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Triggers
{
    public abstract class Trigger :IEntity
    {
        protected List<ITriggerable> _Triggerables = new();
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
            _Triggerables.Add(triggerable);    
        }

        protected async Task<List<Result>> ExecuteTriggerables(){
            List<Result> result = new();
            if(_Triggerables == null || _Triggerables.Count == 0){
                result.Add(Result.CreateFailure("There not job to execute"));
                return result;
            }     
            foreach(ITriggerable T in _Triggerables){
                result.Add(await T.Ontrigger());
                
            }
            return result;

        }

        public abstract Task<Result> InitTrigger(); 


    }
}
