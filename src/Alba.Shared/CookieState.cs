using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Baseline;

namespace Alba
{
    public class CookieState
    {
        private string _name;
        private string _value;
        private readonly NameValueCollection _values = new NameValueCollection();

        public static CookieState Parse(string name, string text)
        {
            if (text.IsEmpty())
            {
                return new CookieState(name, "NONE");
            }

            var values = HttpUtility.ParseQueryString(text);
            if (values.Count == 1 && values.AllKeys.Single().IsEmpty())
            {
                return new CookieState(name, text);
            }

            return new CookieState(name, values);
            
        }

        public static CookieState For(Segment segment)
        {
            return Parse(segment.Key, segment.Value);
        }

        public CookieState(string name)
        {
            Name = name;
        }

        public CookieState(string name, string value) : this(name)
        {
            Value = value;
        }

        private CookieState(string name, NameValueCollection values)
        {
            Name = name;
            _values = values;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value.IsEmpty())
                {
                    throw new ArgumentNullException("value");
                }

                _name = value;
            }
        }

        public CookieState With(string key, string value)
        {
            this[key] = value;
            return this;
        }

        public string Value
        {
            get { return _value; }
            set
            {
                if (value.IsEmpty()) throw new ArgumentNullException("value");

                _value = value;
            }
        }

        /// <summary>
        /// Gets or sets the cookie value with the specified cookie name, if the cookie data is structured.
        /// </summary>
        /// 
        /// <returns>
        /// The cookie value with the specified cookie name.
        /// </returns>
        public string this[string subName]
        {
            get
            {
                return _values[subName];
            }
            set
            {
                if (_value.IsNotEmpty())
                {
                    throw new InvalidOperationException("A cookie can have a single value or structured sub-values, but not both");
                }

                _values[subName] = value;
            }
        }

        public NameValueCollection Values
        {
            get { return _values; }
        }

        public void Write(StringBuilder builder)
        {
            builder.Append(Name);
            builder.Append("=");

            if (Value.IsNotEmpty())
            {
                builder.Append(Value);
            }
            else
            {
                builder.Append(_values.ToString());
            }
        }

        /// <summary>
        /// Returns the string representation the current object.
        /// </summary>
        /// 
        /// <returns>
        /// The string representation the current object.
        /// </returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            Write(builder);

            return builder.ToString();
        }
    }
}