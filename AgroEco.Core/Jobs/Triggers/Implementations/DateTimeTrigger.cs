using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Triggers.Implementations
{
    public class DateTimeTrigger : Trigger
    {

        public DateTimeTrigger(string name):
        base( name)
        {
        

        }

        
        public override async Task<Result> InitTrigger()
        {
            await Task.Delay(3000);
            await this.ExecuteTriggerables();
            return Result.CreateSuccess("trigger iniciado");
        }
    }
}
