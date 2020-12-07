using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Alba.Stubs
{
    public class ItemsDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey: notnull
    {
        private readonly IDictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();


        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) dict).GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            dict.Add(item);
        }

        public void Clear()
        {
            dict.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return dict.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            dict.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return dict.Remove(item);
        }

        public int Count => dict.Count;

        public bool IsReadOnly => dict.IsReadOnly;

        public void Add(TKey key, TValue value)
        {
            dict.Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return dict.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            return dict.Remove(key);
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return dict.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get
            {
                if (dict.TryGetValue(key, out TValue? value))
                {
                    return value;
                }

                return default!;
            }
            set => dict[key] = value;
        }

        public ICollection<TKey> Keys => dict.Keys;

        public ICollection<TValue> Values => dict.Values;
    }
}