using JasperFx.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shouldly;

namespace Alba.Testing.Samples
{
    
    public class Quickstart
    {
        #region sample_should_say_hello_world
        [Fact]
        public async Task should_say_hello_world()
        {
            // Alba will automatically manage the lifetime of the underlying host
            await using var host = await AlbaHost.For<global::Program>();
            
            // This runs an HTTP request and makes an assertion
            // about the expected content of the response
            await host.Scenario(_ =>
            {
                _.Get.Url("/");
                _.ContentShouldBe("Hello World!");
                _.StatusCodeShouldBeOk();
            });
        }
        #endregion

        #region sample_should_return_entity_assert_response
        [Fact]
        public async Task should_return_entity_assert_response()
        {
            await using var host = await AlbaHost.For<global::Program>();

            var guid = Guid.NewGuid();
            var res = await host.Scenario(_ =>
            {
                _.Post.Json(new MyEntity(guid, "SomeValue")).ToUrl("/json");
                _.StatusCodeShouldBeOk();
            });

            var json = await res.ReadAsJsonAsync<MyEntity>();
            Assert.Equal(guid, json.Id);
        }
#endregion



        #region sample_should_say_hello_world_with_raw_objects
        [Fact]
        public async Task should_say_hello_world_with_raw_objects()
        {
            await using var host = await AlbaHost.For<global::Program>();
            var response = await host.Scenario(_ =>
            {
                _.Get.Url("/");
                _.StatusCodeShouldBeOk();
            });

            // you can go straight at the HttpContext & do assertions directly on the responseStream
            Stream responseStream = response.Context.Response.Body;

        }
        #endregion


        [Fact]
        public async Task working_with_the_raw_response()
        {
            await using var host = await AlbaHost.For<global::Program>();

            var res = await host.Scenario(_ =>
            {
                _.ConfigureHttpContext(c =>
                {
                    c.Request.Method = "GET";
                    c.Request.Path = "/";
                });


                _.StatusCodeShouldBeOk();
            });

            var text = await res.Context.Response.Body.ReadAllTextAsync();
            text.ShouldBe("Hello World!");
            
        }

        internal async Task configuration_overrides()
        {
            #region sample_configuration_overrides
            var stubbedWebService = new StubbedWebService();

            await using var host = await AlbaHost.For<global::Program>(x =>
            {
                // override the environment if you need to
                x.UseEnvironment("Testing");
                // override service registrations or internal options if you need to
                x.ConfigureServices(s =>
                {
                    s.AddSingleton<IExternalWebService>(stubbedWebService);
                    s.PostConfigure<MvcNewtonsoftJsonOptions>(o =>
                        o.SerializerSettings.TypeNameHandling = TypeNameHandling.All);
                });
            });

            host.BeforeEach(httpContext =>
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
    
}
