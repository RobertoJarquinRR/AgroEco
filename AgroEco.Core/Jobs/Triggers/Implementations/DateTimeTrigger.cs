using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgroEco.Core.Jobs.Triggers.Implementations
{
    public class DateTimeTrigger : Trigger
    {

        public DateTime _fecha { get; set; }

        public DateTimeTrigger(string name, DateTime fecha):
        base( name)
        {
        

        }

        public override async Task<Result> InitTrigger()
        {
            bool ya = false;
            while(ya == false)
            {
                
                await Task.Delay(4000);
                ya = estiempo();
                if(ya == true){
                    var triggerResults = await this.ExecuteTriggerables();
                    var mensajes = triggerResults
                        .Select(r => r.Message)
                        .Where(m => !string.IsNullOrEmpty(m));
                    string resumen = string.Join("; ", mensajes);

                    bool todoExitoso = triggerResults.All(r => r.Success);
                    if (!todoExitoso)
                        return Result.CreateFailure($"Trigger '{Name}' executed with errors: {resumen}");

                    return Result.CreateSuccess($"Trigger '{Name}' executed - {resumen}");
                }

            }
            return Result.CreateSuccess($"Trigger '{Name}' started, waiting for time condition");
        }

        public bool estiempo(){
        
            return true;
        }

    }
}
