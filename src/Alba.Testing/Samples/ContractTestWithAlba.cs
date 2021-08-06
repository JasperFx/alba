using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Xunit;

namespace Alba.Testing.Samples
{
    // SAMPLE: xUnit-Fixture
    public class WebAppFixture : IDisposable
    {
        public readonly AlbaHost AlbaHost = AlbaHost.ForStartup<WebApp.Startup>();

        public void Dispose()
        {
            AlbaHost?.Dispose();
        }
    }
    // ENDSAMPLE

    // SAMPLE: using-xUnit-Fixture
    public class ContractTestWithAlba : IClassFixture<WebAppFixture>
    {
        public ContractTestWithAlba(WebAppFixture app)
        {
            _system = app.AlbaHost;
        }

        private readonly AlbaHost _system;
    // ENDSAMPLE
        [Fact]
        public Task happy_path()
        {
            return _system.Scenario(_ =>
            {
                _.Get.Url("/fake/okay");
                _.StatusCodeShouldBeOk();
            });
        }


        [Fact]
        public Task sad_path()
        {
            return _system.Scenario(_ =>
            {
                _.Get.Url("/fake/bad");
                _.StatusCodeShouldBe(500);
            });
        }

        [Fact]
        public async Task with_validation_errors()
        {
            var result = await _system.Scenario(_ =>
            {
                _.Get.Url("/fake/invalid");
                _.ContentTypeShouldBe("application/problem+json; charset=utf-8");
                _.StatusCodeShouldBe(400);
            });

            var problems = result.ResponseBody.ReadAsJson<ProblemDetails>();
            problems.Title.ShouldBe("This stinks!");
        }
    }
}