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
            using (var host = AlbaTestHost
                .ForStartup<Startup>()
            .BeforeEach(c =>
            {
                BeforeContext = c;
            })
                .AfterEach(c => AfterContext = c))
            {
                BeforeContext = AfterContext = null;

                await host.Scenario(_ =>
                {
                    _.Get.Url("/api/values");
                });

                AfterContext.ShouldNotBeNull();
                BeforeContext.ShouldNotBeNull();
            }
        }
        
        public HttpContext BeforeContext { get; set; }

        public HttpContext AfterContext { get; set; }
    }

}