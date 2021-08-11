using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Security.Claims;
using Alba.Assertions;
using Baseline;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Alba
{
    // SAMPLE: IScenarioResult
    public interface IScenarioResult
    {
        /// <summary>
        /// Helpers to interrogate or read the HttpResponse.Body
        /// of the request
        /// </summary>
        HttpResponseBody ResponseBody { get; }

        /// <summary>
        /// The raw HttpContext used during the scenario
        /// </summary>
        HttpContext Context { get; }
    }
    // ENDSAMPLE


    internal class ScenarioResult : IScenarioResult
    {
        public ScenarioResult(HttpContext context, IAlbaHost albaHost)
        {
            Context = context;
            ResponseBody = new HttpResponseBody(albaHost, context);
        }

        public HttpResponseBody ResponseBody { get; }
        public HttpContext Context { get; }
    }
    
    
    public class Scenario : IUrlExpression
    {
        public static readonly int DefaultBufferSize = 16 * 1024;

        private readonly ScenarioAssertionException _assertionRecords = new ScenarioAssertionException();
        private readonly IAlbaHost _system;
        private readonly IList<Action<HttpContext>> _setups = new List<Action<HttpContext>>();

        private readonly IList<IScenarioAssertion> _assertions = new List<IScenarioAssertion>();
        private int _expectedStatusCode = 200;
        private bool _ignoreStatusCode;

        internal Scenario(IAlbaHost system)
        {
            _system = system;
            Body = new HttpRequestBody(system, this);
            
            ConfigureHttpContext(c =>
            {
                c.Request.Body = new MemoryStream();
                c.Response.Body = new MemoryStream();
            });
        }

        /// <summary>
        /// Register any kind of custom setup of the HttpContext within the request
        /// </summary>
        /// <param name="configure"></param>
        public void ConfigureHttpContext(Action<HttpContext> configure)
        {
            _setups.Add(configure);    
        }

        /// <summary>
        /// Using the configured formatters in this system, write the input object to the
        /// request stream 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="contentType"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Scenario WithInputBody<T>(T input, string contentType)
        {
            ConfigureHttpContext(c =>
            {
                c.Request.ContentType = contentType;
                
                // TODO -- memoize the formatters
                var options = _system.Services.GetRequiredService<IOptions<MvcOptions>>();
                var formatter = options.Value.OutputFormatters.OfType<OutputFormatter>()
                    .FirstOrDefault(x => x.SupportedMediaTypes.Contains(contentType));

                var factory = _system.Services.GetRequiredService<IHttpResponseStreamWriterFactory>();
                
                // You *have* to use a separate HttpContext or all kinds of other things will get screwed up
                // by writing to the response body of the original context
                var stubHttpContext = new DefaultHttpContext();
                stubHttpContext.Response.Body = new MemoryStream();
                var context = new OutputFormatterWriteContext(stubHttpContext, factory.CreateWriter, typeof(T), input);
                
                // TODO -- switch to using async all the way down.
                formatter.WriteAsync(context).GetAwaiter().GetResult();

                c.Request.Body = stubHttpContext.Response.Body;
                c.Request.Body.Position = 0;
                c.Request.ContentLength = c.Request.Body.Length;

            });

            return this;
        }
        
        
        
        /// <summary>
        /// Add an assertion to the Scenario that will be executed after the request
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
            if (InvocationException != null)
            {
                ExceptionDispatchInfo.Throw(InvocationException);
            }
            
            if (!_ignoreStatusCode)
            {
                new StatusCodeAssertion(_expectedStatusCode).Assert(this, context, _assertionRecords);
            }

            foreach (var assertion in _assertions)
            {
                assertion.Assert(this, context, _assertionRecords);
            }

            _assertionRecords.AssertAll();
        }

        /// <summary>
        /// Verify the expected Http Status Code
        /// </summary>
        /// <param name="httpStatusCode"></param>
        /// <returns></returns>
        public Scenario StatusCodeShouldBe(HttpStatusCode httpStatusCode)
        {
            _expectedStatusCode = (int)httpStatusCode;
            return this;
        }

        /// <summary>
        /// Verify the expected Http Status Code
        /// </summary>
        /// <returns></returns>
        public void StatusCodeShouldBe(int statusCode)
        {
            _expectedStatusCode = statusCode;
        }

        /// <summary>
        /// Just ignore the Http Status Code when doing assertions against
        /// the response
        /// </summary>
        public void IgnoreStatusCode()
        {
            _ignoreStatusCode = true;
        }



        public void WriteFormData(Dictionary<string, string> input)
        {
            ConfigureHttpContext(c => c.WriteFormData(input));
        }
        
        SendExpression IUrlExpression.Action<T>(Expression<Action<T>> expression)
        {
            ConfigureHttpContext(context => context.RelativeUrl(_system.Urls.UrlFor(expression, context.Request.Method)));
            return new SendExpression(this);
        }



        SendExpression IUrlExpression.Url(string relativeUrl)
        {
            ConfigureHttpContext(context => context.RelativeUrl(relativeUrl));
            return new SendExpression(this);
        }

        SendExpression IUrlExpression.Input<T>(T input)
        {
            ConfigureHttpContext(context =>
            {
                if (!(_system.Urls is NulloUrlLookup))
                {
                    var url = input == null
                        ? _system.Urls.UrlFor<T>(context.Request.Method)
                        : _system.Urls.UrlFor(input, context.Request.Method);

                    context.RelativeUrl(url);
                }
                else
                {
                    context.RelativeUrl(null);
                }
            });
            
            return new SendExpression(this);
        }

        SendExpression IUrlExpression.Json<T>(T input)
        {
            this.As<IUrlExpression>().Input(input);

            ConfigureHttpContext(c =>
            {
                c.Accepts(MimeType.Json.Value);
            });
            
            WithInputBody(input, MimeType.Json.Value);
            //Body.JsonInputIs(_system.ToJson(input));

            return new SendExpression(this);
        }

        SendExpression IUrlExpression.Xml<T>(T input) 
        {
            this.As<IUrlExpression>().Input(input);

            Body.XmlInputIs(input);

            return new SendExpression(this);
        }

        SendExpression IUrlExpression.FormData<T>(T target)
        {
            this.As<IUrlExpression>().Input(target);

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
            this.As<IUrlExpression>().Input(input);

            Body.WriteFormData(input);

            return new SendExpression(this);
        }

        public SendExpression Text(string text)
        {
            Body.TextIs(text);
            ConfigureHttpContext(context =>
            {
                context.Request.ContentType = MimeType.Text.Value;
                context.Request.ContentLength = text.Length;
            });

            return new SendExpression(this);
        }


        public HttpRequestBody Body { get; }


        public HeaderExpectations Header(string headerKey)
        {
            return new HeaderExpectations(this, headerKey);
        }

        public IUrlExpression Get
        {
            get
            {
                ConfigureHttpContext(context => context.HttpMethod("GET"));
                return this;
            }
        }

        public IUrlExpression Put
        {
            get
            {
                ConfigureHttpContext(context => context.HttpMethod("PUT"));
                return this;
            }
        }

        public IUrlExpression Delete
        {
            get
            {
                ConfigureHttpContext(context => context.HttpMethod("DELETE"));
                return this;
            }
        }

        public IUrlExpression Post
        {
            get
            {
                ConfigureHttpContext(context => context.HttpMethod("POST"));
                return this;
            }
        }

        public IUrlExpression Patch
        {
            get
            {
                ConfigureHttpContext(context => context.HttpMethod("PATCH"));
                return this;
            }
        }

        public IUrlExpression Head
        {
            get
            {
                ConfigureHttpContext(context => context.HttpMethod("HEAD"));
                return this;
            }
        }

        internal void Rewind()
        {
            ConfigureHttpContext(context => context.Request.Body.Position = 0);
        }

        /// <summary>
        /// Only for internal Alba testing, but this writes its input
        /// to an HttpContext
        /// </summary>
        /// <param name="context"></param>
        internal void SetupHttpContext(HttpContext context)
        {
            foreach (var setup in _setups)
            {
                setup(context);
            }
        }

        /// <summary>
        /// Set a value for a request header
        /// </summary>
        /// <param name="headerKey"></param>
        /// <param name="value"></param>
        [Obsolete("Prefer the WithRequestHeader() method, and this will be removed in Alba v6")]
        public void SetRequestHeader(string headerKey, string value)
        {
            WithRequestHeader(headerKey, value);
        }
        
        /// <summary>
        /// Set a value for a request header
        /// </summary>
        /// <param name="headerKey"></param>
        /// <param name="value"></param>
        public void WithRequestHeader(string headerKey, string value)
        {
            ConfigureHttpContext(c => c.Request.Headers[headerKey] = value);
        }

        /// <summary>
        /// Remove all values for a request header
        /// </summary>
        /// <param name="headerKey"></param>
        public void RemoveRequestHeader(string headerKey)
        {
            ConfigureHttpContext(c => c.Request.Headers.Remove(headerKey));
        }

        /// <summary>
        /// Add an additional claim to the HttpContext, but this requires using the JwtSecurityStub
        /// </summary>
        /// <param name="claim"></param>
        public void WithClaim(Claim claim)
        {
            Claims.Add(claim);
        }

        internal List<Claim> Claims { get; } = new List<Claim>();
        internal Exception InvocationException { get; set; }

        /// <summary>
        /// Set the Authorization header value to "Bearer [jwt]" on the HTTP request
        /// </summary>
        /// <param name="jwt"></param>
        public void WithBearerToken(string jwt)
        {
            ConfigureHttpContext(c => c.SetBearerToken(jwt));
        }
    }
}