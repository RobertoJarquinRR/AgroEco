using System.Text.Json;

namespace AgroEco.Core.Jobs.Actions;

public sealed class ActionFactory : IActionFactory
{
    private readonly IReadOnlyDictionary<string, IActionCreator> _creators;

    public ActionFactory(IEnumerable<IActionCreator> creators)
    {
        _creators = creators.ToDictionary(
            creator => creator.Descriptor.TypeId,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<ActionDescriptor> GetAvailable()
        => _creators.Values
            .Select(creator => creator.Descriptor)
            .OrderBy(descriptor => descriptor.DisplayName)
            .ToList();

    public Result<Action> Create(
        string typeId,
        string name,
        JsonElement config)
    {
        if (string.IsNullOrWhiteSpace(typeId))
        {
            return Result<Action>.CreateFailure("Action type is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<Action>.CreateFailure("Action name is required.");
        }

        if (!_creators.TryGetValue(typeId, out IActionCreator? creator))
        {
            return Result<Action>.CreateFailure(
                $"Unknown action type '{typeId}'.");
        }

        return creator.Create(name, config);
    }
}
