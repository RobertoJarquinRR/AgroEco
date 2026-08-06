using AgroEco.Core.Jobs.Triggers;
using System.Security.Cryptography;

namespace AgroEco.Core.Jobs
{
    public class Job
    {       
        public Guid ID { get; private set; }

        public string Name { get; private set; }

        public string? Description { get; private set; }

        public int? Priority { get; private set; } = 1;

        public Status Status { get; private set; } 

        public DateTime? Date { get; private set; } = DateTime.Now;

        public IJobAction Action { get; private set; }

        public ITrigger Trigger { get; private set; }

        Job(Guid id, string name, string? description, int? priority, DateTime? date, IJobAction action, ITrigger trigger)
        {
            ID = id;
            Name = name;
            Description = description;
            Priority = priority;
            Date = date;
            Action = action;
            Trigger = trigger;

            Trigger.OnTriggerFired += async () => await Execute();
        }

        public static Result<Job> CreateJob(
        Guid id,
        string name,
        string? description,
        int? priority,
        DateTime? date,
        IJobAction action,
        ITrigger trigger)
        {
            
            if (string.IsNullOrEmpty(name))
            {
                return Result<Job>.CreateFailure("Name can't be empty");
            }

            
            Job t = new(id, name, description, priority, date, action, trigger);


            return Result<Job>.CreateSuccess(t);
        }

        public async Task<Result> Execute(){


            Console.WriteLine("Ejecutando el Job...");
            return await Action.Execute();

        }

      
    }

}

