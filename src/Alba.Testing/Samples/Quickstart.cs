using System;
using System.Threading.Tasks;
using Baseline;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace Alba.Testing.Samples
{

    public class Quickstart
    {
        // SAMPLE: should_say_hello_world
        [Fact]
        public async Task should_say_hello_world()
        {
            using (var system = SystemUnderTest.ForStartup<Startup>())
            {
                // This runs an HTTP request and makes an assertion
                // about the expected content of the response
                await system.Scenario(_ =>
                {
                    _.Get.Url("/");
                    _.ContentShouldBe("Hello, World!");
                    _.StatusCodeShouldBeOk();
                });
            }
        }
        // ENDSAMPLE

        [Fact]
        public async Task should_say_hello_world_raw()
        {
            // SAMPLE: programmatic-bootstrapping
            var system = SystemUnderTest.For(_ =>
            {
                _.Configure(app =>
                {
                    app.Run(c =>
                    {
                        return c.Response.WriteAsync("Hello, World!");
                    });
                });
            });
            // ENDSAMPLE

            try
            {
                await system.Scenario(_ =>
                {
                    _.Get.Url("/");
                    _.ContentShouldContain("Hello, World!");
                });
            }
            finally
            {
                system.Dispose();
            }


        }

        // SAMPLE: should_say_hello_world_with_raw_objects
        [Fact]
        public async Task should_say_hello_world_with_raw_objects()
        {
            using (var system = SystemUnderTest.ForStartup<Startup>())
            {
                var response = await system.Scenario(_ =>
                {
                    _.Get.Url("/");
                    _.StatusCodeShouldBeOk();
                });

                response.ResponseBody.ReadAsText()
                    .ShouldBe("Hello, World!");

                // or you can go straight at the HttpContext
                // The ReadAllText() extension method is from Baseline


                var body = response.Context.Response.Body;
                body.Position = 0; // need to rewind it because we read it above
                body.ReadAllText().ShouldBe("Hello, World!");
            }
        }
        // ENDSAMPLE


        [Fact]
        public async Task working_with_the_raw_response()
        {
            using (var system = SystemUnderTest.ForStartup<Startup>())
            {
                var response = await system.Scenario(_ =>
                {
                    _.Configure = c =>
                    {
                        c.Request.Method = "GET";
                        c.Request.Path = "/";
                    };
                    


                    _.StatusCodeShouldBeOk();
                });

                response.Context.Response.Body.ReadAllText()
                    .ShouldBe("Hello, World!");
            }
        }
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