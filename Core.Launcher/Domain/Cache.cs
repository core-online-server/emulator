using Core.Abstract.Domain;

namespace Core.Launcher.Domain;

public class Cache<TValue> : ICache<TValue>
    where TValue : notnull
{
    private readonly Dictionary<TValue, int> _dictionary = new();

    private readonly List<TValue> _collection = new();

    public void Hold(TValue value)
    {
        _dictionary[value] = _collection.Count;

        _collection.Add(value);
    }

    public void Release(TValue value)
    {
        _dictionary.Remove(value);

        _collection.Remove(value);
    }

    public int Get(TValue value)
    {
        return _dictionary[value] + 1;
    }

    public TValue Get(int id)
    {
        if (id == 0) return default!;

        return _collection[id - 1];
    }
}