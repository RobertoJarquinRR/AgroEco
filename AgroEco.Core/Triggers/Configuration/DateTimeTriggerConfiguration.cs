namespace AgroEco.Core.Triggers.Configuration;

public sealed record DateTimeTriggerConfiguration(DateTimeOffset TargetTime)
    : TriggerConfiguration;
