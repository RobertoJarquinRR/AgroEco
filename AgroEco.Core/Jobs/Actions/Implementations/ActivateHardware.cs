using AgroEco.Core.Hardware;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Actions.Implementations
{
    public class ActivateHardware : Action
    {
        private readonly IActivable _hardware;

        private readonly int _waterLevel;


        public ActivateHardware(string name, IActivable activable, int waterLevel) : base(name)
        {
            _waterLevel = waterLevel;
            _hardware = activable;

        }

        public override async Task<Result> Execute()
        {
            var hardwareResult = await _hardware.Activate();
            if (!hardwareResult.Success)
                return Result.CreateFailure($"Failed to activate hardware in action '{Name}': {hardwareResult.Message}");

            Console.WriteLine($"Watering level: {_waterLevel}");
            return Result.CreateSuccess($"Action '{Name}' executed - {hardwareResult.Message}");
        }
    }
}