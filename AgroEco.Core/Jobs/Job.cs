using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Triggers;
using Action = AgroEco.Core.Jobs.Actions.Action;
using System.Linq;

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

        public List<Action> Actions { get; private set; } = new();

        public Trigger Trigger { get; private set; }


        public List<Result> Results { get; private set; } = new();
     
        Job(string name, Status status,string? description, int? priority, DateTime? date, List<Action> action, Trigger trigger)
        {
            
            Name = name;
            this.Status = status;
            Description = description;
            Priority = priority;
            Date = date;
            Actions = action;
            Trigger = trigger;
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
       
        List<Action> actions,
        Trigger trigger)
        {
      
            if (string.IsNullOrEmpty(name))
            {
                return Result<Job>.CreateFailure("Name can't be empty");
            }
            Job t = new(name, status, description, priority, DateTime.Now, actions, trigger);


            return Result<Job>.CreateSuccess(t, "job create successfully");

        }

        public Result UpdateDetails(
            string name,
            string? description,
            int? priority,
            DateTime? date)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.CreateFailure("Name can't be empty");
            }

            Name = name.Trim();
            Description = description;
            Priority = priority;
            Date = date;
            return Result.CreateSuccess();
        }

        public async Task<Result> OnTrigger()
        {   
            
            Console.WriteLine("Ejecutando el Job...");

            if(Status == Status.Succeeded)
            {
                return Result.CreateSuccess($"Job {Name} executes successfully");
            }

            Result runningResult = ChangeStatus(Status.Running);
            if (!runningResult.Success)
            {
                return runningResult;
            }

            foreach (Action action in Actions)
            {
                if(action.Status == Status.Succeeded || action.Status == Status.Canceled)
                {
                    continue;
                }
                Result startResult = action.ChangeStatus(Status.Running);
                if (!startResult.Success)
                {
                    Results.Add(startResult);
                    continue;
                }

                Result actionResult;
                try
                {
                    actionResult = await action.Execute();
                }
                catch (Exception exception)
                {
                    actionResult = Result.CreateFailure(
                        $"Action '{action.Name}' failed with an exception.",
                        exception);
                }

                Results.Add(actionResult);
                action.ChangeStatus(
                    actionResult.Success ? Status.Succeeded : Status.Faulted);
            }
            Result completionResult = ChangeStatus(Status.Succeeded);
            if (!completionResult.Success)
            {
                return completionResult;
            }

            var messages = Results
                .Select(r => r.Message)
                .Where(m => !string.IsNullOrEmpty(m));
            string actionSummary = string.Join("; ", messages);

            return Result.CreateSuccess(
                $"Job '{Name}' executed successfully. Action results: {actionSummary}");
        }

        public Result ChangeStatus(Status status)
        {
            bool validTransition = Status switch
            {
                Status.Created => status is Status.Enqueued or Status.Running or Status.Canceled,
                Status.Enqueued => status is Status.Running or Status.Canceled,
                Status.Running => status is Status.Succeeded or Status.Faulted or Status.Canceled,
                Status.Succeeded or Status.Faulted or Status.Canceled => false,
                _ => false
            };

            if (!validTransition)
            {
                return Result.CreateFailure(
                    $"Job '{Name}' cannot transition from {Status} to {status}.");
            }

            Status = status;
            return Result.CreateSuccess();
        }

        public Result Rehydrate(){
            
            try{
                if(Trigger == null)
                {
                    return Result.CreateFailure(
                        $"Job '{Name}' has no trigger definition.");

                }
                return Result.CreateSuccess($"Job '{Name}' rehydrated successfully.");
            }
            catch (Exception exception)
            {
                return Result.CreateFailure(
                    $"Job '{Name}' could not be rehydrated.",
                    exception);
            }
        }



    }

}
