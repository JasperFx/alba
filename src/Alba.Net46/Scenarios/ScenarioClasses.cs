using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using Alba.Scenarios.Assertions;
using Baseline;
using Environment = System.Collections.Generic.Dictionary<string, object>;

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
            Request = new Environment();
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

        public void Assert()
        {
            if (!_ignoreStatusCode)
            {
                new StatusCodeAssertion(_expectedStatusCode).Assert(this, _assertionRecords);
            }

            _assertions.Each(x => x.Assert(this, _assertionRecords));


            _assertionRecords.AssertValid();
        }

        public HttpRequestBody Body => new HttpRequestBody(Support, Request);

        public IScenarioSupport Support { get; }

        public Environment Request { get; }

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

            Request.RequestHeaders().ContentType(MimeType.HttpFormMimetype);

            Body.WriteFormData(input);

            return new SendExpression(Request);
        }
    }


    public class HttpRequestBody
    {
        private readonly IScenarioSupport _support;
        private readonly Environment _parent;

        public HttpRequestBody(IScenarioSupport support, Environment parent)
        {
            _support = support;
            _parent = parent;
        }

        public void XmlInputIs(object target)
        {
            var serializer = new XmlSerializer(target.GetType());
            var stream = _parent.RequestBody();
            serializer.Serialize(stream, target);
            stream.Position = 0;
        }

        public void JsonInputIs(object target)
        {
            string json = _support.ToJson(target);

            JsonInputIs(json);
        }

        public void JsonInputIs(string json)
        {
            var stream = _parent.RequestBody();

            var writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();

            stream.Position = 0;
        }

        public void WriteFormData<T>(T target) where T : class
        {
            var values = new NameValueCollection();

            typeof (T).GetProperties().Where(x => x.CanWrite && x.CanRead).Each(prop =>
            {
                var rawValue = prop.GetValue(target, null);

                values.Add(prop.Name, rawValue?.ToString() ?? string.Empty);
            });

            typeof (T).GetFields().Each(field =>
            {
                var rawValue = field.GetValue(target);

                values.Add(field.Name, rawValue?.ToString() ?? string.Empty);
            });

            _parent.RequestHeaders().ContentType(MimeType.HttpFormMimetype);
            _parent.WriteFormData(values);
        }

        public void ReplaceBody(Stream stream)
        {
            stream.Position = 0;
            _parent.Append(OwinConstants.RequestBodyKey, stream);
        }

    }
}