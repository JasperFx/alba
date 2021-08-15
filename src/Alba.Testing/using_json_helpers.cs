using System;
using System.Threading.Tasks;
using Alba.Testing.Samples;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Shouldly;
using WebApp.Controllers;
using Xunit;

namespace Alba.Testing
{
    public static class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<WebApp.Startup>(); });
    }
    
    public class using_json_helpers
    {
        #region sample_get_json
        [Fact]
        public async Task get_happy_path()
        {
            var builder = Program.CreateHostBuilder(Array.Empty<string>());

            await using var system = new AlbaHost(builder);
            
            // Issue a request, and check the results
            var result = await system.GetAsJson<OperationResult>("/math/add/3/4");
                
            result.Answer.ShouldBe(7);
        }
        #endregion

        #region sample_post_json_get_json
        [Fact]
        public async Task post_and_expect_response()
        {
            using var system = AlbaHost.ForStartup<WebApp.Startup>();
            var request = new OperationRequest
            {
                Type = OperationType.Multiply,
                One = 3,
                Two = 4
            };

            var result = await system.PostJson(request, "/math")
                .Receive<OperationResult>();
                
            result.Answer.ShouldBe(12);
            result.Method.ShouldBe("POST");
        }
        #endregion
        
        [Fact]
        public async Task put_and_expect_response()
        {
            using var system = AlbaHost.ForStartup<WebApp.Startup>();
            var request = new OperationRequest
            {
                Type = OperationType.Subtract,
                One = 3,
                Two = 4
            };

            var result = await system.PutJson(request, "/math")
                .Receive<OperationResult>();
                
            result.Answer.ShouldBe(-1);
            result.Method.ShouldBe("PUT");
        }
    }
}
