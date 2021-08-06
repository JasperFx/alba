using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Alba.Testing
{
    public class before_and_after_actions
    {
        private readonly ITestOutputHelper _output;

        public before_and_after_actions(ITestOutputHelper output)
        {
            _output = output;
        }

        protected IHostBuilder EmptyHostBuilder()
        {
            return new HostBuilder().ConfigureWebHost(x =>
            {
                x.Configure(app => app.Run(c => c.Response.WriteAsync("Hey.")));
            });

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
                // Quick check
                system.Services.ShouldNotBeNull();

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
            using var system = await EmptyHostBuilder().StartAlbaHostAsync();
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

        protected IHostBuilder AuthenticatedHostBuilder()
        {
            return new HostBuilder().ConfigureWebHost(x =>
            {
                x.Configure(app => app.Run(c =>
                {
                    _output.WriteLine("In app.Run");
                    c.Response.StatusCode = c.User.Identity.IsAuthenticated
                        ? 200
                        : 401;
                    return Task.CompletedTask;
                }));
            });

        }

        [Fact]
        public async Task authentication_with_before()
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(null, "Basic"));

            //This works
            using (var system = new SystemUnderTest(AuthenticatedHostBuilder())
                .BeforeEachAsync(c => Task.Run(() =>
                {
                    _output.WriteLine("In BeforeEach");
                    c.User = authenticatedUser;
                })))
            {
                var result = await system.Scenario(x => x.Get.Url("/"));
                result.Context.Response.StatusCode.ShouldBe(200);
            }
        }

        [Fact]
        public async Task authentication_with_long_running_before()
        {
            

            //This doesn't
            using (var system = new SystemUnderTest(AuthenticatedHostBuilder())
                .BeforeEachAsync(doSecurity))
            {
                var result = await system.Scenario(x => x.Get.Url("/"));
                result.Context.Response.StatusCode.ShouldBe(200);
            }
        }

        private async Task doSecurity(HttpContext context)
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(null, "Basic"));
            
            _output.WriteLine("Start BeforeEach");
            await Task.Delay(100);
            //Thread.Sleep(100);
            context.User = authenticatedUser;
            _output.WriteLine("End BeforeEach");
        }
    }
}