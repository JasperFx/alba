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
    public class Scenario : IUrlExpression
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


        public void AssertThat(IScenarioAssertion assertion)
        {
            _assertions.Add(assertion);
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

        public void StatusCodeShouldBe(HttpStatusCode httpStatusCode)
        {
            _expectedStatusCode = (int)httpStatusCode;
        }

        public void StatusCodeShouldBe(int statusCode)
        {
            _expectedStatusCode = statusCode;
        }

        public void IgnoreStatusCode()
        {
            _ignoreStatusCode = true;
        }

        public void ContentShouldContain(string text)
        {
            _assertions.Add(new BodyContainsAssertion(text));
        }

        public void ContentShouldNotContain(string text)
        {
            _assertions.Add(new BodyDoesNotContainAssertion(text));
        }

        public void ContentShouldBe(string exactContent)
        {
            _assertions.Add(new BodyTextAssertion(exactContent));
        }

        public void StatusCodeShouldBeOk()
        {
            StatusCodeShouldBe(HttpStatusCode.OK);
        }

        public void ContentTypeShouldBe(MimeType mimeType)
        {
            ContentTypeShouldBe(mimeType.Value);
        }


        public void ContentTypeShouldBe(string mimeType)
        {
            Header("content-type").SingleValueShouldEqual(mimeType);
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
            var url = input == null
                ? _system.UrlFor<T>(Context.Request.Method)
                : _system.UrlFor(input, Context.Request.Method);

            Context.RelativeUrl(url);

            return new SendExpression(Context);
        }

        SendExpression IUrlExpression.Json<T>(T input)
        {
            this.As<IUrlExpression>().Input(input);
            Body.JsonInputIs(_system.ToJson(input));

            Context.Request.Headers["content-type"] = "application/json";
            Context.Request.Headers["accept"] = "application/json";

            return new SendExpression(Context);
        }

        SendExpression IUrlExpression.Xml<T>(T input) 
        {
            var writer = new StringWriter();

            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(writer, input);

            var bytes = Encoding.UTF8.GetBytes(writer.ToString());

            var stream = Context.Request.Body;
            stream.Write(bytes, 0, bytes.Length);
            stream.Position = 0;

            Context.Request.ContentType("application/xml");
            Context.Accepts("application/xml");

            this.As<IUrlExpression>().Input(input);

            return new SendExpression(Context);
        }

        SendExpression IUrlExpression.FormData<T>(T input)
        {
            this.As<IUrlExpression>().Input(input);

            Body.WriteFormData(input);

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
    }
}