using System;
using System.IO;
using System.Threading.Tasks;
using Baseline;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NSubstitute.Extensions;
using Shouldly;
using WebApp;
using Xunit;

namespace Alba.Testing.Samples
{
    
    public static class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }

    public class Quickstart
    {
        #region sample_should_say_hello_world
        [Fact]
        public async Task should_say_hello_world()
        {
            await using var host = await Program
                .CreateHostBuilder(Array.Empty<string>())
                
                // This extension method is just a shorter version
                // of new AlbaHost(builder)
                .StartAlbaAsync();
            
            // This runs an HTTP request and makes an assertion
            // about the expected content of the response
            await host.Scenario(_ =>
            {
                _.Get.Url("/");
                _.ContentShouldBe("Hello, World!");
                _.StatusCodeShouldBeOk();
            });
        }
        #endregion

        [Fact]
        public async Task should_say_hello_world_raw()
        {
            #region sample_programmatic_bootstrapping
            using var system = AlbaHost.For(_ =>
            {
                _.Configure(app =>
                {
                    app.Run(c => c.Response.WriteAsync("Hello, World!"));
                });
            });
            
            // or pass an IHostBuilder into the constructor function
            // of SystemUnderTest

            var builder = Host
                .CreateDefaultBuilder()
                .ConfigureWebHostDefaults(c=> c.UseStartup<Startup>())
                .ConfigureServices(services =>
                {
                    // override any service registrations you need,
                    // like maybe using stubs for problematic dependencies
                });
            
            using var system2 = new AlbaHost(builder);

            #endregion


            await system.Scenario(_ =>
            {
                _.Get.Url("/");
                _.ContentShouldContain("Hello, World!");
            });
        }

        #region sample_should_say_hello_world_with_raw_objects
        [Fact]
        public async Task should_say_hello_world_with_raw_objects()
        {
            using (var system = AlbaHost.ForStartup<Startup>())
            {
                var response = await system.Scenario(_ =>
                {
                    _.Get.Url("/");
                    _.StatusCodeShouldBeOk();
                });

                response.ReadAsText()
                    .ShouldBe("Hello, World!");

                // or you can go straight at the HttpContext
                Stream responseStream = response.Context.Response.Body;
                // do assertions directly on the responseStream
            }
        }
        #endregion


        [Fact]
        public async Task working_with_the_raw_response()
        {
            using (var system = AlbaHost.ForStartup<Startup>())
            {
                var response = await system.Scenario(_ =>
                {
                    _.ConfigureHttpContext(c =>
                    {
                        c.Request.Method = "GET";
                        c.Request.Path = "/";
                    });
                    


                    _.StatusCodeShouldBeOk();
                });

                response.Context.Response.Body.ReadAllText()
                    .ShouldBe("Hello, World!");
            }
        }


        #region sample_SimplisticSystemUnderTest
        [Fact]
        public async Task the_home_page_does_not_blow_up()
        {
            using (var system = AlbaHost.ForStartup<Startup>())
            {
                var response = await system.Scenario(_ =>
                {
                    _.Get.Url("/");
                    _.StatusCodeShouldBeOk();
                });
            }
        }
        #endregion


        public void setting_up_system_under_test_examples()
        {
            #region sample_override_the_content_path
            
            // Alba has a helper for overriding the root path
            var system = AlbaHost
                .ForStartup<Startup>(rootPath:"c:\\path_to_your_actual_application");

            // or do it with idiomatic ASP.Net Core

            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(c=> c.UseStartup<Startup>())
                .UseContentRoot("c:\\path_to_your_actual_application");

            var system2 = new AlbaHost(builder);

            #endregion

        }

        public void configuration_overrides()
        {
            #region sample_configuration_overrides
            var stubbedWebService = new StubbedWebService();

            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(c => c.UseStartup<Startup>())

                // override the environment if you need to
                .UseEnvironment("Testing")

                // override service registrations or internal options if you need to
                .ConfigureServices(s =>
                {
                    s.AddSingleton<IExternalWebService>(stubbedWebService);
                    s.PostConfigure<MvcNewtonsoftJsonOptions>(o =>
                        o.SerializerSettings.TypeNameHandling = TypeNameHandling.All);
                });

            // Create the SystemUnderTest
            var system = new AlbaHost(builder)
                .BeforeEach(httpContext =>
                {
                    // do some data setup or clean up before every single test
                })
                .AfterEach(httpContext =>
                {
                    // do any kind of cleanup after each scenario completes
                });
            #endregion
        }

        public interface IExternalWebService
        {


        }

        public class StubbedWebService : IExternalWebService
        {
            
        }
    }

    #region sample_HelloWorldApp
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
    #endregion
    
    



}
