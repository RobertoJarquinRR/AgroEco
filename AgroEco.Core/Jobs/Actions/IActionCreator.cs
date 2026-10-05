using AgroEco.Core.Jobs.Actions.Configuration;

namespace AgroEco.Core.Jobs.Actions;

public interface IActionCreator
{
    ActionDescriptor Descriptor { get; }

    Result<Action> Create(
        string name,
        ActionConfiguration configuration);
}
