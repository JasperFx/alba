using Microsoft.AspNetCore.Builder;
using Shouldly;

namespace MinimalApiUsageSample
{
    public static class Program
    {
        // Going to have to introduce just a little bit more ceremony
        // than you get from the standard template for minimal APIs
        
        // The new entry point would need to delegate a little bit
        // instead of being that new, empty file model
        // public static void Main(string[] args)
        // {
        //     CreateWebApplicationBuilder(args).Build().Run();
        // }

        // Introduce a new method for configuring the routes in the 
        // application that can be reused between the main application
        // bootstrapping and the test harness
        public static void ConfigureApplication(WebApplication app)
        {
            app.MapGet("/", () => "Hello World!");
        }

        // Introduce a new method for configuring the builder part of the 
        // application that can be reused between the main application
        // bootstrapping and the test harness. That doesn't preclude the ability
        // to override DI registrations for testing of course
        public static WebApplicationBuilder CreateWebApplicationBuilder(string[] args) =>
            WebApplication.CreateBuilder(args);
    }
}





namespace Alba.Testing.Samples
{
    public class MinimalApiUsage
    {
        [Fact]
        public async Task playing_with_minimal_apis()
        {
            // You'd share this host between tests in real usage, but let that go for now.
            // You want to bootstrap the application in testing basically the exact same way
            // as the real application without duplicating oodles of service registrations and other 
            // configuration, so we'll delegate to the methods in our Program class up above
            // and use this new extension method within Alba
            await using var host = await MinimalApiUsageSample.Program.CreateWebApplicationBuilder(new string[0])
                .StartAlbaAsync(MinimalApiUsageSample.Program.ConfigureApplication);

            // I'm adding this to the next Alba, but there are preexisting helpers for 
            // reading or posting data via JSON serialization
            var text = await host.GetAsText("/");
            
            text.ShouldBe("Hello World!");

        }
    }


}

