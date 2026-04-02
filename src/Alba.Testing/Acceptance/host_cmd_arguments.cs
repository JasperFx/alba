using Shouldly;

namespace Alba.Testing.Acceptance;

public class host_cmd_arguments
{
    private static readonly string[] DefaultParameters =
    [
        "--environment",
        "--contentRoot",
        "--applicationName",
    ];

    [Fact]
    public async Task should_use_default_cmd_parameters_when_running_without_RunJasperFxCommand()
    {
        await using var host = await AlbaHost.For<Program>();

        var args = await GetCmdArgumentsAsync(host);
        args.Keys.ShouldBe(DefaultParameters, ignoreOrder: true);
    }

    [Fact]
    public async Task should_use_extra_cmd_parameters_when_running_without_RunJasperFxCommand()
    {
        await using var host = await AlbaHost.For<Program>(x => x
            .UseSetting("extra1", "value")
            .UseSetting("extra2", null));

        var args = await GetCmdArgumentsAsync(host);
        args.Keys.ShouldBe(
            DefaultParameters.Concat(["--extra1", "--extra2"]),
            ignoreOrder: true);
    }

    [Fact]
    public async Task should_use_default_cmd_parameters_when_running_with_RunJasperFxCommand()
    {
        await using var host = await AlbaHost.For<MinimalApiWithOakton.Program>();

        var args = await GetCmdArgumentsAsync(host);
        args.Keys.ShouldBe(DefaultParameters, ignoreOrder: true);
    }

    [Fact]
    public async Task should_use_extra_cmd_parameters_when_running_with_RunJasperFxCommand()
    {
        await using var host = await AlbaHost.For<MinimalApiWithOakton.Program>(x => x
            .UseSetting("extra1", "value")
            .UseSetting("extra2", null));

        var args = await GetCmdArgumentsAsync(host);
        args.Keys.ShouldBe(
            DefaultParameters.Concat(["--extra1", "--extra2"]),
            ignoreOrder: true);
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