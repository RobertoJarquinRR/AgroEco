using AgroEco.Core.Alerts;

namespace AgroEco.Core.Jobs.Actions.Configuration;

public sealed record SendAlertActionConfiguration : ActionConfiguration
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public AlertLevel Level { get; init; } = AlertLevel.Info;
    public List<string> EnableChannels { get; init; } = new();
    public List<string> DisableChannels { get; init; } = new();
    public Dictionary<string, object> Options { get; init; } = new();
}