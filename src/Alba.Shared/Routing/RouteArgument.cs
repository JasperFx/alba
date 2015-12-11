using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Alba.Urls;
using Baseline;
using Baseline.Conversion;
using Baseline.Reflection;

namespace Alba.Routing
{
    public class RouteArgument : ISegment
    {
        public static readonly Conversions Conversions = new Conversions();

        private Func<string, object> _converter = x => x;
        private Action<object, object> _writeData = (x, y) => { };
        private Func<object, object> _readData = x => null; 

        private Type _argType;
        private MemberInfo _mappedMember;
        public string Key { get; }
        public int Position { get; }
        public string CanonicalPath()
        {
            return "*";
        }

        public string SegmentPath { get; }
        public bool IsParameter => true;

        public RouteArgument(string key, int position, Type argType = null)
        {
            ArgType = argType ?? typeof (string);
            Key = key;
            Position = position;

            SegmentPath = ":" + Key;
        }

        public MemberInfo MappedMember
        {
            get { return _mappedMember; }
            set
            {
                if (value is FieldInfo)
                {
                    var field = value.As<FieldInfo>();
                    _writeData = (input, val) => field.SetValue(input, val);
                    _readData = input => field.GetValue(input);
                    ArgType = field.FieldType;
                }
                else if (value is PropertyInfo)
                {
                    var prop = value.As<PropertyInfo>();
                    _writeData = (input, val) => prop.SetValue(input, val);
                    _readData = input => prop.GetValue(input);
                    ArgType = prop.PropertyType;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "We don't serve your kind here! Only FieldInfo and PropertyInfo's are supported");   
                }

                _mappedMember = value;
            }
        }

        // TODO -- add a version that uses an Expression
        public void MapToField<T>(string fieldName)
        {
            var field = typeof (T).GetFields().FirstOrDefault(x => x.Name == fieldName);
            MappedMember = field;
        }

        
        public void MapToProperty<T>(Expression<Func<T, object>> property)
        {
            MappedMember = ReflectionHelper.GetProperty(property);
        }

        public Type ArgType
        {
            get { return _argType; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _argType = value;

                _converter = Conversions.FindConverter(ArgType);
            }
        }

        public void ApplyRouteDataToInput(object input, IDictionary<string, object> routeData)
        {
            var raw =  routeData[Key];
            _writeData(input, raw);
        }

        public string ReadRouteDataFromInput(object input)
        {
            return _readData(input)?.ToString() ?? string.Empty;
        }

        public void SetValues(IDictionary<string, object> env, string[] segments)
        {
            var raw = segments[Position];
            env.SetRouteData(Key, _converter(raw));
        }

        protected bool Equals(RouteArgument other)
        {
            return string.Equals(Key, other.Key) && Position == other.Position;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RouteArgument) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Key != null ? Key.GetHashCode() : 0)*397) ^ Position;
            }
        }

        public override string ToString()
        {
            return $"{Key}:{Position}";
        }
    }
}