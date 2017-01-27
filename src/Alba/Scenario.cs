using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Alba.Assertions;
using Baseline;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Alba
{
    public interface IScenarioResult
    {
        HttpResponseBody ResponseBody { get; }
        HttpContext Context { get; }
    }

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

        public HttpContext Context { get; }

        internal async Task RunBeforeActions()
        {
            foreach (var before in _befores)
            {
                await before(Context).ConfigureAwait(false);
            }
        }

        internal async Task RunAfterActions()
        {
            foreach (var before in _afters)
            {
                await before(Context).ConfigureAwait(false);
            }
        }

        // holds on to the http context & IApplicationServer


        public Scenario AssertThat(IScenarioAssertion assertion)
        {
            _assertions.Add(assertion);

            return this;
        }


        public void Before<T>(Func<T, HttpContext, Task> action)
        {
            throw new NotImplementedException();
        }

        public void After<T>(Func<T, HttpContext, Task> action)
        {
            throw new NotImplementedException();
        }

        public void Before<T>(Func<HttpContext, Task> action)
        {
            throw new NotImplementedException();
        }

        public void After<T>(Func<HttpContext, Task> action)
        {
            throw new NotImplementedException();
        }

        public void Before<T>(Func<T, Task> action)
        {
            throw new NotImplementedException();
        }

        public void After<T>(Func<T, Task> action)
        {
            throw new NotImplementedException();
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

        public Scenario StatusCodeShouldBe(HttpStatusCode httpStatusCode)
        {
            _expectedStatusCode = (int)httpStatusCode;
            return this;
        }

        public void StatusCodeShouldBe(int statusCode)
        {
            _expectedStatusCode = statusCode;
        }

        public void IgnoreStatusCode()
        {
            _ignoreStatusCode = true;
        }




        
        SendExpression IUrlExpression.Action<T>(Expression<Action<T>> expression)
        {
            Context.RelativeUrl(_system.UrlFor(expression, Context.Request.Method));
            return new SendExpression(Context);
        }



        SendExpression IUrlExpression.Url(string relativeUrl)
        {
            Context.RelativeUrl(relativeUrl);
            return new SendExpression(Context);
        }

        SendExpression IUrlExpression.Input<T>(T input)
        {
            if (_system.SupportsUrlLookup)
            {
                var url = input == null
                    ? _system.UrlFor<T>(Context.Request.Method)
                    : _system.UrlFor(input, Context.Request.Method);

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

        SendExpression IUrlExpression.FormData<T>(T input)
        {
            this.As<IUrlExpression>().Input(input);

            Body.WriteFormData(input);

            return new SendExpression(Context);
        }

        public SendExpression Text(string text)
        {
            Body.TextIs(text);
            Context.Request.ContentType = "text/plain";

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