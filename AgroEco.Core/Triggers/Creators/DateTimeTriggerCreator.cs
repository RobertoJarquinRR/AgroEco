using AgroEco.Core.Configuration;
using AgroEco.Core.Triggers.Configuration;
using AgroEco.Core.Triggers.Implementations;

namespace AgroEco.Core.Triggers.Creators;

public sealed class DateTimeTriggerCreator : ITriggerCreator
{
    public TriggerDescriptor Descriptor { get; } =
        new(
            "datetime",
            "Fecha y hora",
            [new("targetTime", "Fecha objetivo", "datetime-local")]);

    public Result<Trigger> Create(
        string name,
        TriggerConfiguration configuration)
    {
        if (configuration is not DateTimeTriggerConfiguration dateTimeConfiguration)
        {
            return Result<Trigger>.CreateFailure(
                "The trigger configuration is invalid for the datetime trigger.");
        }

        if (dateTimeConfiguration.TargetTime <= DateTimeOffset.UtcNow)
        {
            return Result<Trigger>.CreateFailure(
                "The target time must be in the future.");
        }

        return Result<Trigger>.CreateSuccess(
            new DateTimeTrigger(
                name.Trim(),
                dateTimeConfiguration));
    }
}
