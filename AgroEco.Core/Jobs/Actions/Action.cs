using AgroEco.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs
{
    public abstract class Action : IEntity
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public Status Status { get; private set; }
        public int JobId { get; private set; }

        protected Action(string name){ 
            Name = name;
            if(Status == 0){
                Status = Status.Created;
            }
        }

        public abstract Task<Result> Execute();

        public void ChangeStatus(Status status)
        {

            Status = status;

           
        }


    }
}
