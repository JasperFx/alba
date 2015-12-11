using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alba.Routing;
using Baseline;

namespace Alba.Urls
{
    public class RouteWithInputModel<T> : IRouteWithInputModel
    {
        private readonly IList<IParameter> _parameters = new List<IParameter>();

        public RouteWithInputModel(Route route, string httpMethod)
        {
            Route = route;
            HttpMethod = httpMethod;
        }

        public Route Route { get; }
        public string HttpMethod { get; }

        public void Register(IUrlGraph graph)
        {
            graph.Register(Route.Name, this);
            graph.RegisterByInput(typeof (T), this);
        }

        public bool HasParameters => _parameters.Any();

        public IDictionary<string, string> ToParameters(object model)
        {
            var dict = new Dictionary<string, string>();

            _parameters.Each(x =>
            {
                var value = x.Read(model);
                dict.Add(x.Key, value?.ToString() ?? string.Empty);
            });

            return dict;
        }

        public void ApplyValues(object input, IDictionary<string, string> rawValues)
        {
            _parameters.Each(x =>
            {
                if (rawValues.ContainsKey(x.Key))
                {
                    var value = rawValues[x.Key];
                    x.Write(input, value);
                }
            });
        }

        public void AddFieldParam(string name, string key = null)
        {
            var field = typeof (T).GetFields().SingleOrDefault(x => x.Name == name);
            if (field == null) throw new ArgumentOutOfRangeException(nameof(name));
            AddFieldParam(field, key);
        }


        public void AddFieldParam(FieldInfo field, string key = null)
        {
            var parameter = new FieldInfoParameter(field, key);
            _parameters.Add(parameter);
        }


        public void AddPropertyParam(string name, string key = null)
        {
            var property = typeof (T).GetProperties().FirstOrDefault(x => x.Name == name);
            if (property == null) throw new ArgumentOutOfRangeException(nameof(name));

            AddPropertyParam(property, key);
        }

        public void AddPropertyParam(PropertyInfo property, string key = null)
        {
            var parameter = new PropertyInfoParameter(property, key);
            _parameters.Add(parameter);
        }


        internal interface IParameter
        {
            string Key { get; }
            object Read(object input);
            void Write(object input, string raw);
        }

        internal class FieldInfoParameter : IParameter
        {
            private readonly Func<string, object> _converter;
            private readonly FieldInfo _field;

            internal FieldInfoParameter(FieldInfo field, string key = null)
            {
                _field = field;
                Key = key ?? field.Name;

                _converter = UrlGraph.Conversions.FindConverter(_field.FieldType);
            }

            public object Read(object input)
            {
                return _field.GetValue(input);
            }

            public string Key { get; }

            public void Write(object input, string raw)
            {
                _field.SetValue(input, _converter(raw));
            }
        }

        internal class PropertyInfoParameter : IParameter
        {
            private readonly Func<string, object> _converter;
            private readonly PropertyInfo _property;

            internal PropertyInfoParameter(PropertyInfo property, string key = null)
            {
                _property = property;
                Key = key ?? property.Name;

                _converter = UrlGraph.Conversions.FindConverter(_property.PropertyType);
            }

            public object Read(object input)
            {
                return _property.GetValue(input);
            }

            public string Key { get; }

            public void Write(object input, string raw)
            {
                _property.SetValue(input, _converter(raw));
            }
        }
    }
}