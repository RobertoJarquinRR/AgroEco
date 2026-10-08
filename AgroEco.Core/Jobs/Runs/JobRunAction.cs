using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs;

namespace AgroEco.Core.Jobs.Runs;

public class JobRunAction : IEntity
{
    public int Id { get; set; }

    public int JobRunId { get; set; }

    public int ActionId { get; set; }

    public string ActionName { get; set; } = string.Empty;

    public string ActionType { get; set; } = string.Empty;

    public Status Status { get; set; }

    public string? Message { get; set; }

    public string? Error { get; set; }

    public long DurationMs { get; set; }
}