using System.Text.Json;

namespace AgroEco.Core.Jobs.Actions;

public interface IActionCreator
{
    ActionDescriptor Descriptor { get; }

    Result<Action> Create(
        string name,
        JsonElement config);
}
