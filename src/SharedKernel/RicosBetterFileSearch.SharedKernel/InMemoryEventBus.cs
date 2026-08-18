namespace RicosBetterFileSearch.SharedKernel;

public class InMemoryEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public void Publish<T>(T domainEvent) where T : IDomainEvent
    {
        var eventType = typeof(T);
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            foreach (var handler in handlers)
            {
                ((Action<T>)handler)(domainEvent);
            }
        }
    }

    public void Subscribe<T>(Action<T> handler) where T : IDomainEvent
    {
        var eventType = typeof(T);
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Delegate>();
        }
        _handlers[eventType].Add(handler);
    }
}
