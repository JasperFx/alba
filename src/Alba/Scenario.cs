using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using Alba.Assertions;
using Baseline;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
#nullable enable
namespace Alba
{
    /// <summary>
    /// Models both the setup and expectations for a single HTTP request made through
    /// an AlbaHost
    /// </summary>
    public class Scenario : IUrlExpression
    {
        private readonly ScenarioAssertionException _assertionRecords = new();

        private readonly IList<IScenarioAssertion> _assertions = new List<IScenarioAssertion>();
        private readonly IList<Action<HttpContext>> _setups = new List<Action<HttpContext>>();
        private readonly AlbaHost _system;
        private int _expectedStatusCode = 200;
        private bool _ignoreStatusCode;
        
        internal Scenario(AlbaHost system)
        {
            _system = system ?? throw new ArgumentNullException(nameof(system));
            Body = new HttpRequestBody(system, this);

            ConfigureHttpContext(c => { c.Request.Body = new MemoryStream(); });
        }

        internal Dictionary<string, object> Items { get; } = new();

        /// <summary>
        /// Helpers to write content to the HttpRequest
        /// </summary>
        public HttpRequestBody Body { get; }

        /// <summary>
        /// Specify an HTTP GET Url
        /// </summary>
        public IUrlExpression Get
        {
            get
            {
                ConfigureHttpContext(context => context.HttpMethod("GET"));
                return this;
            }
        }

        
        /// <summary>
        /// Specify an HTTP PUT Url
        /// </summary>
        public IUrlExpression Put
        {
            get
            {
                ConfigureHttpContext(context => context.HttpMethod("PUT"));
                return this;
            }
        }

        /// <summary>
        /// Specify an HTTP DELETE Url
        /// </summary>
        public IUrlExpression Delete
        {
            get
            {
                ConfigureHttpContext(context => context.HttpMethod("DELETE"));
                return this;
            }
        }

        /// <summary>
        /// Specify an HTTP POST Url
        /// </summary>
        public IUrlExpression Post
        {
            get
            {
                ConfigureHttpContext(context => context.HttpMethod("POST"));
                return this;
            }
        }

        /// <summary>
        /// Specify an HTTP PATCH Url
        /// </summary>
        public IUrlExpression Patch
        {
            get
            {
                ConfigureHttpContext(context => context.HttpMethod("PATCH"));
                return this;
            }
        }

        /// <summary>
        /// Specify an HTTP HEAD Url
        /// </summary>
        public IUrlExpression Head
        {
            get
            {
                ConfigureHttpContext(context => context.HttpMethod("HEAD"));
                return this;
            }
        }

        internal List<Claim> Claims { get; } = new();
        internal Exception? Exception { get; set; }


        SendExpression IUrlExpression.Url(string relativeUrl)
        {
            ConfigureHttpContext(context => context.RelativeUrl(relativeUrl));
            return new SendExpression(this);
        }

        SendExpression IUrlExpression.Json<T>(T input)
        {
            WriteRequestBody(input, MimeType.Json.Value);

            ConfigureHttpContext(x => x.Accepts(MimeType.Json.Value));

            return new SendExpression(this);
        }

        SendExpression IUrlExpression.Xml<T>(T input)
        {
            Body.XmlInputIs(input);

            return new SendExpression(this);
        }

        SendExpression IUrlExpression.FormData<T>(T target)
        {
            var values = new Dictionary<string, string>();

            typeof(T).GetProperties().Where(x => x.CanWrite && x.CanRead).Each(prop =>
            {
                var rawValue = prop.GetValue(target, null);

                values.Add(prop.Name, rawValue?.ToString() ?? string.Empty);
            });

            typeof(T).GetFields().Each(field =>
            {
                var rawValue = field.GetValue(target);

                values.Add(field.Name, rawValue?.ToString() ?? string.Empty);
            });

            Body.WriteFormData(values);

            return new SendExpression(this);
        }

        SendExpression IUrlExpression.FormData(Dictionary<string, string> input)
        {
            Body.WriteFormData(input);

            return new SendExpression(this);
        }

        /// <summary>
        /// Write the supplied text to the body of the request
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public SendExpression Text(string text)
        {
            Body.TextIs(text);
            ConfigureHttpContext(context => context.Request.ContentType = MimeType.Text.Value);
            ConfigureHttpContext(context => context.Request.ContentLength = text.Length);

            return new SendExpression(this);
        }

        /// <summary>
        ///     Register any kind of custom setup of the HttpContext within the request
        /// </summary>
        /// <param name="configure"></param>
        public void ConfigureHttpContext(Action<HttpContext> configure)
        {
            _setups.Add(configure);
        }

        /// <summary>
        /// Write the supplied input model to the request body using the configured
        /// formatter in the underlying application that supports the supplied content type
        /// </summary>
        /// <param name="input">An input model that should be serialized to the HTTP request body</param>
        /// <param name="contentType">Like application/json or text/xml</param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void WriteRequestBody<T>(T input, string contentType)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            
            ConfigureHttpContext(c =>
            {
                var formatter = _system.Outputs[contentType];
                if (formatter == null)
                {
                    throw new InvalidOperationException(
                        $"Alba was not able to find a registered formatter for content type '{contentType}'. Either specify the body contents explicitly, or try registering 'services.AddMvcCore()'");
                }

                var stubContext = new DefaultHttpContext();
                var stream = new RewindableStream();
                stubContext.Response.Body = stream; // Has to be rewindable
                var writer = new StreamWriter(stream);

                var outputContext =
                    new OutputFormatterWriteContext(stubContext, (s, e) => writer, typeof(T), input);

                formatter.WriteAsync(outputContext).GetAwaiter().GetResult();

                c.Request.ContentType = contentType;
                c.Request.Body = stream;
                c.Request.Body.Position = 0;
                c.Request.ContentLength = c.Request.Body.Length;
            });
        }

        /// <summary>
        ///     Add an assertion to the Scenario that will be executed after the request
        /// </summary>
        /// <param name="assertion"></param>
        /// <returns></returns>
        public Scenario AssertThat(IScenarioAssertion assertion)
        {
            _assertions.Add(assertion);

            return this;
        }

        internal void RunAssertions(HttpContext context)
        {
            if (!_ignoreStatusCode)
            {
                new StatusCodeAssertion(_expectedStatusCode).Assert(this, context, _assertionRecords);
            }

            foreach (var assertion in _assertions) assertion.Assert(this, context, _assertionRecords);

            _assertionRecords.AssertAll();
        }

        /// <summary>
        ///     Verify the expected Http Status Code
        /// </summary>
        /// <param name="httpStatusCode"></param>
        /// <returns></returns>
        public Scenario StatusCodeShouldBe(HttpStatusCode httpStatusCode)
        {
            _expectedStatusCode = (int) httpStatusCode;
            return this;
        }

        /// <summary>
        ///     Verify the expected Http Status Code
        /// </summary>
        /// <returns></returns>
        public void StatusCodeShouldBe(int statusCode)
        {
            _expectedStatusCode = statusCode;
        }

        /// <summary>
        ///     Just ignore the Http Status Code when doing assertions against
        ///     the response
        /// </summary>
        public void IgnoreStatusCode()
        {
            _ignoreStatusCode = true;
        }


        /// <summary>
        /// Write the dictionary as form data to the HTTP request body
        /// and set the matching request content types
        /// </summary>
        /// <param name="input"></param>
        public void WriteFormData(Dictionary<string, string> input)
        {
            ConfigureHttpContext(c => c.WriteFormData(input));
        }


        /// <summary>
        /// Specify an expectation for the response headers
        /// </summary>
        /// <param name="headerKey"></param>
        /// <returns></returns>
        public HeaderExpectations Header(string headerKey)
        {
            return new(this, headerKey);
        }

        internal void Rewind()
        {
            ConfigureHttpContext(context => context.Request.Body.Position = 0);
        }

        /// <summary>
        ///     Only for internal Alba testing, but this writes its input
        ///     to an HttpContext
        /// </summary>
        /// <param name="context"></param>
        internal void SetupHttpContext(HttpContext context)
        {
            foreach (var setup in _setups) setup(context);
        }

        /// <summary>
        ///     Set a value for a request header
        /// </summary>
        /// <param name="headerKey"></param>
        /// <param name="value"></param>
        [Obsolete("Prefer the WithRequestHeader() method, and this will be removed in Alba v6")]
        public void SetRequestHeader(string headerKey, string value)
        {
            WithRequestHeader(headerKey, value);
        }

        /// <summary>
        ///     Set a value for a request header
        /// </summary>
        /// <param name="headerKey"></param>
        /// <param name="value"></param>
        public void WithRequestHeader(string headerKey, string value)
        {
            ConfigureHttpContext(c => c.Request.Headers[headerKey] = value);
        }

        /// <summary>
        ///     Remove all values for a request header
        /// </summary>
        /// <param name="headerKey"></param>
        public void RemoveRequestHeader(string headerKey)
        {
            ConfigureHttpContext(c => c.Request.Headers.Remove(headerKey));
        }

        /// <summary>
        ///     Add an additional claim to the HttpContext, but this requires using the JwtSecurityStub
        /// </summary>
        /// <param name="claim"></param>
        public void WithClaim(Claim claim)
        {
            Claims.Add(claim);
        }

        /// <summary>
        ///     Set the Authorization header value to "Bearer [jwt]" on the HTTP request
        /// </summary>
        /// <param name="jwt"></param>
        public void WithBearerToken(string jwt)
        {
            ConfigureHttpContext(c => c.SetBearerToken(jwt));
        }

        internal class RewindableStream : MemoryStream
        {
            protected override void Dispose(bool disposing)
            {
                // Nothing!
            }
        }
    }
}
