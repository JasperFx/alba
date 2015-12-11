using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Alba.Routing;
using Baseline;
using Baseline.Reflection;

namespace Alba.Urls
{
    public class MethodRoute<THandler> : IMethodRoute<THandler>
    {
        public Route Route { get; }
        public MethodInfo Method { get; }

        private readonly IDictionary<int, string> _parameters = new Dictionary<int, string>();

        /// <summary>
        /// Just for testing
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="url"></param>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        public static MethodRoute<THandler> For(Expression<Action<THandler>> expression, string url, string httpMethod)
        {
            var method = ReflectionHelper.GetMethod(expression);
            var leaf = new Route(url, httpMethod, e => Task.CompletedTask);

            return new MethodRoute<THandler>(leaf, method);
        } 

        public MethodRoute(Route route, MethodInfo method)
        {
            Route = route;
            Method = method;
        }

        public void Register(IUrlGraph graph)
        {
            graph.Register(Route.Name, this);
            graph.RegisterByHandler(typeof(THandler), Method, this);
        }

        public void AddParameter(string paramName, string key = null)
        {
            var parameter = Method.GetParameters().Where(x => x.Name == paramName).SingleOrDefault();
            if (parameter == null) throw new ArgumentOutOfRangeException(nameof(paramName));

            

            AddParameter(parameter, key);

        }

        public void AddParameter(ParameterInfo parameter, string key = null)
        {
            _parameters.Add(parameter.Position, key ?? parameter.Name);
        }

        public IDictionary<string, string> ToParameters(Expression<Action<THandler>> expression)
        {
            var arguments = MethodCallParser.ToArguments(expression);
            
            var parameters = new Dictionary<string, string>();

            _parameters.Each(pair =>
            {
                parameters.Add(pair.Value, arguments[pair.Key].ToString());
            });

            return parameters;
        }
    }
}