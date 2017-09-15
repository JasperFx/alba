﻿using System.Threading.Tasks;
using Shouldly;
using WebApp;
using Xunit;

namespace Alba.Testing.Acceptance
{
    public class assertions_against_redirects
    {
        [Fact]
        public async Task redirect()
        {
            using (var system = SystemUnderTest.ForStartup<Startup>())
            {
                await system.Scenario(_ =>
                {
                    _.Get.Url("/auth/redirect");

                    _.RedirectShouldBe("/api/values");
                });
            }
        }

        [Fact]
        public async Task not_redirected()
        {
            var result = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                using (var system = SystemUnderTest.ForStartup<Startup>())
                {
                    return system.Scenario(_ =>
                    {
                        _.Get.Url("/api/values");

                        _.RedirectShouldBe("/else");
                    });
                }
            });

            result.Message.ShouldContain("Expected to be redirected to '/else' but was ''.");
        }

        [Fact]
        public async Task redirect_wrong_value()
        {
            var result = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                using (var system = SystemUnderTest.ForStartup<Startup>())
                {
                    return system.Scenario(_ =>
                    {
                        _.Get.Url("/auth/redirect");

                        _.RedirectShouldBe("/else");
                    });
                }
            });

            result.Message.ShouldContain("Expected to be redirected to '/else' but was '/api/Values'.");
        }

        [Fact]
        public async Task redirect_permanent()
        {
            using (var system = SystemUnderTest.ForStartup<Startup>())
            {
                await system.Scenario(_ =>
                {
                    _.Get.Url("/auth/redirectpermanent");

                    _.RedirectPermanentShouldBe("/api/values");
                });
            }
        }

        [Fact]
        public async Task redirect_permanent_wrong_value()
        {
            var result = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                using (var system = SystemUnderTest.ForStartup<Startup>())
                {
                    return system.Scenario(_ =>
                    {
                        _.Get.Url("/auth/redirectpermanent");

                        _.RedirectPermanentShouldBe("/else");
                    });
                }
            });

            result.Message.ShouldContain("Expected to be redirected to '/else' but was '/api/Values'.");
        }

        [Fact]
        public async Task redirect_permanent_non_permanent_result()
        {
            var result = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                using (var system = SystemUnderTest.ForStartup<Startup>())
                {
                    return system.Scenario(_ =>
                    {
                        _.Get.Url("/auth/redirect");

                        _.RedirectPermanentShouldBe("/api/values");
                    });
                }
            });

            result.Message.ShouldContain("Expected permanent redirect to be 'True' but it was not.");
        }
    }
}
