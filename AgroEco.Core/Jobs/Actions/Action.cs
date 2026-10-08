using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs.Actions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Actions
{
    public abstract class Action : IEntity
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public Status Status { get; private set; }
        public int JobId { get; private set; }
        public ActionConfiguration? Configuration { get; set; }

        protected Action(string name){ 
            Name = name;
            if(Status == 0){
                Status = Status.Created;
            }
        }

        public abstract Task<Result> Execute();

        internal virtual void AttachServices(IServiceProvider services)
        {
        }

        public Result ChangeStatus(Status status)
        {
            if (Status == status)
            {
                return Result.CreateSuccess();
            }

            if (!IsValidTransition(Status, status))
            {
                return Result.CreateFailure(
                    $"Action '{Name}' cannot transition from {Status} to {status}.");
            }

            Status = status;
            return Result.CreateSuccess();
        }

        private static bool IsValidTransition(Status current, Status next)
        {
            return current switch
            {
                Status.Created => next is Status.Enqueued or Status.Running or Status.Canceled,
                Status.Enqueued => next is Status.Running or Status.Canceled,
                Status.Running => next is Status.Succeeded or Status.Faulted or Status.Canceled,
                Status.Succeeded or Status.Faulted or Status.Canceled => next is Status.Created,
                _ => false
            };
        }

    }
}
