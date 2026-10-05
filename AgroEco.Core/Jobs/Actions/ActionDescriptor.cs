using AgroEco.Core.Configuration;

namespace AgroEco.Core.Jobs.Actions;

public sealed record ActionDescriptor(
    string TypeId,
    string DisplayName,
    IReadOnlyList<ConfigFieldDescriptor> Fields);
