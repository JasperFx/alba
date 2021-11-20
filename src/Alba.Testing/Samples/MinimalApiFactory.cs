using System.Threading.Tasks;
using Shouldly;
using Xunit;

#if NET6_0_OR_GREATER

namespace Alba.Testing.Samples
{
    public class MinimalApiUsagev2
    {

        [Fact]
        public async Task bootstrapping_with_WebApplicationFactory()
        {
            // WebApplicationFactory can resolve old and new style of Program.cs
            // .NET 6 style - the global:: namespace prefix would not be required in a normal test project
            await using var host = await AlbaHost.For<global::Program>(x =>
            {
                x.ConfigureServices((context, services) =>
                {
                    //...
                });
            });

            var text = await host.GetAsText("/");
            text.ShouldBe("Hello World!");

            // Works with .NET 5 / Traditional entry point
            await using var host2 = await AlbaHost.For<WebApp.Program>(x =>
            {
                x.ConfigureServices((context, services) =>
                {
                    //...
                });
            });

            var text2 = await host2.GetAsText("/api/values");
            text2.ShouldBe("value1, value2");
        }
    }
}

#endif
