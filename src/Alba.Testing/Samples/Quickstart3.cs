using System.Threading.Tasks;
using Xunit;

namespace Alba.Testing.Samples
{
    public class Quickstart3
    {
        // SAMPLE: Quickstart3
        [Fact]
        public async Task build_system_under_test_from_Program()
        {
            var hostBuilder = WebApiAspNetCore3.Program.CreateHostBuilder(new string[0]);
            // You can also call hostBuilder.ConfigureServices() here to further customize
            // your application, and that's frequently valuable in testing scenarios
            
            using (var system = new SystemUnderTest(hostBuilder))
            {
                await system.Scenario(s =>
                {
                    s.Get.Url("/");
                    s.ContentShouldBe("Hello world.");
                });
            }
        }
        // ENDSAMPLE
    }
}