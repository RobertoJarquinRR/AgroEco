using AgroEco.Core.Triggers.Configuration;

namespace AgroEco.Core.Triggers;

public interface ITriggerFactory
{
    IReadOnlyList<TriggerDescriptor> GetAvailable();

    Result<Trigger> Create(
        string typeId,
        string name,
        TriggerConfiguration configuration);
}
