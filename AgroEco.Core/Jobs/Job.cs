using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs.Triggers;
using System.Security.Cryptography;

namespace AgroEco.Core.Jobs
{
    public class Job : ITriggerable, IEntity
    {       
        public int Id { get; private set; }

        public string Name { get; private set; }

        public string? Description { get; private set; }

        public int? Priority { get; private set; }

        public Status Status { get; private set; } 

        public DateTime? Date { get; private set; }

        public List<Action> Action { get; private set; } = new();

        public Trigger Trigger { get; private set; }


        public List<Result> result = new();
     
        Job(string name, Status status,string? description, int? priority, DateTime? date, List<Action> action, Trigger trigger)
        {
            
            Name = name;
            this.Status = status;
            Description = description;
            Priority = priority;
            Date = date;
            Action = action;
            Trigger = trigger;
            Trigger.AttachReceiver(this);
        }

        // contructor privado para el orm
        private Job() {
            Name = null!;
            Trigger = null!;
        }

        public static async Task<Result<Job>> CreateJob(
        string name,
        string? description,
        Status status,
        int? priority,
        DateTime? date,
        List<Action>action,
        Trigger trigger)
        {
      
            if (string.IsNullOrEmpty(name))
            {
                return Result<Job>.CreateFailure("Name can't be empty");
            }
            Job t = new( name, status, description, priority, date, action, trigger);

            if (status == Status.Running)
            {
                await t.Ontrigger();
            }

            return Result<Job>.CreateSuccess(t, "job create successfully");

        }
        public async Task<Result> Ontrigger()
        {   
            
            Console.WriteLine("Ejecutando el Job...");

            if(Status == Status.Succeeded)
            {
                return Result.CreateSuccess($"Job {Name} executes successfully");
            }

            foreach(Action action in Action){
                if(action.Status == Status.Succeeded || action.Status == Status.Canceled)
                {
                    continue;
                }
                result.Add(await action.Execute());

                action.ChangeStatus(Status.Running);
                
            }
            ChangeStatus(Status.Succeeded);

            return Result.CreateSuccess($"Job {Name} executes successfully");
        }

        public void ChangeStatus(Status status)
        {
            Status = status;
        }

      
    }

}

