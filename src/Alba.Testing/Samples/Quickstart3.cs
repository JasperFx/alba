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
        #region sample_Quickstart3
        [Fact]
        public async Task build_host_from_Program()    
        {
            // Bootstrap your application just as your real application does
            var hostBuilder = Program.CreateHostBuilder(Array.Empty<string>());

            await using var host = new AlbaHost(hostBuilder);

            // Just as a sample, I'll run a scenario against
            // a "hello, world" application's root url
            await host.Scenario(s =>
            {
                s.Get.Url("/");
                s.ContentShouldBe("Hello, World!");
            });
        }
        #endregion
        
        
        #region sample_shorthand_bootstrapping
        [Fact]
        public async Task fluent_interface_bootstrapping()    
        {
            await using var host = await Program
                .CreateHostBuilder(Array.Empty<string>())
                .StartAlbaAsync();

            // Just as a sample, I'll run a scenario against
            // a "hello, world" application's root url
            await host.Scenario(s =>
            {
                s.Get.Url("/");
                s.ContentShouldBe("Hello, World!");
            });
        }
        #endregion

    }
    

}
