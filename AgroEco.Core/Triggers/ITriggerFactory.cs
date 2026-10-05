using System.Text.Json;

namespace AgroEco.Core.Triggers;

public interface ITriggerFactory
{
    IReadOnlyList<TriggerDescriptor> GetAvailable();

    Result<Trigger> Create(
        string typeId,
        string name,
        JsonElement config);
}
