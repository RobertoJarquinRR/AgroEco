using System.Collections.Generic;

namespace AgroEco.Core.Configuration;

public sealed record ConfigFieldChoice(string Value, string Label);

public sealed record ConfigFieldDescriptor(
    string Name,
    string Label,
    string InputType,
    bool Required = true,
    IReadOnlyList<ConfigFieldChoice>? Choices = null,
    bool Multiple = false);
