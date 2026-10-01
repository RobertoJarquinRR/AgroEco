using AgroEco.Core;
using AgroEco.Core.Hardware;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Hardware
{
    public class ServoMotor : IActivable
    {
       
        public double Degrees;

        public ServoMotor(double degrees)
        {
            Degrees = degrees;
        }

        public Task<Result> Activate()
        {
            Console.WriteLine("Servomotor moving to " + Degrees + " degrees");
            return Task.FromResult(
                Result.CreateSuccess($"Servomotor moved to {Degrees} degrees"));
        }


    }
}
