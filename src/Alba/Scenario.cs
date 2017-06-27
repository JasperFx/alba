using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Alba.Assertions;
using Baseline;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

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

    public class Scenario : IUrlExpression, IScenarioResult
    {
        private readonly ScenarioAssertionException _assertionRecords = new ScenarioAssertionException();
        private readonly ISystemUnderTest _system;
        private readonly IList<Func<HttpContext, Task>> _befores = new List<Func<HttpContext, Task>>();
        private readonly IList<Func<HttpContext, Task>> _afters = new List<Func<HttpContext, Task>>();

        private readonly IList<IScenarioAssertion> _assertions = new List<IScenarioAssertion>();
        private int _expectedStatusCode = 200;
        private bool _ignoreStatusCode;

        public Scenario(ISystemUnderTest system, IServiceScope scope)
        {
            _system = system;
            Context = system.CreateContext();
            Context.RequestServices = scope.ServiceProvider;
        }

        HttpResponseBody IScenarioResult.ResponseBody => new HttpResponseBody(_system, Context);

        /// <summary>
        /// The HttpContext for this Scenario
        /// </summary>
        public HttpContext Context { get; }

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

        public HttpRequestBody Body => new HttpRequestBody(_system, Context);


        internal void RunAssertions()
        {
            if (!_ignoreStatusCode)
            {
                new StatusCodeAssertion(_expectedStatusCode).Assert(this, _assertionRecords);
            }

            _assertions.Each(x => x.Assert(this, _assertionRecords));


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




        
        SendExpression IUrlExpression.Action<T>(Expression<Action<T>> expression)
        {
            Context.RelativeUrl(_system.Urls.UrlFor(expression, Context.Request.Method));
            return new SendExpression(Context);
        }



        SendExpression IUrlExpression.Url(string relativeUrl)
        {
            Context.RelativeUrl(relativeUrl);
            return new SendExpression(Context);
        }

        SendExpression IUrlExpression.Input<T>(T input)
        {
            if (!(_system.Urls is NulloUrlLookup))
            {
                var url = input == null
                    ? _system.Urls.UrlFor<T>(Context.Request.Method)
                    : _system.Urls.UrlFor(input, Context.Request.Method);

                Context.RelativeUrl(url);
            }
            else
            {
                Context.RelativeUrl(null);
            }

            return new SendExpression(Context);
        }

        SendExpression IUrlExpression.Json<T>(T input)
        {
            this.As<IUrlExpression>().Input(input);

            Body.JsonInputIs(_system.ToJson(input));

            return new SendExpression(Context);
        }

        SendExpression IUrlExpression.Xml<T>(T input) 
        {
            this.As<IUrlExpression>().Input(input);

            Body.XmlInputIs(input);

            return new SendExpression(Context);
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

            return new SendExpression(Context);
        }

        SendExpression IUrlExpression.FormData(Dictionary<string, string> input)
        {
            this.As<IUrlExpression>().Input(input);

            Body.WriteFormData(input);

            return new SendExpression(Context);
        }

        public SendExpression Text(string text)
        {
            Body.TextIs(text);
            Context.Request.ContentType = MimeType.Text.Value;
            Context.Request.ContentLength = text.Length;

            return new SendExpression(Context);
        }


        public SendExpression QueryString(string paramName, string paramValue)
        {
            Context.Request.QueryString = Context.Request.QueryString.Add(paramName, paramValue);
            return new SendExpression(Context);
        }


        public HeaderExpectations Header(string headerKey)
        {
            return new HeaderExpectations(this, headerKey);
        }

        public IUrlExpression Get
        {
            get
            {
                Context.HttpMethod("GET");
                return this;
            }
        }

        public IUrlExpression Put
        {
            get
            {
                Context.HttpMethod("PUT");
                return this;
            }
        }

        public IUrlExpression Delete
        {
            get
            {
                Context.HttpMethod("DELETE");
                return this;
            }
        }

        public IUrlExpression Post
        {
            get
            {
                Context.HttpMethod("POST");
                return this;
            }
        }

        public IUrlExpression Head
        {
            get
            {
                Context.HttpMethod("HEAD");
                return this;
            }
        }

        internal void Rewind()
        {
            Context.Request.Body.Position = 0;
        }
    }
}