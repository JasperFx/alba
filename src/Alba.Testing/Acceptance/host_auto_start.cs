using JasperFx.CommandLine;
using Shouldly;

namespace Alba.Testing.Acceptance;

public class host_auto_start
{
    [Fact]
    public async Task should_enable_auto_start_host()
    {
        await using var host = await AlbaHost.For<Program>();

        JasperFxEnvironment.AutoStartHost.ShouldBe(true);
    }
}
