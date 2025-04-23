using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Shouldly;

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

        internal void sample_usage(AlbaHost system)
        {
            #region sample_before_and_after
            // Synchronously
            system.BeforeEach(context =>
            {
                // Modify the HttpContext immediately before each
                // Scenario()/HTTP request is executed
                context.Request.Headers.Append("trace", "something");
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

            #endregion
        }


        [Fact]
        public async Task synchronous_before_and_after()
        {
            using (var system = new AlbaHost(EmptyHostBuilder()))
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
            using var system = await EmptyHostBuilder().StartAlbaAsync();
            int count = 0;

            system.BeforeEachAsync(c =>
            {
                c.ShouldNotBeNull();
                count++;

                return Task.CompletedTask;
            });
            
            system.BeforeEachAsync(c =>
            {
                c.ShouldNotBeNull();
                count++;

                return Task.CompletedTask;
            });

            system.AfterEachAsync(c =>
            {
                c.ShouldNotBeNull();
                count.ShouldBe(2);

                count++;

                return Task.CompletedTask;
            });
            
            system.AfterEachAsync(c =>
            {
                c.ShouldNotBeNull();
                count.ShouldBe(3);

                count++;

                return Task.CompletedTask;
            });

            var result = await system.Scenario(x => x.Get.Url("/"));

            count.ShouldBe(4);
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
            await using var system = new AlbaHost(AuthenticatedHostBuilder())
                .BeforeEachAsync(c => Task.Run(() =>
                {
                    _output.WriteLine("In BeforeEach");
                    c.User = authenticatedUser;
                }));
            var result = await system.Scenario(x => x.Get.Url("/"));
            result.Context.Response.StatusCode.ShouldBe(200);
        }

        [Fact]
        public async Task authentication_with_long_running_before()
        {
            

            //This doesn't
            using (var system = new AlbaHost(AuthenticatedHostBuilder())
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
