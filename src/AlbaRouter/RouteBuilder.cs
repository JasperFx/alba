using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;

namespace AlbaRouter
{
    // This is mostly tested through Storyteller specs
    public static class RouteBuilder
    {
        public static readonly IList<string> InputTypeNames = new List<string> {"input", "query", "message", "body"};

        public static Route Build<T>(Expression<Action<T>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);
            return Build(typeof (T), method);
        }

        public static Route Build(Type handlerType, MethodInfo method)
        {
            return Build(method.Name, handlerType, method);
        }

        public static Route Build(string pattern, Type handlerType, MethodInfo method)
        {
            pattern = pattern.Replace("___", "-").Replace("__", "_@");

            var parts = pattern.Split('_')
                .Select(x => x.Replace("@", "_")).ToArray();
            



            var verb = HttpVerbs.All.FirstOrDefault(x => x.EqualsIgnoreCase(parts[0]));
            if (verb.IsNotEmpty())
            {
                parts = parts.Skip(1).ToArray();
                
            }
            else
            {
                verb = HttpVerbs.GET;
            }

            var segments = parts
                .Select((x, position) => new Segment(x.ToLowerInvariant(), position))
                .OfType<ISegment>()
                .ToArray();


            Type inputType = DetermineInputType(method);
            var hasPrimitives = method.GetParameters().Any(x => x.ParameterType == typeof(Guid) || RouteArgument.Conversions.Has(x.ParameterType));
            if (hasPrimitives)
            {
                for (var i = 0; i < segments.Length; i++)
                {
                    var current = parts[i];
                    var parameter = method.GetParameters().FirstOrDefault(x => x.Name == current);
                    if (parameter != null)
                    {
                        var argument = new RouteArgument(parameter, i);
                        segments[i] = argument;
                    }
                }
            }
            else if (inputType != null)
            {
                var members = inputType.GetProperties().OfType<MemberInfo>().Concat(inputType.GetFields()).ToArray();

                for (var i = 0; i < segments.Length; i++)
                {
                    var current = parts[i];
                    var member = members.FirstOrDefault(x => x.Name == current);
                    if (member != null)
                    {
                        var argument = new RouteArgument(member, i);
                        segments[i] = argument;
                    }
                }

            }

            var route = new Route(segments, verb, env => Task.CompletedTask)
            {
                HandlerType = handlerType,
                Method = method,
                InputType = inputType
            };

            method.ForAttribute<RouteNameAttribute>(att => route.Name = att.Name);

            return route;
        }

        public static Type DetermineInputType(MethodInfo method)
        {
            var first = method.GetParameters().FirstOrDefault();
            if (first == null) return null;

            if (InputTypeNames.Contains(first.Name, StringComparer.OrdinalIgnoreCase))
            {
                return first.ParameterType;
            }

            return null;
        }
    }


}