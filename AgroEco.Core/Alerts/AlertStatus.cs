namespace AgroEco.Core.Alerts;

public enum AlertStatus
{
    Pending = 0,
    Delivered = 1,
    PartiallyDelivered = 2,
    Failed = 3,
    Canceled = 4
}