using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Actions.Implementations
{
    public class actionTest : Action
    {

        public actionTest( string name, Status status) : base(name)
        {
        }

       
        public override async Task<Result> Execute()
        {
            Console.WriteLine("ejecutando la accion");
            return Result.CreateSuccess("tarea de practica");
        }

        
    }
}
