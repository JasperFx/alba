using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Alba.Testing.Samples
{
    public static class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }

    public class Startup
    {
        public void Configure(IApplicationBuilder builder)
        {
            builder.Run(context =>
            {
                context.Response.Headers["content-type"] = "text/plain";
                return context.Response.WriteAsync("Hello, World!");
            });
        }
    }

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
