using AgroEco.Core;
using AgroEco.Core.Hardware;

namespace AgroEco.Hardware
{
    public class Sensors : IActivable
    {
        

        public Task<Result> Activate()
        {
            Console.WriteLine("Sensor activado");
            
            return Task.FromResult(
                Result.CreateSuccess("Sensor activated successfully"));
        }
    }
}
