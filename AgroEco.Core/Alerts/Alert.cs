using AgroEco.Core.Interfaces;
using System.Collections.Generic;

namespace AgroEco.Core.Alerts;

public sealed class Alert : IEntity
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public AlertLevel Level { get; set; } = AlertLevel.Info;

    public AlertStatus Status { get; set; } = AlertStatus.Pending;

    public AlertConfiguration Configuration { get; set; } = AlertConfiguration.Default;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeliveredAt { get; set; }

    public Dictionary<string, object> Metadata { get; set; } = new();

    public List<AlertDeliveryEntity> Deliveries { get; set; } = new();

    internal Alert() { }

    public static Alert Create(string title, string message, AlertLevel level = AlertLevel.Info, AlertConfiguration? config = null)
        => new()
        {
            Title = title,
            Message = message,
            Level = level,
            Configuration = config ?? AlertConfiguration.Default
        };

    public void MarkDelivered()
    {
        Status = AlertStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
    }

    public void MarkPartiallyDelivered()
    {
        Status = AlertStatus.PartiallyDelivered;
        DeliveredAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        Status = AlertStatus.Failed;
    }

    public void MarkCanceled()
    {
        Status = AlertStatus.Canceled;
    }

    public void AddDelivery(AlertDeliveryEntity delivery)
    {
        Deliveries.Add(delivery);
    }

    public void UpdateStatusFromDeliveries()
    {
        if (Deliveries.Count == 0)
        {
            Status = AlertStatus.Pending;
            return;
        }

        var successCount = Deliveries.Count(d => d.Success);
        var totalCount = Deliveries.Count;

        if (successCount == totalCount)
        {
            Status = AlertStatus.Delivered;
            DeliveredAt = DateTime.UtcNow;
        }
        else if (successCount > 0)
        {
            Status = AlertStatus.PartiallyDelivered;
            DeliveredAt = DateTime.UtcNow;
        }
        else
        {
            Status = AlertStatus.Failed;
        }
    }
}
