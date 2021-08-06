using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Configuration;
using WebApiAspNetCore3;    

using Xunit;

namespace Alba.Testing.Samples
{

    public class Quickstart3
    {
        // SAMPLE: Quickstart3
        [Fact]
        public async Task build_system_under_test_from_Program()    
        {
            var hostBuilder = Program.CreateHostBuilder(Array.Empty<string>());
            // You can also call hostBuilder.ConfigureServices() here to further customize
            // your application, and that's frequently valuable in testing scenarios
            
            using (var system = new AlbaTestHost(hostBuilder))
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
    

}