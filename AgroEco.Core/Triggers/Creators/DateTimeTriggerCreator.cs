using System.Globalization;
using System.Text.Json;
using AgroEco.Core.Configuration;
using AgroEco.Core.Triggers.Implementations;

namespace AgroEco.Core.Triggers.Creators;

public sealed class DateTimeTriggerCreator : ITriggerCreator
{
    public TriggerDescriptor Descriptor { get; } =
        new(
            "datetime",
            "Fecha y hora",
            [new("targetTime", "Fecha objetivo", "datetime-local")]);

    public Result<Trigger> Create(string name, JsonElement config)
    {
        if (!TryReadTargetTime(config, out DateTimeOffset targetTime))
        {
            return Result<Trigger>.CreateFailure(
                "The 'targetTime' configuration value must be a valid date and time.");
        }

        return Result<Trigger>.CreateSuccess(
            new DateTimeTrigger(name.Trim(), targetTime));
    }

    private static bool TryReadTargetTime(
        JsonElement config,
        out DateTimeOffset targetTime)
    {
        targetTime = default;
        return config.TryGetProperty("targetTime", out JsonElement element)
            && element.ValueKind == JsonValueKind.String
            && DateTimeOffset.TryParse(
                element.GetString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out targetTime);
    }
}
