using AgroEco.Core.Hadware;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Actions.Implementations
{
    public class Activarhadware : Action
    {
        public IActivable _hadwar;

        int nivelAgua = 0;


        public Activarhadware(string name, IActivable activable, int nivelagua) : base(name)
        {
            this.nivelAgua = nivelagua;

            _hadwar = activable;

        }

        public async override Task<Result> Execute()
        {
            var hwResult = await _hadwar.activar();
            if (!hwResult.Success)
                return Result.CreateFailure($"Failed to activate hardware in action '{Name}': {hwResult.Message}");

            Console.WriteLine("regando el agua" + nivelAgua);
            return Result.CreateSuccess($"Action '{Name}' executed - {hwResult.Message}");
        }
    }
}