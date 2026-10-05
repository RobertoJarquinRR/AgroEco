using AgroEco.Core.Configuration;

namespace AgroEco.Core.Triggers;

public sealed record TriggerDescriptor(
    string TypeId,
    string DisplayName,
    IReadOnlyList<ConfigFieldDescriptor> Fields);
