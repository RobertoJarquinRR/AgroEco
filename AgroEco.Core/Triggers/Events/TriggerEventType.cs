namespace AgroEco.Core.Triggers.Events;

public enum TriggerEventType
{
    Enabled = 1,
    Disabled = 2,
    Fired = 3,
    ReachedMaxExecutions = 4,
    ReachedEndDate = 5,
    Error = 6
}