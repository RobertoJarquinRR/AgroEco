namespace AgroEco.Core.Configuration;

public sealed record ConfigFieldDescriptor(
    string Name,
    string Label,
    string InputType,
    bool Required = true);
