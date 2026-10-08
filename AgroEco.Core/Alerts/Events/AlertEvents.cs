using AgroEco.Core.Alerts;

namespace AgroEco.Core.Alerts.Events;

public sealed record AlertRaisedEvent(
    Alert Alert,
    IReadOnlyList<string> TargetChannels
);

public sealed record AlertDeliveredEvent(
    Alert Alert,
    IReadOnlyList<AlertDeliveryResult> Results
);