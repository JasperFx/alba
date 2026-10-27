#if NET11_0_OR_GREATER
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Shouldly;

namespace Alba.Testing;

// Alba's net11.0 build opts into runtime async, so the runtime runs its async methods
// instead of compiler generated state machines
public class runtime_async
{
    [Fact]
    public void alba_is_compiled_without_async_state_machines()
    {
        typeof(AlbaHost).Assembly.GetTypes()
            .Where(t => typeof(IAsyncStateMachine).IsAssignableFrom(t))
            .Select(t => t.FullName)
            .ShouldBeEmpty();
    }

    [Fact]
    public void alba_async_methods_are_runtime_async()
    {
        var scenario = typeof(AlbaHost).GetMethod(nameof(AlbaHost.Scenario))!;

        scenario.MethodImplementationFlags.HasFlag(MethodImplAttributes.Async).ShouldBeTrue();
    }

    [Fact]
    public async Task live_stack_traces_show_alba_frames_instead_of_state_machine_infrastructure()
    {
        string trace = null;

        await using var host = await AlbaHost.For(WebApplication.CreateBuilder(), app =>
        {
            app.MapGet("/", () => "ok");
        });

        // Runs inside Scenario, before the request is sent
        host.BeforeEachAsync(_ =>
        {
            trace = new StackTrace().ToString();
            return Task.CompletedTask;
        });

        await host.Scenario(x => x.Get.Url("/"));

        trace.ShouldContain("Alba.AlbaHost.Scenario");
        trace.ShouldNotContain("AsyncMethodBuilderCore");
    }
}
#endif
