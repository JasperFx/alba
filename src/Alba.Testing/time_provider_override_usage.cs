using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinimalApiWithOakton;
using Shouldly;

namespace Alba.Testing
{
    public class time_provider_override_usage
    {
        private static readonly DateTimeOffset TheStart = new(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);

        #region sample_time_provider_override
        [Fact]
        public async Task freezing_and_advancing_the_clock()
        {
            // The clock is both the extension and the controllable TimeProvider
            var clock = new TimeProviderOverride(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero));

            await using var host = await AlbaHost.For<MinimalApiWithOakton.Program>(clock);

            // Time is frozen at the configured start instant
            (await host.GetAsJson<CurrentTime>("/time"))!.UtcNow
                .ShouldBe(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero));
            (await host.GetAsJson<CurrentTime>("/time"))!.UtcNow
                .ShouldBe(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero));

            // Travel forward three days
            clock.Advance(TimeSpan.FromDays(3));
            (await host.GetAsJson<CurrentTime>("/time"))!.UtcNow
                .ShouldBe(new DateTimeOffset(2030, 1, 4, 0, 0, 0, TimeSpan.Zero));

            // Or pin the clock to any instant
            clock.SetUtcNow(new DateTimeOffset(2031, 6, 15, 12, 0, 0, TimeSpan.Zero));
            (await host.GetAsJson<CurrentTime>("/time"))!.UtcNow
                .ShouldBe(new DateTimeOffset(2031, 6, 15, 12, 0, 0, TimeSpan.Zero));
        }
        #endregion

        [Fact]
        public async Task the_clock_is_the_only_time_provider_registration()
        {
            var clock = new TimeProviderOverride(TheStart);

            // MinimalApiWithOakton registers TimeProvider.System itself, and the
            // framework TryAdds one as well; the override must be the single winner
            await using var host = await AlbaHost.For<MinimalApiWithOakton.Program>(clock);

            host.Services.GetRequiredService<TimeProvider>().ShouldBeSameAs(clock);
            host.Services.GetServices<TimeProvider>().Single().ShouldBeSameAs(clock);
        }

        [Fact]
        public async Task with_host_builder()
        {
            var clock = new TimeProviderOverride(TheStart);

            var builder = new HostBuilder().ConfigureWebHost(x =>
            {
                x.ConfigureServices(s => s.AddSingleton(TimeProvider.System));
                x.Configure(app => app.Run(c => c.Response.WriteAsync("ok")));
            });

            await using var host = await AlbaHost.For(builder, clock);

            assertClockWasApplied(host, clock);
        }

        [Fact]
        public async Task with_web_application_builder()
        {
            var clock = new TimeProviderOverride(TheStart);

            var builder = WebApplication.CreateBuilder();
            builder.Services.AddSingleton(TimeProvider.System);

            await using var host = await builder.StartAlbaAsync(app => { }, clock);

            assertClockWasApplied(host, clock);
        }

        [Fact]
        public async Task with_web_application_factory()
        {
            var clock = new TimeProviderOverride(TheStart);

            await using var host = await AlbaHost.For<MinimalApiWithOakton.Program>(clock);

            assertClockWasApplied(host, clock);
        }

        private static void assertClockWasApplied(IAlbaHost host, TimeProviderOverride clock)
        {
            var provider = host.Services.GetRequiredService<TimeProvider>();
            provider.ShouldBeSameAs(clock);
            provider.GetUtcNow().ShouldBe(TheStart);
        }
    }
}
