using System.Collections.Concurrent;

public class Registry<TKey, TValue> where TValue : class
{
    private readonly ConcurrentDictionary<TKey, TValue> _items = new();

    public void Register(TKey key, TValue value)
        => _items[key] = value;

    public bool Unregister(TKey key)
        => _items.TryRemove(key, out _);

    public TValue? Get(TKey key)
        => _items.TryGetValue(key, out var v) ? v : null;

    public IEnumerable<TValue> All()
        => _items.Values;

    public IEnumerable<TValue> Where(Func<TValue, bool> predicate)
        => _items.Values.Where(predicate);
}