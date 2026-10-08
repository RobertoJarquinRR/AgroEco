using AgroEco.Core.Interfaces;

namespace AgroEco.Core.Triggers.Events;

public class TriggerEvent : IEntity
{
    public int Id { get; set; }

    public int TriggerId { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public TriggerEventType EventType { get; set; }

    public string? Message { get; set; }

    public int? JobRunId { get; set; }
}