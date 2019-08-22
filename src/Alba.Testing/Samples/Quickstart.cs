﻿using System;
using System.Threading.Tasks;
using Baseline;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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
            
            // or pass an IWebHostBuilder into the constructor function
            // of SystemUnderTest

            var builder = WebHost
                .CreateDefaultBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    // override any service registrations you need,
                    // like maybe using stubs for problematic dependencies
                });
            
            var system2 = new SystemUnderTest(builder);
            
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


                // TestServer 3.0 doesn't really work with overridden response streams,
                // so we can't rewind the stream here.
#if !NETCOREAPP3_0
                var body = response.Context.Response.Body;
                body.Position = 0; // need to rewind it because we read it above
                body.ReadAllText().ShouldBe("Hello, World!");
#endif
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

        // SAMPLE: SimplisticSystemUnderTest
        [Fact]
        public async Task the_home_page_does_not_blow_up()
        {
            using (var system = SystemUnderTest.ForStartup<Startup>())
            {
                var response = await system.Scenario(_ =>
                {
                    _.Get.Url("/");
                    _.StatusCodeShouldBeOk();
                });
            }
        }
        // ENDSAMPLE


        public void setting_up_system_under_test_examples()
        {
            // SAMPLE: override_the_content_path
            
            // Alba has a helper for overriding the root path
            var system = SystemUnderTest
                .ForStartup<Startup>(rootPath:"c:\\path_to_your_actual_application");

            // or do it with idiomatic ASP.Net Core

            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>()
                .UseContentRoot("c:\\path_to_your_actual_application");

            var system2 = new SystemUnderTest(builder);

            // ENDSAMPLE

        }

        public void configuration_overrides()
        {
            // SAMPLE: configuration-overrides
            var stubbedWebService = new StubbedWebService();
            
            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>()
                
                // override the environment if you need to
                .UseEnvironment("Testing")
                
                // override service registrations if you need to
                .ConfigureServices(s =>
                {
                    s.AddSingleton<IExternalWebService>(stubbedWebService);
                });
            
            // Create the SystemUnderTest, and alternatively override what Alba
            // thinks is the main application assembly
            // THIS IS IMPORTANT FOR MVC CONTROLLER DISCOVERY
            var system = new SystemUnderTest(builder, applicationAssembly:typeof(Startup).Assembly);

            system.BeforeEach(httpContext =>
            {
                // do some data setup or clean up before every single test
            });

            system.AfterEach(httpContext =>
            {
                // do any kind of cleanup after each scenario completes
            });
            // ENDSAMPLE
        }

        public interface IExternalWebService
        {


        }

        public class StubbedWebService : IExternalWebService
        {
            
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