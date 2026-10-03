using AgroEco.Core.Interfaces;

namespace AgroEco.Core.Jobs.Persistence;

public sealed class UpdateJob
{
    private readonly IRepository<Job> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateJob(
        IRepository<Job> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        Job job,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(job);

        Job? existingJob = await _repository.GetByIdAsync(job.Id, cancellationToken);
        if (existingJob is null)
        {
            return Result.CreateFailure($"Job with id '{job.Id}' not found");
        }

        await _repository.UpdateAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.CreateSuccess($"Job '{job.Name}' updated successfully");
    }
}
