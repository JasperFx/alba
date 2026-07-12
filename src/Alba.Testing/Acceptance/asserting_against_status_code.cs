using Microsoft.AspNetCore.Http;
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
                return c.Response.WriteAsync("Some text");
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
                return c.Response.WriteAsync("Some text");
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
                return c.Response.WriteAsync("the error text");
            };

            var ex = await fails(_ =>
            {
                _.Get.Url("/wrong/status/code");
            });

            ex.Message.ShouldContain("Expected a status code between 200 and 299, but was 500");
            ex.Message.ShouldContain("the error text");
        }

        [Theory]
        [InlineData(200)]
        [InlineData(201)]
        [InlineData(204)]
        [InlineData(299)]
        public Task success_status_codes_pass_by_default(int statusCode)
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.StatusCode = statusCode;
                c.Response.ContentType("text/plain");
                return c.Response.WriteAsync("Some text");
            };

            return host.Scenario(x =>
            {
                x.Get.Url("/one");
            });
        }

        [Theory]
        [InlineData(200)]
        [InlineData(201)]
        [InlineData(204)]
        [InlineData(299)]
        public Task using_scenario_with_StatusCodeShouldBeSuccess_happy_path(int statusCode)
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.StatusCode = statusCode;
                c.Response.ContentType("text/plain");
                return c.Response.WriteAsync("Some text");
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
                return c.Response.WriteAsync("Some text");
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

        [Fact]
        public async Task explicit_status_code_expectation_wins_over_ignore()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.StatusCode = 200;
                c.Response.ContentType("text/plain");
                return c.Response.WriteAsync("Some text");
            };

            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Get.Url("/one");
                    x.IgnoreStatusCode();
                    x.StatusCodeShouldBe(500);
                });
            });

            ex.Message.ShouldContain("Expected status code 500, but was 200");
        }

    }
}