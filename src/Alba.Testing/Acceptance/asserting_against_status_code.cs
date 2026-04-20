using System.Net;
using Shouldly;

namespace Alba.Testing.Acceptance
{
    public class asserting_against_status_code : ScenarioContext
    {

        [Fact]
        public Task using_scenario_with_StatusCodeShouldBe_happy_path()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.StatusCode = 200;
                c.Response.ContentType("text/plain");
                c.Response.Write("Some text");

                return Task.CompletedTask;
            };

            return host.Scenario(x =>
            {
                x.Get.Url("/one");
                x.StatusCodeShouldBe(HttpStatusCode.OK);
            });
        }

        [Fact]
        public async Task using_scenario_with_StatusCodeShouldBe_sad_path()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.StatusCode = 200;
                c.Response.ContentType("text/plain");
                c.Response.Write("Some text");

                return Task.CompletedTask;
            };

            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Get.Url("/one");
                    x.StatusCodeShouldBe(HttpStatusCode.InternalServerError);
                });
            });

            ex.Message.ShouldContain("Expected status code 500, but was 200");
        }

        [Fact]
        public async Task happily_blows_up_on_an_unexpected_500()
        {
            router.Handlers["/wrong/status/code"] = c =>
            {
                c.Response.StatusCode = 500;
                c.Response.Write("the error text");

                return Task.CompletedTask;
            };

            var ex = await fails(_ =>
            {
                _.Get.Url("/wrong/status/code");
            });

            ex.Message.ShouldContain("Expected status code 200, but was 500");
            ex.Message.ShouldContain("the error text");
        }

        [Fact]
        public Task using_scenario_with_StatusCodeShouldBeSuccess_happy_path()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.StatusCode = 204;
                c.Response.ContentType("text/plain");
                c.Response.Write("Some text");

                return Task.CompletedTask;
            };

         
                return host.Scenario(x =>
                {
                    x.Get.Url("/one");
                    x.StatusCodeShouldBeSuccess();
                });
            

        }

        [Fact]
        public async Task using_scenario_with_StatusCodeShouldBeSuccess_sad_path()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.StatusCode = 500;
                c.Response.ContentType("text/plain");
                c.Response.Write("Some text");

                return Task.CompletedTask;
            };

            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Get.Url("/one");
                    x.StatusCodeShouldBeSuccess();
                });
            });

            ex.Message.ShouldContain("Expected a status code between 200 and 299, but was 500");
        }

    }
}