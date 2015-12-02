using System.Collections.Generic;

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
    }
}