using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace Alba.Stubs
{
    public class StubRequestCookieCollection : IRequestCookieCollection
    {
        private readonly IDictionary<string, string> dict = new ItemsDictionary<string, string>();
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ContainsKey(string key)
        {
            return dict.ContainsKey(key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        {
            return dict.TryGetValue(key, out value);
        }

        public int Count => dict.Count;

        public string this[string key] => dict[key];

        public ICollection<string> Keys => dict.Keys;
    }
}
