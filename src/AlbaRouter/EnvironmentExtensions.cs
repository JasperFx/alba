using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace AlbaRouter
{
    public static class EnvironmentExtensions
    {
        public static readonly string RouteData = "route.data";
        public static readonly string SpreadData = "route.spread";

        public static void SetRouteData(this HttpContext context, IDictionary<string, object> routeValues)
        {
            if (context.Items.ContainsKey(RouteData))
            {
                context.Items[RouteData] = routeValues;
            }
            else
            {
                context.Items.Add(RouteData, routeValues);
            }
        }

        public static void SetRouteData(this HttpContext context, string key, object value)
        {
            var routeData = context.GetRouteData();
            if (routeData.ContainsKey(key))
            {
                routeData[key] = value;
            }
            else
            {
                routeData.Add(key, value);
            }

        }

        public static object GetRouteData(this HttpContext context, string key)
        {
            var routeData = context.GetRouteData();

            if (routeData != null && routeData.ContainsKey(key))
            {
                return routeData[key];
            }

            return null;
        }

        public static IDictionary<string, object> GetRouteData(this HttpContext context)
        {
            if (context.Items.ContainsKey(RouteData)) return (IDictionary<string, object>) context.Items[RouteData];

            var values = new Dictionary<string, object>();
            context.Items.Add(RouteData, values);

            return values;
        }

        public static string[] GetSpreadData(this HttpContext context)
        {
            return (string[]) (context.Items.ContainsKey(SpreadData) ? context.Items[SpreadData] : new string[0]);
        }

        public static void SetSpreadData(this HttpContext context, string[] values)
        {
            if (context.Items.ContainsKey(SpreadData))
            {
                context.Items[SpreadData] = values;
            }
            else
            {
                context.Items.Add(SpreadData, values);
            }
        }
    }
}