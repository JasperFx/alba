using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Xunit;

namespace Alba.Testing.Samples
{
    #region sample_xUnit_Fixture
    public class WebAppFixture : IDisposable
    {
        public readonly IAlbaHost AlbaHost = WebApp.Program
            .CreateHostBuilder(Array.Empty<string>())
            .StartAlba();

        public void Dispose()
        {
            AlbaHost?.Dispose();
        }
    }
    #endregion

    #region sample_using_xUnit_Fixture
    public class ContractTestWithAlba : IClassFixture<WebAppFixture>
    {
        public ContractTestWithAlba(WebAppFixture app)
        {
            _host = app.AlbaHost;
        }

        private readonly IAlbaHost _host;
    #endregion
        [Fact]
        public Task happy_path()
        {
            return _host.Scenario(_ =>
            {
                _.Get.Url("/fake/okay");
                _.StatusCodeShouldBeOk();
            });
        }


        [Fact]
        public Task sad_path()
        {
            return _host.Scenario(_ =>
            {
                _.Get.Url("/fake/bad");
                _.StatusCodeShouldBe(500);
            });
        }

        [Fact]
        public async Task with_validation_errors()
        {
            var result = await _host.Scenario(_ =>
            {
                _.Get.Url("/fake/invalid");
                _.ContentTypeShouldBe("application/problem+json; charset=utf-8");
                _.StatusCodeShouldBe(400);
            });

            var problems = result.ReadAsJson<ProblemDetails>();
            problems.Title.ShouldBe("This stinks!");
        }
    }

    #region sample_ScenarioCollection

    [CollectionDefinition("scenarios")]
    public class ScenarioCollection : ICollectionFixture<WebAppFixture>
    {
        
    }

    #endregion

    #region sample_ScenarioContext

    [Collection("scenarios")]
    public abstract class ScenarioContext
    {
        protected ScenarioContext(WebAppFixture fixture)
        {
            Host = fixture.AlbaHost;
        }

        public IAlbaHost Host { get; }
    }

    #endregion

    #region sample_integration_fixture

    public class sample_integration_fixture : ScenarioContext
    {
        public sample_integration_fixture(WebAppFixture fixture) : base(fixture)
        {
        }
        
        [Fact]
        public Task happy_path()
        {
            return Host.Scenario(_ =>
            {
                _.Get.Url("/fake/okay");
                _.StatusCodeShouldBeOk();
            });
        }
    }

    #endregion
}
