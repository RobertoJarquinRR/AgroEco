namespace AgroEco.Core.Alerts;

public sealed class AlertDeliveryEntity
{
    public int Id { get; set; }
    public int AlertId { get; set; }
    public string ChannelType { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime AttemptedAt { get; set; }
    public TimeSpan Duration { get; set; }
}