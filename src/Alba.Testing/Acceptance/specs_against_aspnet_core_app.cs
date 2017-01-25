using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using WebApp;
using WebApp.Controllers;
using Xunit;

namespace Alba.Testing.Acceptance
{
    public class specs_against_aspnet_core_app
    {
        private Task<IScenarioResult> run(Action<Scenario> configuration)
        {
            using (var system = SystemUnderTest.ForStartup<Startup>())
            {
                var formatters = system.Services.GetServices<IInputFormatter>().ToArray();

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

        [Fact]
        public async Task can_read_json_response()
        {
            var result = await run(_ =>
            {
                _.Get.Url("/api/json");
            });


            var person = result.ResponseBody.ReadAsJson<Person>();


            person.FirstName.ShouldBe("Jeremy");
            person.LastName.ShouldBe("Miller");
        }

        [Fact]
        public async Task can_post_json_response()
        {

            var result = await run(_ =>
            {
                _.Post
                    .Json(new Person {FirstName = "Tom", LastName = "Brady"})
                    .ToUrl("/api/json");

            });



            var person = result.ResponseBody.ReadAsJson<Person>();

            person.FirstName.ShouldBe("Tom");
            person.LastName.ShouldBe("Brady");
        }
    }
}