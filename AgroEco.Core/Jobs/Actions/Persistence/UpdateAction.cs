using AgroEco.Core.Interfaces;
using Action = AgroEco.Core.Jobs.Actions.Action;

namespace AgroEco.Core.Jobs.Actions.Persistence;

public sealed class UpdateAction
{
    private readonly IRepository<Action> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAction(
        IRepository<Action> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        Action action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        Action? existingAction = await _repository.GetByIdAsync(
            action.Id,
            cancellationToken);
        if (existingAction is null)
        {
            return Result.CreateFailure($"Action with id '{action.Id}' not found");
        }

        await _repository.UpdateAsync(action, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.CreateSuccess($"Action '{action.Name}' updated successfully");
    }
}
