using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AgroEco.UI.Events;

public sealed class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();

    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        var eventType = typeof(TEvent);
        _handlers.AddOrUpdate(
            eventType,
            _ => [handler],
            (_, existing) =>
            {
                var list = new List<Delegate>(existing) { handler };
                return list;
            });
    }

    public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        var eventType = typeof(TEvent);
        if (_handlers.TryGetValue(eventType, out var list))
        {
            var handlerDelegate = (Delegate)handler;
            var updated = list.Where(h => !h.Equals(handlerDelegate)).ToList();
            _handlers[eventType] = updated;
        }
    }

    public void Publish<TEvent>(TEvent eventData) where TEvent : class
    {
        var eventType = typeof(TEvent);
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            foreach (var handler in handlers.OfType<Action<TEvent>>())
            {
                try
                {
                    handler(eventData);
                }
                catch
                {
                }
            }
        }
    }
}