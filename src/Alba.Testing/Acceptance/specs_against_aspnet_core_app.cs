using System;
using System.Threading.Tasks;
using WebApp;
using Xunit;

namespace Alba.Testing.Acceptance
{
    public class specs_against_aspnet_core_app
    {
        private Task<Scenario> run(Action<Scenario> configuration)
        {
            using (var system = SystemUnderTest.ForStartup<Startup>())
            {
                return system.Scenario(configuration);
            }
        }

        [Fact]
        public Task bootstrap_and_execute_a_request_through_an_aspnet_core_app()
        {
            return run(_ =>
            {
                _.Get.Url("/api/values");
                _.ContentTypeShouldBe("text/plain");
                _.StatusCodeShouldBeOk();
            });
        }

        [Fact]
        public async Task obeys_the_http_method()
        {
            await run(_ =>
            {
                _.Post.Url("/api/values");
                _.Body.TextIs("Blue");

                _.ContentShouldBe("I ran a POST with value Blue");
            });
        }

        [Fact]
        public Task can_do_the_parallel_directory_trick_from_fubu_for_content_data()
        {
            return run(_ =>
            {
                _.Get.Url("/hello.txt");
                _.ContentShouldContain("Hello from ASP.Net Core app!");
            });
        }
    }
}