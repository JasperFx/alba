using System.Threading.Tasks;
using Baseline;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace Alba.Testing.Samples
{
    public class Quickstart
    {
        // SAMPLE: should_say_hello_world
        [Fact]
        public Task should_say_hello_world()
        {
            using (var system = SystemUnderTest.ForStartup<Startup>())
            {
                // This runs an HTTP request and makes an assertion
                // about the expected content of the response
                return system.Scenario(_ =>
                {
                    _.Get.Url("/");
                    _.ContentShouldBe("Hello, World!");
                });
            }
        }
        // ENDSAMPLE

        // SAMPLE: should_say_hello_world_with_raw_objects
        [Fact]
        public async Task should_say_hello_world_with_raw_objects()
        {
            using (var system = SystemUnderTest.ForStartup<Startup>())
            {
                var response = await system.Scenario(_ =>
                {
                    _.Context.Request.Method = "GET";
                    _.Context.Request.Path = "/";

                    _.StatusCodeShouldBeOk();
                });

                response.Context.Response.Body.ReadAllText()
                    .ShouldBe("Hello, World!");
            }
        }
        // ENDSAMPLE
    }

    // SAMPLE: HelloWorldApp
    public class Startup
    {
        public void Configure(IApplicationBuilder builder)
        {
            builder.Run(context =>
            {
                context.Response.Headers["content-type"] = "text/plain";
                return context.Response.WriteAsync("Hello, World!");
            });
        }
    }
    // ENDSAMPLE
}