using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core.Jobs.Triggers
{
    public class DateTimeTrigger: ITrigger
    {

        public event Func<Task>? OnTriggerFired;

        public DateTimeTrigger(){
            
        }
        public async Task probar(){

            await Task.Delay(3000);
            if (OnTriggerFired != null)
            {
                
                await OnTriggerFired.Invoke();
            }
        }

    }
}
