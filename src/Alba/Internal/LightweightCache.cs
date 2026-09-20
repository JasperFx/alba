namespace Alba.Internal;

/// <summary>
/// A dictionary that builds missing values on demand. Backs static MimeType state
/// shared by every AlbaHost, so access is locked and enumeration order is stable
/// </summary>
internal sealed class LightweightCache<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _values = new();
    private readonly Func<TKey, TValue> _onMissing;
    private readonly object _lock = new();

    public LightweightCache(Func<TKey, TValue> onMissing)
    {
        _onMissing = onMissing;
    }

    public TValue this[TKey key]
    {
        get
        {
            lock (_lock)
            {
                if (!_values.TryGetValue(key, out var value))
                {
                    value = _onMissing(key);

                    if (value != null)
                    {
                        _values[key] = value;
                    }
                }

                return value;
            }
        }
        set
        {
            lock (_lock)
            {
                _values[key] = value;
            }
        }
    }

    public TValue[] GetAll()
    {
        lock (_lock)
        {
            return _values.Values.ToArray();
        }
    }
}
