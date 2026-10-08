using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs.Runs;
using System.Linq;

namespace AgroEco.Core.Jobs.Runs.Persistence;

public class GetJobRunsByJob
{
    private readonly IJobRunRepository _repository;

    public GetJobRunsByJob(IJobRunRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<JobRun>>> HandleAsync(int jobId)
    {
        List<JobRun> runs = await _repository.GetByJobIdAsync(jobId);

        return Result<List<JobRun>>.CreateSuccess(runs);
    }
}