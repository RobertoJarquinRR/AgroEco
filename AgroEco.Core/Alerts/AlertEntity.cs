using AgroEco.Core.Interfaces;
using System.Collections.Generic;

namespace AgroEco.Core.Alerts;

public sealed class AlertEntity : IEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Status { get; set; }
    public string ConfigurationJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string MetadataJson { get; set; } = "{}";

    public List<AlertDeliveryEntity> Deliveries { get; set; } = new();
}