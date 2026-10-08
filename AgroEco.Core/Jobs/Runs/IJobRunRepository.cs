namespace AgroEco.Core.Jobs.Runs;

public interface IJobRunRepository
{
    Task<List<JobRun>> GetByJobIdAsync(
        int jobId,
        CancellationToken ct = default);
}
