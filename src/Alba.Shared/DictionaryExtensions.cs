using System.Collections.Generic;
using System.Linq;
using Baseline.Testing;

namespace Alba
{
    public static class DictionaryExtensions
    {

        public static T Get<T>(this IDictionary<string, object> env, string key)
        {
            object value;
            return env.TryGetValue(key, out value) ? (T)value : default(T);
        }


        public static void Append(this IDictionary<string, object> env, string key, object o)
        {
            if (env.ContainsKey(key))
            {
                env[key] = o;
            }
            else
            {
                env.Add(key, o);
            }
        }

        public static void Set<T>(this IDictionary<string, object> dict, string key, T value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }

        // TODO -- need to go find the test for this in fubu
        public static void CopyTo(this IDictionary<string, object> source, IDictionary<string, object> destination,
            params string[] keys)
        {
            keys.Where(source.ContainsKey).Each(x => destination.Add(x, source[x]));
        }
    }
}