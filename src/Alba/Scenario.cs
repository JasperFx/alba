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
    // SAMPLE: IScenarioResult
    public interface IScenarioResult
    {
        /// <summary>
        ///     Helpers to interrogate or read the HttpResponse.Body
        ///     of the request
        /// </summary>
        HttpResponseBody ResponseBody { get; }

        /// <summary>
        ///     The raw HttpContext used during the scenario
        /// </summary>
        HttpContext Context { get; }
    }
    // ENDSAMPLE


    internal class ScenarioResult : IScenarioResult
    {
        public ScenarioResult(HttpContext context, AlbaHost albaHost)
        {
            Context = context;
            ResponseBody = new HttpResponseBody(albaHost, context);
        }

        public HttpResponseBody ResponseBody { get; }
        public HttpContext Context { get; }
    }


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
        ///     Shorthand alternative to ConfigureHttpContext
        /// </summary>
        public Action<HttpContext> Configure
        {
            set => _setups.Add(value);
        }


        public HttpRequestBody Body { get; }

        public IUrlExpression Get
        {
            get
            {
                Configure = context => context.HttpMethod("GET");
                return this;
            }
        }

        public IUrlExpression Put
        {
            get
            {
                Configure = context => context.HttpMethod("PUT");
                return this;
            }
        }

        public IUrlExpression Delete
        {
            get
            {
                Configure = context => context.HttpMethod("DELETE");
                return this;
            }
        }

        public IUrlExpression Post
        {
            get
            {
                Configure = context => context.HttpMethod("POST");
                return this;
            }
        }

        public IUrlExpression Patch
        {
            get
            {
                Configure = context => context.HttpMethod("PATCH");
                return this;
            }
        }

        public IUrlExpression Head
        {
            get
            {
                Configure = context => context.HttpMethod("HEAD");
                return this;
            }
        }

        internal List<Claim> Claims { get; } = new();
        internal Exception? Exception { get; set; }


        SendExpression IUrlExpression.Url(string relativeUrl)
        {
            Configure = context => context.RelativeUrl(relativeUrl);
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

        public SendExpression Text(string text)
        {
            Body.TextIs(text);
            Configure = context => context.Request.ContentType = MimeType.Text.Value;
            Configure = context => context.Request.ContentLength = text.Length;

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


        public void WriteFormData(Dictionary<string, string> input)
        {
            Configure = c => c.WriteFormData(input);
        }


        public HeaderExpectations Header(string headerKey)
        {
            return new(this, headerKey);
        }

        internal void Rewind()
        {
            Configure = context => context.Request.Body.Position = 0;
        }

        /// <summary>
        ///     Only for internal Alba testing, but this writes its input
        ///     to an HttpContext
        /// </summary>
        /// <param name="context"></param>
        public void SetupHttpContext(HttpContext context)
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
            Configure = c => c.Request.Headers[headerKey] = value;
        }

        /// <summary>
        ///     Remove all values for a request header
        /// </summary>
        /// <param name="headerKey"></param>
        public void RemoveRequestHeader(string headerKey)
        {
            Configure = c => c.Request.Headers.Remove(headerKey);
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
            Configure = c => c.SetBearerToken(jwt);
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