using AgroEco.Core.Interfaces;
using AgroEco.Core.Triggers.Events;
using System.Linq;

namespace AgroEco.Core.Triggers.Events.Persistence;

public class GetTriggerEventsByTrigger
{
    private readonly IRepository<TriggerEvent> _repository;

    public GetTriggerEventsByTrigger(IRepository<TriggerEvent> repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<TriggerEvent>>> HandleAsync(int triggerId)
    {
        var events = await _repository.GetAllAsync();
        var filtered = events.Where(e => e.TriggerId == triggerId)
                             .OrderByDescending(e => e.OccurredAt)
                             .ToList();

        return Result<List<TriggerEvent>>.CreateSuccess(filtered);
    }
}