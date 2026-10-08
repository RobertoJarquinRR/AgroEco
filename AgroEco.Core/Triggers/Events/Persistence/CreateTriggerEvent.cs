using AgroEco.Core.Interfaces;
using AgroEco.Core.Triggers.Events;

namespace AgroEco.Core.Triggers.Events.Persistence;

public class CreateTriggerEvent
{
    private readonly IRepository<TriggerEvent> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTriggerEvent(
        IRepository<TriggerEvent> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(TriggerEvent evt)
    {
        if (evt == null)
        {
            return Result.CreateFailure("TriggerEvent cannot be null");
        }

        await _repository.AddAsync(evt);
        await _unitOfWork.SaveChangesAsync();

        return Result.CreateSuccess($"TriggerEvent for TriggerId {evt.TriggerId} created successfully");
    }
}