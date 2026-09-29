using AgroEco.Core.Jobs.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Alerts
{
    public class Alert : ITriggerable
    {

        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Message { get; set; }

        private Alert()
        { }

        

        public Task<Result> OnTrigger()
        {
            throw new NotImplementedException();
        }
    }
}
