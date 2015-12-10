using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Baseline;

namespace Alba
{
    public class Cookie
    {
        private readonly IList<CookieState> _states = new List<CookieState>();

        public Cookie()
        {
        }

        public Cookie(string name)
        {
            var state = new CookieState(name);
            _states.Add(state);
        }

        public Cookie(string name, string value)
        {
            var state = new CookieState(name, value);
            _states.Add(state);
        }

        public Cookie(string name, string value, string domain) : this(name, value)
        {
            Domain = domain;
        }

        public IEnumerable<CookieState> States
        {
            get { return _states; }
        }

        public CookieState For(string key)
        {
            return _states.FirstOrDefault(x => x.Name.EqualsIgnoreCase(key));
        }

        public bool Matches(string name)
        {
            return _states.Any(x => x.Name.EqualsIgnoreCase(name));
        }

        /// <summary>
        ///     Gets or sets the expiration date and time for the cookie.
        /// </summary>
        /// <returns>
        ///     The time of day (on the client) at which the cookie expires.
        /// </returns>
        public DateTimeOffset? Expires { get; set; }

        /// <summary>
        ///     Gets or sets the maximum age permitted for a resource.
        /// </summary>
        /// <returns>
        ///     The maximum age permitted for a resource.
        /// </returns>
        public TimeSpan? MaxAge { get; set; }

        /// <summary>
        ///     Gets or sets the domain to associate the cookie with.
        /// </summary>
        /// <returns>
        ///     The name of the domain to associate the cookie with.
        /// </returns>
        public string Domain { get; set; }

        /// <summary>
        ///     Gets or sets the virtual path to transmit with the current cookie.
        /// </summary>
        /// <returns>
        ///     The virtual path to transmit with the cookie.
        /// </returns>
        public string Path { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to transmit the cookie using Secure Sockets Layer (SSL)—that is, over HTTPS only.
        /// </summary>
        /// <returns>
        ///     true to transmit the cookie over an SSL connection (HTTPS); otherwise, false.
        /// </returns>
        public bool Secure { get; set; }

        /// <summary>
        ///     Gets or sets a value that specifies whether a cookie is accessible by client-side script.
        /// </summary>
        /// <returns>
        ///     true if the cookie has the HttpOnly attribute and cannot be accessed through a client-side script; otherwise, false.
        /// </returns>
        public bool HttpOnly { get; set; }

        /// <summary>
        /// This is only usable if there is only one state and one value
        /// </summary>
        public string Value
        {
            get
            {
                if (_states.Count != 1)
                {
                    return null;
                }

                return _states.Single().Value;
            }
            set
            {
                if (_states.Count != 1)
                {
                    throw new InvalidOperationException("This action is only valid for single value cookies");
                }

                _states.Single().Value = value;
            }
        }

        public Cookie Add(CookieState state)
        {
            _states.Add(state);
            return this;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            _states.Each(x => {
                                  x.Write(builder);
                                  builder.Append("; ");
            });

            if (Expires.HasValue)
            {
                builder.Append("expires=");
                string displayedExpiration = Expires.Value.ToString("r", CultureInfo.InvariantCulture);
                builder.Append(displayedExpiration);
                builder.Append("; ");
            }

            if (MaxAge.HasValue)
            {
                builder.Append("max-age=");
                builder.Append(MaxAge.Value.TotalSeconds);
                builder.Append("; ");
            }

            if (Domain.IsNotEmpty())
            {
                builder.Append("domain=");
                builder.Append(Domain);
                builder.Append("; ");
            }

            if (Path.IsNotEmpty())
            {
                builder.Append("path=");
                builder.Append(Path);
                builder.Append("; ");
            }

            if (Secure)
            {
                builder.Append("secure; ");
            }

            if (HttpOnly)
            {
                builder.Append("httponly; ");
            }



            return builder.ToString().TrimEnd(';', ' ');
        }

        public static string DateToString(DateTimeOffset dateTime)
        {
            return dateTime.ToUniversalTime().ToString("r", (IFormatProvider)CultureInfo.InvariantCulture);
        }

        public string GetValue(string name)
        {
            var state = _states.FirstOrDefault(x => x.Name.EqualsIgnoreCase(name));
            return state == null ? null : state.Value;
        }
    }
}