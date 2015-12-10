using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Baseline;

namespace Alba
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class HtmlHeadInjectionMiddleware
    {
        private readonly AppFunc _inner;
        private readonly Func<IDictionary<string, object>, string> _content;

        public static Func<AppFunc, AppFunc> ToMidFunc(Func<IDictionary<string, object>, string> content)
        {
            return env =>
            {
                var middleware = new HtmlHeadInjectionMiddleware(env, content);
                return middleware.Invoke;
            };

        } 

        public HtmlHeadInjectionMiddleware(AppFunc inner, Func<IDictionary<string, object>, string> content)
        {
            _inner = inner;
            _content = content;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            if (!environment.Get<string>(OwinConstants.RequestMethodKey).EqualsIgnoreCase("GET"))
            {
                return _inner(environment);
            }

            var originalOutput = environment.Get<Stream>(OwinConstants.ResponseBodyKey);
            var recordedStream = new MemoryStream();
            environment.Set(OwinConstants.ResponseBodyKey, recordedStream);

            return _inner(environment).ContinueWith(t => {
                recordedStream.Position = 0;
                environment[OwinConstants.ResponseBodyKey] = originalOutput;


                if (IsGetHtmlRequest(environment) && environment.StatusCode() < 500)
                {
                    injectContent(environment, recordedStream);
                }
                else
                {
                    recordedStream.CopyTo(originalOutput);
                    recordedStream.Flush();
                }
            });
        }

        private void injectContent(IDictionary<string, object> environment, MemoryStream recordedStream)
        {
            var html = recordedStream.ReadAllText();
            var builder = new StringBuilder(html);
            var position = html.IndexOf("</head>", 0, StringComparison.OrdinalIgnoreCase);

            if (position >= 0)
            {
                builder.Insert(position, _content(environment));
            }

            environment.Write(builder.ToString());
        }

        public static bool IsGetHtmlRequest(IDictionary<string, object> environment)
        {
            return 
                environment.Get<string>(OwinConstants.RequestMethodKey).EqualsIgnoreCase("GET") && 
                environment.ResponseHeaders().ContentType().EqualsIgnoreCase(MimeType.Html.Value);
        }
    }

    public class InjectionOptions
    {
        public Func<IDictionary<string, object>, string> Content = x => "";
    }
}
