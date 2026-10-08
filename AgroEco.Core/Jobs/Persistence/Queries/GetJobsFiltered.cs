using AgroEco.Core.Jobs;
using AgroEco.Core;
using System.Threading;

namespace AgroEco.Core.Jobs.Persistence.Queries;

public sealed record JobFilter(
    string? SearchTerm = null,
    Status? Status = null,
    int? Priority = null,
    DateOnly? DateFrom = null,
    DateOnly? DateTo = null,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "Date",
    string SortDirection = "desc"
);

public sealed record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
};

public class GetJobsFiltered
{
    private readonly IJobRepository _jobRepository;

    public GetJobsFiltered(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<Result<PagedResult<Job>>> HandleAsync(JobFilter filter, CancellationToken ct = default)
    {
        try
        {
            var (jobs, totalCount) = await _jobRepository.GetFilteredAsync(filter, ct);
            
            var result = new PagedResult<Job>(jobs, totalCount, filter.Page, filter.PageSize);
            return Result<PagedResult<Job>>.CreateSuccess(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<Job>>.CreateFailure($"Error filtrando tareas: {ex.Message}");
        }
    }
}

public class GetJobExecutionHistory
{
    private readonly IJobRepository _jobRepository;

    public GetJobExecutionHistory(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<Result<List<JobExecutionRecord>>> HandleAsync(int jobId, CancellationToken ct = default)
    {
        try
        {
            var records = await _jobRepository.GetExecutionHistoryAsync(jobId, ct);
            return Result<List<JobExecutionRecord>>.CreateSuccess(records);
        }
        catch (Exception ex)
        {
            return Result<List<JobExecutionRecord>>.CreateFailure($"Error obteniendo historial: {ex.Message}");
        }
    }
}

public sealed record JobExecutionRecord(
    int JobId,
    string JobName,
    DateTime ExecutedAt,
    Status Status,
    string? ResultMessage,
    List<ActionExecutionRecord> Actions
);

public sealed record ActionExecutionRecord(
    string ActionName,
    Status Status,
    string? Message,
    DateTime StartedAt,
    DateTime? CompletedAt
);