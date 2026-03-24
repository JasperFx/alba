using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Shouldly;

namespace Alba.Testing.Acceptance;

public class host_logger
{
    [Fact]
    public async Task should_not_use_windows_event_log()
    {
        await using var host = await AlbaHost.For<Program>();

        var loggerProviders = host.Services.GetServices<ILoggerProvider>();

        loggerProviders.Any(p => p is EventLogLoggerProvider).ShouldBeFalse();
    }
}
