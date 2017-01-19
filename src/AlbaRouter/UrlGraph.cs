using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Conversion;
using Baseline.Reflection;
using HandlerMethods = Baseline.LightweightCache<string, AlbaRouter.Route>;

namespace AlbaRouter
{
    public class UrlGraph : IUrlRegistry
    {
        public static readonly Conversions Conversions = new Conversions();

        private readonly LightweightCache<Type, List<Route>> _routesByInputModel
            = new LightweightCache<Type, List<Route>>(_ => new List<Route>());

        private readonly LightweightCache<Type, HandlerMethods> _routesPerHandler
            = new LightweightCache<Type, HandlerMethods>(type => new HandlerMethods());

        private readonly LightweightCache<string, Route> _routesPerName
            = new LightweightCache<string, Route>();


        public void Register(Route route)
        {
            _routesPerName[route.Name] = route;
            if (route.InputType != null)
            {
                _routesByInputModel[route.InputType].Add(route);
            }

            if (route.HandlerType != null)
            {
                _routesPerHandler[route.HandlerType][route.Method.Name] = route;
            }
        }

        public string UrlFor(object model, string httpMethod = null)
        {
            var route = RouteFor(model, httpMethod);
            return route.ToUrlFromInputModel(model);
        }

        public Route RouteFor(object model, string httpMethod = null)
        {
            var routes = _routesByInputModel[model.GetType()];
            return resolveRoute(httpMethod, routes);
        }

        public string UrlFor<T>(string httpMethod = null) where T : class
        {
            var route = RouteFor<T>(httpMethod);

            assertNoParameters(route);

            return "/" + route.Pattern;
        }

        private static void assertNoParameters(Route route) 
        {
            if (route.HasParameters || route.HasSpread)
                throw new UrlResolutionException($"Route {route} has arguments and cannot be resolved this way");
        }

        public Route RouteFor<T>(string httpMethod = null)
        {
            return RouteFor(typeof (T), httpMethod);
        }

        public Route RouteFor(Type handlerOrInputType, string httpMethod = null)
        {
            var routes = _routesPerHandler[handlerOrInputType].Concat(_routesByInputModel[handlerOrInputType]);
            return resolveRoute(httpMethod, routes);
        }

        private static Route resolveRoute(string httpMethod, IEnumerable<Route> routes)
        {
            if (!routes.Any())
            {
                throw new UrlResolutionException($"There are no matching routes");
            }

            if (routes.Count() == 1)
            {
                var onlyOne = routes.Single();
                if (httpMethod.IsEmpty() || onlyOne.HttpMethod.EqualsIgnoreCase(httpMethod)) return onlyOne;

                throw new UrlResolutionException(
                    $"The matching route ({onlyOne.HttpMethod}:{onlyOne.Pattern}) is a mismatch on the requested Http verb '{httpMethod}'");
            }

            if (httpMethod.IsEmpty())
            {
                throw new UrlResolutionException(
                    $"Multiple matches, try searching with the Http Verb. Found: {routes.Select(x => x.ToString()).Join(", ")}");
            }

            var matching = routes.SingleOrDefault(x => x.HttpMethod.EqualsIgnoreCase(httpMethod));
            if (matching == null)
                throw new UrlResolutionException($"There are no matching routes for Http Verb '{httpMethod}'");

            return matching;
        }

        public string UrlFor(Type handlerType, MethodInfo method = null, string httpMethod = null)
        {
            var route = method == null ? RouteFor(handlerType, httpMethod) : RouteFor(handlerType, method);

            assertNoParameters(route);

            return "/" + route.Pattern;
        }

        public Route RouteFor(Type handlerType, MethodInfo method)
        {
            if (!_routesPerHandler.Has(handlerType))
                throw new UrlResolutionException($"There are no matching routes for handler {handlerType.FullName}");

            var routes = _routesPerHandler[handlerType];
            if (!routes.Has(method.Name))
            {
                throw new UrlResolutionException($"No route matches the method {handlerType.FullName}.{method.Name}()");
            }

            var route = routes[method.Name];

            return route;
        }

        public string UrlFor<THandler>(Expression<Action<THandler>> expression, string httpMethod = null)
        {
            var method = ReflectionHelper.GetMethod(expression);

            var route = RouteFor(typeof(THandler), method);

            return route.ReadRouteDataFromMethodArguments(expression);
        }

        public string UrlFor(string routeName, IDictionary<string, object> parameters = null)
        {
            var route = RouteByName(routeName);
            if (route == null) throw new UrlResolutionException($"There are no routes with the name '{routeName}'");

            if (parameters == null)
            {
                assertNoParameters(route);
                return "/" + route.Pattern;
            }
            else
            {
                return route.ToUrlFromParameters(parameters);
            }


        }

        private Route RouteByName(string routeName)
        {
            return _routesPerName.Has(routeName) ? _routesPerName[routeName] : null;
        }
    }
}