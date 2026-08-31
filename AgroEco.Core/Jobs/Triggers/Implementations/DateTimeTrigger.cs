using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Triggers
{
    public class DateTimeTrigger : Trigger
    {

        public DateTimeTrigger(string name, TriggersType type) :
        base( name, type)
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
