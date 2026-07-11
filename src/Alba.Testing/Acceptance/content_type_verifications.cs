using Microsoft.AspNetCore.Http;
using Shouldly;

namespace Alba.Testing
{
    public class content_type_verifications : ScenarioContext
    {

        [Fact]
        public Task content_type_should_be_happy_path()
        {
            router.Handlers["/memory/hello"] = c =>
            {
                c.Response.ContentType("text/plain");
                return c.Response.WriteAsync("some text");
            };

            return host.Scenario(_ =>
            {
                _.Get.Url("/memory/hello");

                _.ContentTypeShouldBe(MimeType.Text);
            });
        }


        [Fact]
        public async Task content_type_sad_path()
        {
            router.RegisterRoute<InMemoryEndpoint>(x => x.get_memory_hello(), "GET", "/memory/hello");
            router.Handlers["/memory/hello"] = c =>
            {
                c.Response.ContentType("text/plain");
                return c.Response.WriteAsync("Some text");
            };

            var ex = await fails(_ =>
            {
                _.Get.Url("/memory/hello");

                _.ContentTypeShouldBe("text/json");
            });

            ex.Message.ShouldContain(
                "Expected a single header value of 'Content-Type'='text/json', but the actual value was 'text/plain'");
        }
    }
}