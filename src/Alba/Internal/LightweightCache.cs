using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Alba.Internal;

internal sealed class LightweightCache<TKey, TValue> : IEnumerable<TValue> where TKey : notnull
{
    private readonly IDictionary<TKey, TValue> _values;

    // The cache backs static MimeType state that is shared by every AlbaHost.
    // Locking keeps the dictionary intact and its enumeration order stable.
    private readonly object _lock = new();

    private Func<TValue, TKey> _getKey = delegate { throw new NotImplementedException(); };

    private Func<TKey, TValue> _onMissing = delegate (TKey key) {
        var message = $"Key '{key}' could not be found";
        throw new KeyNotFoundException(message);
    };

    public LightweightCache()
        : this(new Dictionary<TKey, TValue>())
    {
    }

    public LightweightCache(Func<TKey, TValue> onMissing)
        : this(new Dictionary<TKey, TValue>(), onMissing)
    {
    }

    public LightweightCache(IDictionary<TKey, TValue> dictionary, Func<TKey, TValue> onMissing)
        : this(dictionary)
    {
        _onMissing = onMissing;
    }

    public LightweightCache(IDictionary<TKey, TValue> dictionary)
    {
        _values = dictionary;
    }


    public Func<TKey, TValue> OnMissing
    {
        set => _onMissing = value;
    }

    public Func<TValue, TKey> GetKey
    {
        get => _getKey;
        set => _getKey = value;
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _values.Count;
            }
        }
    }

    public TValue? First
    {
        get
        {
            lock (_lock)
            {
                foreach (var pair in _values)
                {
                    return pair.Value;
                }

                return default(TValue);
            }
        }
    }

    public TValue this[TKey key]
    {
        get
        {
            lock (_lock)
            {
                if (!_values.TryGetValue(key, out TValue? value))
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

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<TValue>)this).GetEnumerator();
    }

    public IEnumerator<TValue> GetEnumerator()
    {
        return ((IEnumerable<TValue>)GetAll()).GetEnumerator();
    }

    /// <summary>
    ///     Guarantees that the Cache has the default value for a given key.
    ///     If it does not already exist, it's created.
    /// </summary>
    /// <param name="key"></param>
    public void FillDefault(TKey key)
    {
        Fill(key, _onMissing(key));
    }

    public void Fill(TKey key, TValue value)
    {
        lock (_lock)
        {
            if (_values.ContainsKey(key))
            {
                return;
            }

            _values.Add(key, value);
        }
    }

    public bool TryRetrieve(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        lock (_lock)
        {
            return _values.TryGetValue(key, out value);
        }
    }

    public void Each(Action<TValue> action)
    {
        foreach (var value in GetAll())
        {
            action(value);
        }
    }

    public void Each(Action<TKey, TValue> action)
    {
        KeyValuePair<TKey, TValue>[] pairs;
        lock (_lock)
        {
            pairs = _values.ToArray();
        }

        foreach (var pair in pairs)
        {
            action(pair.Key, pair.Value);
        }
    }

    public bool Has(TKey key)
    {
        lock (_lock)
        {
            return _values.ContainsKey(key);
        }
    }

    public bool Exists(Predicate<TValue> predicate)
    {
        var returnValue = false;

        Each(delegate (TValue value) { returnValue |= predicate(value); });

        return returnValue;
    }

    public TValue? Find(Predicate<TValue> predicate)
    {
        foreach (var value in GetAll())
        {
            if (predicate(value))
            {
                return value;
            }
        }

        return default;
    }

    public TValue[] GetAll()
    {
        lock (_lock)
        {
            var returnValue = new TValue[_values.Count];
            _values.Values.CopyTo(returnValue, 0);

            return returnValue;
        }
    }

    public void Remove(TKey key)
    {
        lock (_lock)
        {
            _values.Remove(key);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _values.Clear();
        }
    }

    public void WithValue(TKey key, Action<TValue> action)
    {
        lock (_lock)
        {
            if (_values.ContainsKey(key))
            {
                action(this[key]);
            }
        }
    }

    public void ClearAll()
    {
        Clear();
    }
}
