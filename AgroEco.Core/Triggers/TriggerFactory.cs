using AgroEco.Core.Triggers.Configuration;

namespace AgroEco.Core.Triggers;

public sealed class TriggerFactory : ITriggerFactory
{
    private readonly IReadOnlyDictionary<string, ITriggerCreator> _creators;

    public TriggerFactory(IEnumerable<ITriggerCreator> creators)
    {
        _creators = creators.ToDictionary(
            creator => creator.Descriptor.TypeId,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<TriggerDescriptor> GetAvailable()
        => _creators.Values
            .Select(creator => creator.Descriptor)
            .OrderBy(descriptor => descriptor.DisplayName)
            .ToList();

    public Result<Trigger> Create(
        string typeId,
        string name,
        TriggerConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(typeId))
        {
            return Result<Trigger>.CreateFailure("Trigger type is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<Trigger>.CreateFailure("Trigger name is required.");
        }

        ArgumentNullException.ThrowIfNull(configuration);

        if (!_creators.TryGetValue(typeId, out ITriggerCreator? creator))
        {
            return Result<Trigger>.CreateFailure(
                $"Unknown trigger type '{typeId}'.");
        }

        return creator.Create(name, configuration);
    }
}
