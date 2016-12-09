using System.Collections.Generic;

namespace Alba.Routing
{
    public static class EnvironmentExtensions
    {
        public static readonly string OwinRouteData = "owin.route.data";
        public static readonly string OwinSpreadData = "owin.route.spread";

        public static void SetRouteData(this IDictionary<string, object> env, IDictionary<string, object> routeValues)
        {
            if (env.ContainsKey(OwinRouteData))
            {
                env[OwinRouteData] = routeValues;
            }
            else
            {
                env.Add(OwinRouteData, routeValues);
            }
        }

        public static void SetRouteData(this IDictionary<string, object> env, string key, object value)
        {
            var routeData = env.GetRouteData();
            if (routeData.ContainsKey(key))
            {
                routeData[key] = value;
            }
            else
            {
                routeData.Add(key, value);
            }

        }

        public static object GetRouteData(this IDictionary<string, object> env, string key)
        {
            var routeData = env.GetRouteData();

            if (routeData != null && routeData.ContainsKey(key))
            {
                return routeData[key];
            }

            return null;
        }

        public static IDictionary<string, object> GetRouteData(this IDictionary<string, object> env)
        {
            if (env.ContainsKey(OwinRouteData)) return (IDictionary<string, object>) env[OwinRouteData];

            var values = new Dictionary<string, object>();
            env.Add(OwinRouteData, values);

            return values;
        }

        public static string[] GetSpreadData(this IDictionary<string, object> env)
        {
            return (string[]) (env.ContainsKey(OwinSpreadData) ? env[OwinSpreadData] : new string[0]);
        }

        public static void SetSpreadData(this IDictionary<string, object> env, string[] values)
        {
            if (env.ContainsKey(OwinSpreadData))
            {
                env[OwinSpreadData] = values;
            }
            else
            {
                env.Add(OwinSpreadData, values);
            }
        }
    }
}