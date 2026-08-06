using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Actions
{
    public class actionTest : IJobAction
    {
        public async Task<Result> Execute()
        {
            Console.WriteLine("ejecutando la accion");
            return Result.CreateSuccess("tarea de practica");
            
            
        }
    }
}
