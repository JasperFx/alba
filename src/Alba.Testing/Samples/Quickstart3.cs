using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
#if NETCOREAPP3_0
using Microsoft.Extensions.Configuration;
using WebApiAspNetCore3;    
#endif
using Xunit;

namespace Alba.Testing.Samples
{
#if NETCOREAPP3_0
    public class Quickstart3
    {
        // SAMPLE: Quickstart3
        [Fact]
        public async Task build_system_under_test_from_Program()    
        {
            var hostBuilder = Program.CreateHostBuilder(new string[0]);
            // You can also call hostBuilder.ConfigureServices() here to further customize
            // your application, and that's frequently valuable in testing scenarios
            
            using (var system = new SystemUnderTest(hostBuilder))
            {
                // You can use the IoC container for the SystemUnderTest
                var configuration 
                    = system.Services.GetRequiredService<IConfiguration>();
                
                await system.Scenario(s =>
                {
                    s.Get.Url("/");
                    s.ContentShouldBe("Hello world.");
                });
            }
        }
        // ENDSAMPLE 

    }
    
#endif
}