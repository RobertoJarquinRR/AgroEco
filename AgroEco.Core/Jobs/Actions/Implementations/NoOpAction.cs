using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Actions.Implementations
{
    public class NoOpAction : Action
    {

        public NoOpAction(string name) : base(name)
        {
        }

       
        public override Task<Result> Execute()
        {
            
            return Task.FromResult(
                Result.CreateSuccess("tarea de practica"));
        }

        
    }
}
