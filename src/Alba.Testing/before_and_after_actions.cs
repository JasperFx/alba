using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class before_and_after_actions
    {
        protected IWebHostBuilder EmptyHostBuilder()
        {
            return new WebHostBuilder()
                .Configure(app => app.Run(c => c.Response.WriteAsync("Hey.")));
        }

        // SAMPLE: before-and-after
        public void sample_usage(SystemUnderTest system)
        {
            // Synchronously
            system.BeforeEach(context =>
            {
                // Modify the HttpContext immediately before each
                // Scenario()/HTTP request is executed
                context.Request.Headers.Add("trace", "something");
            });
            
            system.AfterEach(context =>
            {
                // perform an action immediately after the scenario/HTTP request
                // is executed
            });
            
            
            // Asynchronously
            system.BeforeEachAsync(context =>
            {
                // do something asynchronous here
                return Task.CompletedTask;
            });
            
            system.AfterEachAsync(context =>
            {
                // do something asynchronous here
                return Task.CompletedTask;
            });


        }
        // ENDSAMPLE
        
        [Fact]
        public async Task synchronous_before_and_after()
        {
            using (var system = new SystemUnderTest(EmptyHostBuilder()))
            {
                int count = 0;

                system.BeforeEach(c =>
                {
                    c.ShouldNotBeNull();
                    count = 1;
                });

                system.AfterEach(c =>
                {
                    c.ShouldNotBeNull();
                    count.ShouldBe(1);

                    count = 2;
                });

                var result = await system.Scenario(x => x.Get.Url("/"));
                
                count.ShouldBe(2);
            }
        }
        
        [Fact]
        public async Task asynchronous_before_and_after()
        {
            using (var system = new SystemUnderTest(EmptyHostBuilder()))
            {
                int count = 0;

                system.BeforeEachAsync(c =>
                {
                    c.ShouldNotBeNull();
                    count = 1;

                    return Task.CompletedTask;
                });

                system.AfterEachAsync(c =>
                {
                    c.ShouldNotBeNull();
                    count.ShouldBe(1);

                    count = 2;

                    return Task.CompletedTask;
                });

                var result = await system.Scenario(x => x.Get.Url("/"));
                
                count.ShouldBe(2);
            }
        }
    }

}