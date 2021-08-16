using System;
using System.Buffers;
using System.Threading.Tasks;
using Alba;
using NUnit.Framework;
using WebApp;

namespace NUnitSamples
{

    #region sample_NUnit_Application

    [SetUpFixture]
    public class Application
    {
        // Make this lazy so you don't build it out
        // when you don't need it.
        private static readonly Lazy<IAlbaHost> _host;

        static Application()
        {
            _host = new Lazy<IAlbaHost>(() => Program
                .CreateHostBuilder(Array.Empty<string>())
                .StartAlba());
        }

        public static IAlbaHost AlbaHost => _host.Value;

        // Make sure that NUnit will shut down the AlbaHost when
        // all the projects are finished
        [OneTimeTearDown]
        public void Teardown()
        {
            if (_host.IsValueCreated)
            {
                _host.Value.Dispose();
            }
        }
    }

    #endregion

    #region sample_NUnit_scenario_test
    public class sample_integration_fixture
    {
        [Test]
        public Task happy_path()
        {
            return Application.AlbaHost.Scenario(_ =>
            {
                _.Get.Url("/fake/okay");
                _.StatusCodeShouldBeOk();
            });
        }
    }
    #endregion
}