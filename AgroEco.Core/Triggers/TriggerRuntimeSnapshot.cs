using System.Collections.Generic;

namespace AgroEco.Core.Triggers;

public sealed record TriggerSubscriberSnapshot(
    string Type,
    int? Id = null,
    string? Name = null);

public sealed record TriggerRuntimeSnapshot(
    int TriggerId,
    string Name,
    TriggerRuntimeStatus RuntimeStatus,
    int SubscriberCount,
    IReadOnlyList<TriggerSubscriberSnapshot> Subscribers);

public sealed record TriggerableExecutionReport(
    string SubscriberType,
    int? SubscriberId,
    string? SubscriberName,
    Result Result);

public sealed record TriggerExecutionReport(
    int TriggerId,
    string TriggerName,
    TriggerRuntimeStatus RuntimeStatus,
    IReadOnlyList<TriggerableExecutionReport> SubscriberReports,
    Result OverallResult);

public sealed record TriggerRuntimeChange(
    int TriggerId,
    string TriggerName,
    TriggerRuntimeStatus PreviousStatus,
    TriggerRuntimeStatus NewStatus,
    IReadOnlyList<TriggerSubscriberSnapshot> Subscribers);