using AgroEco.Core.Interfaces;

namespace AgroEco.Core.Triggers.Persistence;

public sealed class UpdateTrigger
{
    private readonly IRepository<Trigger> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTrigger(
        IRepository<Trigger> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        Trigger trigger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(trigger);

        Trigger? existingTrigger = await _repository.GetByIdAsync(
            trigger.Id,
            cancellationToken);
        if (existingTrigger is null)
        {
            return Result.CreateFailure($"Trigger with id '{trigger.Id}' not found");
        }

        await _repository.UpdateAsync(trigger, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.CreateSuccess($"Trigger '{trigger.Name}' updated successfully");
    }
}
