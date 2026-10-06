using System;

namespace AgroEco.UI.Events;

public interface IEventBus
{
    void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
    void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
    void Publish<TEvent>(TEvent eventData) where TEvent : class;
}