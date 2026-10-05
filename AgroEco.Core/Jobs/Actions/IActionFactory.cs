using System.Text.Json;

namespace AgroEco.Core.Jobs.Actions;

public interface IActionFactory
{
    IReadOnlyList<ActionDescriptor> GetAvailable();

    Result<Action> Create(
        string typeId,
        string name,
        JsonElement config);
}
