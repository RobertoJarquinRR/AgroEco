using AgroEco.Core.Triggers.Configuration;

namespace AgroEco.Core.Triggers;

public interface ITriggerCreator
{
    TriggerDescriptor Descriptor { get; }

    Result<Trigger> Create(
        string name,
        TriggerConfiguration configuration);
}
