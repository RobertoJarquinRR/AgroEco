using System.Collections.Concurrent;

namespace AgroEco.Core.Triggers.Engine;

public sealed class TriggerRegistry
{
    private readonly Registry<int, Trigger> _registry = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public Trigger? Get(int id)
    {
        return _registry.Get(id);
    }

    public void Register(int id, Trigger trigger)
    {
        _registry.Register(id, trigger);
    }

    public bool Unregister(int id)
    {
        return _registry.Unregister(id);
    }

    public IEnumerable<Trigger> All()
    {
        return _registry.All();
    }

    public IEnumerable<Trigger> Where(Func<Trigger, bool> predicate)
    {
        return _registry.Where(predicate);
    }

    public async Task<Trigger?> TryGetReusableAsync(int id, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            Trigger? registeredTrigger = _registry.Get(id);
            if (registeredTrigger is not null)
            {
                if (registeredTrigger.RuntimeStatus is TriggerRuntimeStatus.Completed
                    or TriggerRuntimeStatus.Faulted
                    || (registeredTrigger.RuntimeStatus == TriggerRuntimeStatus.Stopped
                        && !registeredTrigger.HasTriggerables))
                {
                    _registry.Unregister(id);
                    return null;
                }

                return registeredTrigger;
            }

            return null;
        }
        finally
        {
            _lock.Release();
        }
    }
}