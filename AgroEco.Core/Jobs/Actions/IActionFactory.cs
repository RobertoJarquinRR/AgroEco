using AgroEco.Core.Jobs.Actions.Configuration;

namespace AgroEco.Core.Jobs.Actions;

public interface IActionFactory
{
    IReadOnlyList<ActionDescriptor> GetAvailable();

    Result<Action> Create(
        string typeId,
        string name,
        ActionConfiguration configuration);
}
