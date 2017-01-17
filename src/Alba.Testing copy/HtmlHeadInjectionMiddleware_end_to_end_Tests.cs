using System;
using System.Net;
using System.Threading.Tasks;
using HtmlTags;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class HtmlHeadInjectionMiddleware_end_to_end_Tests
    {
        [Fact]
        public void injects_html_into_the_head()
        {
            var midfunc = HtmlHeadInjectionMiddleware.ToMidFunc(env => "<!--Hello!-->");
            var appfunc = midfunc(env =>
            {
                env.StatusCode(200);

                if (env.IsGet())
                {
                    var document = new HtmlDocument();
                    env.Write(document.ToString(), MimeType.Html.Value);
                }
                else
                {
                    env.Write("Different", "text/plain");
                }


                

                return Task.CompletedTask;
            });


            throw new NotImplementedException();
//            using (var server = new NowinHarness(appfunc))
//            {
//                var client = new WebClient();
//
//                var path = $"http://localhost:{server.Port}";
//                client.DownloadString(path).ShouldContain("<!--Hello!-->");
//            }

        }
    }
}