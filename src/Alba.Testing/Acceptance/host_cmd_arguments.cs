using JasperFx.CommandLine;
using Shouldly;

namespace Alba.Testing.Acceptance;

public class host_cmd_arguments
{

    [Fact]
    public async Task should_start_host_when_running_without_RunJasperFxCommands()
    {
        await using var host = await AlbaHost.For<Program>();

        var args = await GetCmdArgumentsAsync(host);
        args.Keys.ShouldBe([
            "--Logging__LogLevel__Microsoft.Identity.Web",
            "--Logging__EventLog__LogLevel__Default",
            "--environment",
            "--contentRoot",
            "--applicationName",
        ]);
    }

    [Fact]
    public async Task should_start_host_when_using_RunJasperFxCommands()
    {
        await using var host = await AlbaHost.For<Program>(x =>
            x.UseSetting("UseRunJasperFxCommands", "true"));

        var args = await GetCmdArgumentsAsync(host);
        args.Keys.ShouldBe([
            "--UseRunJasperFxCommands",
            "--Logging__LogLevel__Microsoft.Identity.Web",
            "--Logging__EventLog__LogLevel__Default",
            "--environment",
            "--contentRoot",
            "--applicationName"
        ]);
    }

    private static async Task<Dictionary<string, string>> GetCmdArgumentsAsync(IAlbaHost host)
    {
        var result = await host.Scenario(x =>
        {
            x.Get.Url("/args");
            x.StatusCodeShouldBeSuccess();
        });

        var argsJson = await result.ReadAsJsonAsync<string[]>();
        var args = argsJson.Select(x => x.Split("="))
            .ToDictionary(x => x[0], x => x[1]);
        return args;
    }
}