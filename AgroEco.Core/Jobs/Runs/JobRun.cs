using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs;

namespace AgroEco.Core.Jobs.Runs;

public class JobRun : IEntity
{
    public int Id { get; set; }

    public int JobId { get; set; }

    public int? TriggerId { get; set; }

    public TriggeredBy TriggeredBy { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }

    public Status Status { get; set; }

    public string? Message { get; set; }

    public string? Error { get; set; }

    public List<JobRunAction> Actions { get; set; } = [];
}