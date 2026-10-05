using System.Text.Json;

namespace AgroEco.Core.Triggers;

public interface ITriggerCreator
{
    TriggerDescriptor Descriptor { get; }

    Result<Trigger> Create(
        string name,
        JsonElement config);
}
