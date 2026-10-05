using System.Text.Json;
using AgroEco.Core.Jobs.Actions.Implementations;

namespace AgroEco.Core.Jobs.Actions.Creators;

public sealed class ActionTestCreator : IActionCreator
{
    public ActionDescriptor Descriptor { get; } =
        new("test", "Acción de prueba", []);

    public Result<Action> Create(string name, JsonElement config)
        => Result<Action>.CreateSuccess(
            new ActionTest(name.Trim()));
}
