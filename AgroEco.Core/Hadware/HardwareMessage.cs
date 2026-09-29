using System.Text.Json;

namespace AgroEco.Core.Hadware;

public sealed record HardwareMessage(
    string Type,
    string ComponentId,
    JsonElement? Value = null,
    string? RequestId = null,
    bool? Success = null,
    string? Error = null);
