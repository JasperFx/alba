using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Alba.Routing;
using Baseline;
using Baseline.Conversion;
using Microsoft.CSharp;
using StoryTeller;
using StoryTeller.Grammars.Tables;

namespace Alba.Testing.Fixtures
{
    public class RouteBuilderFixture : Fixture
    {
        private readonly Dictionary<string, MethodInfo> _methods = new Dictionary<string, MethodInfo>();
        public static readonly LightweightCache<string, Type> TypeAliases = new LightweightCache<string, Type>();

        static RouteBuilderFixture()
        {
            addTypeAlias<int>();
            addTypeAlias<double>();
            addTypeAlias<string>();
            TypeAliases["Guid"] = typeof (Guid);
            addTypeAlias<DateTime>();
            addTypeAlias<DateTimeOffset>();
        }

        private static void addTypeAlias<T>()
        {
            var alias = new CSharpCodeProvider().GetTypeOutput(new CodeTypeReference(typeof(T)));
            TypeAliases[alias] = typeof (T);
        }

        public RouteBuilderFixture()
        {
            Title = "Inferring Routes from Method Signatures";

            var methods = typeof (FakeEndpoints).GetMethods();
            methods.Each(m =>
            {
                var desc = ToDescription(m);
                _methods.Add(desc, m);
            });

            AddSelectionValues("Methods", _methods.Keys.ToArray());
            AddSelectionValues("HttpVerbs", HttpVerbs.All.ToArray());
        }

        public static string ToDescription(MethodInfo method)
        {
            var parameters = method.GetParameters().Select(ToDescription).Join(", ");
            return $"{method.Name}({parameters})";
        }

        public static string ToDescription(ParameterInfo parameter)
        {
            if (parameter.ParameterType == typeof (Guid))
            {
                return $"{parameter.Name}:Guid";
            }

            var provider = new CSharpCodeProvider();

            if (RouteArgument.Conversions.Has(parameter.ParameterType))
            {
                var typeName = provider.GetTypeOutput(new CodeTypeReference(parameter.ParameterType));
                

                return $"{parameter.Name}:{typeName}";
            }
            else
            {
                var props = parameter.ParameterType.GetProperties().Select(prop =>
                {
                    var typeName = provider.GetTypeOutput(new CodeTypeReference(prop.PropertyType));
                    return $"{prop.Name}:{typeName}";
                });

                var fields = parameter.ParameterType.GetFields().Select(field =>
                {
                    var typeName = provider.GetTypeOutput(new CodeTypeReference(field.FieldType));
                    return $"{field.Name}:{typeName}";
                });

                var inners = props.Concat(fields).Join(", ");

                return $"{parameter.Name}:{{{inners}}}";
            }
        }

        [ExposeAsTable("Derive Routes from Method Signature")]
        public void BuildRoute(
            [Header("Method Signature"), SelectionList("Methods")]string Method, 
            [SelectionList("HttpVerbs")]out string HttpMethod, 
            out string Pattern, 
            [Default("EMPTY")] out RouteArgumentExpectation[] Arguments)
        {
            var method = _methods[Method];
            var route = RouteBuilder.Build(typeof(FakeEndpoints), method);

            HttpMethod = route.HttpMethod;
            Pattern = route.Pattern;
            Arguments = route.Arguments.Select(x => new RouteArgumentExpectation(x)).ToArray();
        }
    }

    public class RouteArgumentExpectation
    {
        public RouteArgumentExpectation(string description)
        {
            var parts = description.Replace('@', ':').Split(':');

            Key = parts[0];
            Position = int.Parse(parts[1]);
            ArgType = RouteBuilderFixture.TypeAliases[parts[2]];
        }

        public RouteArgumentExpectation(RouteArgument argument)
        {
            Position = argument.Position;
            ArgType = argument.ArgType;
            Key = argument.Key;
        }

        public override string ToString()
        {
            var typeName = new CSharpCodeProvider().GetTypeOutput(new CodeTypeReference(ArgType));
            return $"{Key}@{Position}:{typeName}";
        }

        public string Key { get; set; }

        public Type ArgType { get; set; }

        public int Position { get; set; }

        protected bool Equals(RouteArgumentExpectation other)
        {
            return string.Equals(Key, other.Key) && Equals(ArgType, other.ArgType) && Position == other.Position;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RouteArgumentExpectation) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Key != null ? Key.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ArgType != null ? ArgType.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Position;
                return hashCode;
            }
        }
    }

    public class FakeEndpoints
    {
        public void get_one() { }
        public void post_one() { }
        public void put_one_two() { }
        public void delete_one_two() { }
        public void head_one_two() { }
        public void patch_one_two() { }
        public void options_one_two() { }
        public void get_person_name(string name) { }

        public void get_query_from_to(int from, int to)
        {
            
        }

        public void get_query_From_to_To(Query query)
        {
            
        }

        public void post_user_id(Guid id)
        {
            
        }

        public void get_person1_Name(Input1 input) { }
        public void get_person2_Name(Input2 input) { }

        public void get_with_foo___bar() { }

        public void get_with__underscore() { }
    }

    public class Query
    {
        public int From;
        public int To;
    }

    public class Input1
    {
        public string Name;
    }

    public class Input2
    {
        public string Name;
    }
}