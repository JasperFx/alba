using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Shouldly;
using WebApp;
using Xunit;

namespace Alba.Testing.Acceptance
{
    public class customize_before_each_and_after_each
    {
        [Fact]
        public async Task before_each_and_after_each_is_called()
        {
            using (var host = new SimpleHost())
            {
                host.BeforeContext = host.AfterContext = null;

                await host.Scenario(_ =>
                {
                    _.Get.Url("/api/values");
                });

                host.AfterContext.ShouldNotBeNull();
                host.BeforeContext.ShouldNotBeNull();
            }
        }
    }

    public class SimpleHost : SystemUnderTest
    {
        public SimpleHost()
        {
            UseStartup<Startup>();

            BeforeEach(c =>
            {
                BeforeContext = c;
            });

            AfterEach(c => AfterContext = c);
        }

        public HttpContext BeforeContext { get; set; }

        public HttpContext AfterContext { get; set; }
    }
}