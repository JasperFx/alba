using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Alba.Routing;
using Baseline;
using Environment = System.Collections.Generic.Dictionary<string, object>;

namespace Alba.Scenarios
{
    public interface IScenarioSupport
    {
        string RootUrl { get; }
        T Get<T>();

        string ToJson(object document);
        T FromJson<T>(string json);
        T FromJson<T>(Stream stream);

        Task Invoke(Environment env);

        IUrlRegistry Urls { get; }
    }

    public static class ScenarioExtensions
    {
        public static HttpResponseBody Scenario(this IScenarioSupport support, Action<Scenario> configuration)
        {
            var scenario = new Scenario(support);
            configuration(scenario);

            support.Invoke(scenario.Request).Wait();

            return new HttpResponseBody(support, scenario.Request);
        }
    }

    public interface IAssertion
    {
        void Assert(Scenario scenario, ScenarioAssertionException ex);
    }

    public class Scenario : IUrlExpression
    {
        private readonly ScenarioAssertionException _assertion;
        private readonly IList<IAssertion> _assertions = new List<IAssertion>(); 

        public Scenario(IScenarioSupport support)
        {
            Support = support;
            Request = new Environment();
            Request.FullUrl(support.RootUrl);
            Request.RequestHeaders().Accepts("*/*");

            Request.Add(OwinConstants.RequestBodyKey, new MemoryStream());

            _assertion = new ScenarioAssertionException();
        }

        public void Assert()
        {
            _assertion.AssertValid();
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

    public interface IUrlExpression
    {
        SendExpression Action<T>(Expression<Action<T>> expression);
        SendExpression Url(string relativeUrl);
        SendExpression Input<T>(T input = null) where T : class;

        SendExpression Json<T>(T input) where T : class;
        SendExpression Xml<T>(T input) where T : class;

        SendExpression FormData<T>(T input) where T : class;
    }

    public class SendExpression
    {
        private readonly Environment Request;

        public SendExpression(Environment request)
        {
            Request = request;
        }

        public SendExpression ContentType(string contentType)
        {
            Request.RequestHeaders().ContentType(contentType);
            return this;
        }

        public SendExpression Accepts(string accepts)
        {
            Request.RequestHeaders().Accepts(accepts);
            return this;
        }

        public SendExpression Etag(string etag)
        {
            Request.RequestHeaders().Replace(HttpRequestHeaders.IfNoneMatch, etag);
            return this;
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


    [Serializable]
    public class ScenarioAssertionException : Exception
    {
        private readonly IList<string> _messages = new List<string>();

        public ScenarioAssertionException()
        {
        }

        protected ScenarioAssertionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public void Add(string message)
        {
            _messages.Add(message);
        }

        public void AssertValid()
        {
            if (_messages.Any())
            {
                throw this;
            }
        }

        public IEnumerable<string> Messages => _messages;

        public override string Message
        {
            get
            {
                var writer = new StringWriter();
                _messages.Each(x => writer.WriteLine(x));

                if (Body.IsNotEmpty())
                {
                    writer.WriteLine();
                    writer.WriteLine();
                    writer.WriteLine("Actual body text was:");
                    writer.WriteLine();
                    writer.WriteLine(Body);
                }

                return writer.ToString();
            }
        }

        public string Body { get; set; }
    }


}