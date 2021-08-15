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

            await using var system = new AlbaHost(hostBuilder);
            
            // Just as a sample, I'll run a scenario against
            // a "hello, world" application's root url
            await system.Scenario(s =>
            {
                s.Get.Url("/");
                s.ContentShouldBe("Hello world.");
            });
        }
        #endregion

    }
    

}
