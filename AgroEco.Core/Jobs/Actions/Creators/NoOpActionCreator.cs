using System.Text.Json;
using AgroEco.Core.Jobs.Actions.Implementations;

namespace AgroEco.Core.Jobs.Actions.Creators;

public sealed class NoOpActionCreator : IActionCreator
{
    public ActionDescriptor Descriptor { get; } =
        new("noop", "Acción de demostración (sin operación)", []);

    public Result<Action> Create(string name, JsonElement config)
        => Result<Action>.CreateSuccess(
            new NoOpAction(name.Trim()));
}
