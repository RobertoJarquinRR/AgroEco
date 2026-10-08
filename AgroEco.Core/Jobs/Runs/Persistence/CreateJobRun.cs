using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs.Runs;

namespace AgroEco.Core.Jobs.Runs.Persistence;

public class CreateJobRun
{
    private readonly IRepository<JobRun> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateJobRun(
        IRepository<JobRun> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(JobRun run)
    {
        if (run == null)
        {
            return Result.CreateFailure("JobRun cannot be null");
        }

        await _repository.AddAsync(run);
        await _unitOfWork.SaveChangesAsync();

        return Result.CreateSuccess($"JobRun for JobId {run.JobId} created successfully");
    }
}