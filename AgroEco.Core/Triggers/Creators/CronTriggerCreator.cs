using AgroEco.Core.Triggers.Configuration;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Core.Configuration;

namespace AgroEco.Core.Triggers.Creators;

public sealed class CronTriggerCreator : ITriggerCreator
{
    public TriggerDescriptor Descriptor { get; } =
        new("cron", "Trigger programado (Cron)",
        [
            new("cronExpression", "Expresión Cron", "text", true),
            new("timeZone", "Zona horaria", "text", false),
            new("startDate", "Fecha inicio", "date", false),
            new("endDate", "Fecha fin", "date", false)
        ]);

    public Result<Trigger> Create(string name, TriggerConfiguration configuration)
    {
        if (configuration is not CronTriggerConfiguration cronConfig)
        {
            return Result<Trigger>.CreateFailure("Invalid configuration for CronTrigger.");
        }

        return Result<Trigger>.CreateSuccess(
            new CronTrigger(name.Trim(), cronConfig));
    }
}