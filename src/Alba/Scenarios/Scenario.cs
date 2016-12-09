using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using Alba.Scenarios.Assertions;
using Baseline;

namespace Alba.Scenarios
{
    public class Scenario : IUrlExpression
    {
        private readonly ScenarioAssertionException _assertionRecords;
        private readonly IList<IScenarioAssertion> _assertions = new List<IScenarioAssertion>();
        private int _expectedStatusCode = 200;
        private bool _ignoreStatusCode;

        public Scenario(IScenarioSupport support)
        {
            Support = support;
            Request = new Dictionary<string, object>();
            if (support != null) Request.FullUrl(support.RootUrl);
            Request.RequestHeaders().Accepts("*/*");

            Request.Add(OwinConstants.RequestBodyKey, new MemoryStream());

            _assertionRecords = new ScenarioAssertionException();
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
            Header("content-length").SingleValueShouldEqual(mimeType);
        }


        public void Assert()
        {
            if (!_ignoreStatusCode)
            {
                new StatusCodeAssertion(_expectedStatusCode).Assert(this, _assertionRecords);
            }

            _assertions.Each(x => x.Assert(this, _assertionRecords));


            _assertionRecords.AssertAll();
        }

        public HttpRequestBody Body => new HttpRequestBody(Support, Request);

        public IScenarioSupport Support { get; }

        public Dictionary<string, object> Request { get; }

        SendExpression IUrlExpression.Action<T>(Expression<Action<T>> expression)
        {
            Request.RelativeUrl(Support.Urls.UrlFor(expression, Request.HttpMethod()));
            return new SendExpression(Request);
        }

        SendExpression IUrlExpression.Url(string relativeUrl)
        {
            Request.RelativeUrl(relativeUrl);
            return new SendExpression(Request);
        }

        SendExpression IUrlExpression.Input<T>(T input)
        {
            var url = input == null
                ? Support.Urls.UrlFor<T>(Request.HttpMethod())
                : Support.Urls.UrlFor(input, Request.HttpMethod());

            Request.RelativeUrl(url);

            return new SendExpression(Request);
        }

        SendExpression IUrlExpression.Json<T>(T input)
        {
            this.As<IUrlExpression>().Input(input);
            Body.JsonInputIs(Support.ToJson(input));

            Request.RequestHeaders().ContentType("application/json");
            Request.RequestHeaders().Accepts("application/json");

            return new SendExpression(Request);
        }

        SendExpression IUrlExpression.Xml<T>(T input)
        {
            var writer = new StringWriter();

            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(writer, input);

            var bytes = Encoding.Default.GetBytes(writer.ToString());

            var stream = Request.RequestBody();
            stream.Write(bytes, 0, bytes.Length);
            stream.Position = 0;

            Request.RequestHeaders().ContentType("application/xml");
            Request.RequestHeaders().Accepts("application/xml");

            this.As<IUrlExpression>().Input(input);

            return new SendExpression(Request);
        }

        SendExpression IUrlExpression.FormData<T>(T input)
        {
            this.As<IUrlExpression>().Input(input);

            Body.WriteFormData(input);

            return new SendExpression(Request);
        }

        public void AssertThat(IScenarioAssertion assertion)
        {
            _assertions.Add(assertion);
        }


        public HeaderExpectations Header(string headerKey)
        {
            return new HeaderExpectations(this, headerKey);
        }


        public IUrlExpression Get
        {
            get
            {
                Request.HttpMethod("GET");
                return this;
            }
        }

        public IUrlExpression Put
        {
            get
            {
                Request.HttpMethod("PUT");
                return this;
            }
        }

        public IUrlExpression Delete
        {
            get
            {
                Request.HttpMethod("DELETE");
                return this;
            }
        }

        public IUrlExpression Post
        {
            get
            {
                Request.HttpMethod("POST");
                return this;
            }
        }

        public IUrlExpression Head
        {
            get
            {
                Request.HttpMethod("HEAD");
                return this;
            }
        }

    }
}