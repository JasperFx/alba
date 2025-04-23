using Shouldly;

namespace Alba.Testing.Acceptance
{
    public class asserting_against_the_response_body_text : ScenarioContext
    {
        #region sample_using_ContentShouldBe
        [Fact]
        public Task using_scenario_with_ContentShouldContain_declaration_happy_path()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.Write("**just the marker**");
                return Task.CompletedTask;
            };

            return host.Scenario(x =>
            {
                x.Get.Url("/one");
                x.ContentShouldContain("just the marker");
            });
        }
        #endregion


        [Fact]
        public async Task using_scenario_with_ContentShouldContain_declaration_sad_path()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.Write("**just the marker**");
                return Task.CompletedTask;
            };

            var ex = await fails(x =>
            {
                x.Get.Url("/one");
                x.ContentShouldContain("wrong text");
            });

            ex.Message.ShouldContain("Expected text 'wrong text' was not found in the response body");
        }

        [Fact]
        public Task using_scenario_with_ContentShouldNotContain_declaration_happy_path()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.Write("**just the marker**");
                return Task.CompletedTask;
            };

            return host.Scenario(x =>
            {
                x.Get.Url("/one");
                x.ContentShouldNotContain("some random stuff");
            });
        }

        [Fact]
        public async Task using_scenario_with_ContentShouldNotContain_declaration_sad_path()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.Write("**just the marker**");
                return Task.CompletedTask;
            };

            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Get.Url("/one");
                    x.ContentShouldNotContain("just the marker");
                });
            });

            ex.Message.ShouldContain("Text 'just the marker' should not be found in the response body");
        }






        [Fact]
        public async Task exact_content_sad_path()
        {
            router.Handlers["/memory/hello"] = c =>
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
            router.Handlers["/memory/hello"] = c =>
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
