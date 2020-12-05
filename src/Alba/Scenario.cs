using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using Alba.Assertions;
using Baseline;
using Microsoft.AspNetCore.Http;

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
        public ScenarioResult(HttpContext context, ISystemUnderTest systemUnderTest)
        {
            Context = context;
            ResponseBody = new HttpResponseBody(systemUnderTest, context);
        }

        public HttpResponseBody ResponseBody { get; }
        public HttpContext Context { get; }
    }
    
    
    public class Scenario : IUrlExpression
    {
        private readonly ScenarioAssertionException _assertionRecords = new ScenarioAssertionException();
        private readonly ISystemUnderTest _system;
        private readonly IList<Action<HttpContext>> _setups = new List<Action<HttpContext>>();

        private readonly IList<IScenarioAssertion> _assertions = new List<IScenarioAssertion>();
        private int _expectedStatusCode = 200;
        private bool _ignoreStatusCode;

        public Scenario(ISystemUnderTest system)
        {
            _system = system;
            Body = new HttpRequestBody(system, this);
            
            ConfigureHttpContext(c =>
            {
                c.Request.Body = new MemoryStream();
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
        /// Shorthand alternative to ConfigureHttpContext
        /// </summary>
        public Action<HttpContext> Configure
        {
            set => _setups.Add(value);
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
            Configure = c => c.WriteFormData(input);
        }
        
        SendExpression IUrlExpression.Action<T>(Expression<Action<T>> expression)
        {
            Configure = context => context.RelativeUrl(_system.Urls.UrlFor(expression, context.Request.Method));
            return new SendExpression(this);
        }



        SendExpression IUrlExpression.Url(string relativeUrl)
        {
            Configure = context => context.RelativeUrl(relativeUrl);
            return new SendExpression(this);
        }

        SendExpression IUrlExpression.Input<T>(T input)
        {
            Configure = context =>
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
            };
            
            return new SendExpression(this);
        }

        SendExpression IUrlExpression.Json<T>(T input)
        {
            this.As<IUrlExpression>().Input(input);

            Body.JsonInputIs(_system.ToJson(input));

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
            Configure = context => context.Request.ContentType = MimeType.Text.Value;
            Configure = context => context.Request.ContentLength = text.Length;

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

        internal void Rewind()
        {
            Configure = context => context.Request.Body.Position = 0;
        }

        /// <summary>
        /// Only for internal Alba testing, but this writes its input
        /// to an HttpContext
        /// </summary>
        /// <param name="context"></param>
        public void SetupHttpContext(HttpContext context)
        {
            foreach (var setup in _setups)
            {
                setup(context);
            }
        }

        public void SetRequestHeader(string headerKey, string value)
        {
            Configure = c => c.Request.Headers[headerKey] = value;
        }
    }
}