using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Alba.Testing.Acceptance
{
    public class asserting_against_the_response_body_text : ScenarioContext
    {
        [Fact]
        public Task using_scenario_with_ContentShouldContain_declaration_happy_path()
        {
            return host.Scenario(x =>
            {
                x.Get.Input<MarkerInput>();
                x.ContentShouldContain("just the marker");
            });
        }


        [Fact]
        public async Task using_scenario_with_ContentShouldContain_declaration_sad_path()
        {
            try
            {
                await host.Scenario(x =>
                {
                    x.Get.Input<MarkerInput>();
                    x.ContentShouldContain("wrong text");
                });

                throw new NotSupportedException("No exception thrown by the scenario");
            }
            catch (Exception e)
            {
                e.ShouldBeOfType<ScenarioAssertionException>();
                e.Message.ShouldContain("The response body does not contain expected text \"wrong text\"");
            }
        }

        [Fact]
        public Task using_scenario_with_ContentShouldNotContain_declaration_happy_path()
        {
            return host.Scenario(x =>
            {
                x.Get.Input<MarkerInput>();
                x.ContentShouldNotContain("some random stuff");
            });
        }

        [Fact]
        public async Task using_scenario_with_ContentShouldNotContain_declaration_sad_path()
        {
            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Get.Input<MarkerInput>();
                    x.ContentShouldNotContain("just the marker");
                });
            });

            ex.Message.ShouldContain("The response body should not contain text \"just the marker\"");
        }






        [Fact]
        public async Task exact_content_sad_path()
        {
            host.Handlers["/memory/hello"] = c =>
            {
                c.Response.ContentType("text/plain");
                c.Response.Write("some text");

                return Task.CompletedTask;
            };

            var e = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Get.Url("/memory/hello");

                    x.ContentShouldBe("the wrong content");
                });
            });

            e.Message.ShouldContain("Expected the content to be 'the wrong content'");
        }

        [Fact]
        public Task exact_content_happy_path()
        {
            host.Handlers["/memory/hello"] = c =>
            {
                c.Response.ContentType("text/plain");
                c.Response.Write("hello from the in memory host");

                return Task.CompletedTask;
            };

            return host.Scenario(x =>
            {
                x.Get.Url("/memory/hello");

                x.ContentShouldBe("hello from the in memory host");
            });
        }



    }
}