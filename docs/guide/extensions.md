# Extension Model

Alba has an extension model based on this interface:

<!-- snippet: sample_IAlbaExtension -->
<a id='snippet-sample_IAlbaExtension'></a>
```cs
/// <summary>
/// Models an extension to an AlbaHost
/// </summary>
public interface IAlbaExtension : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Called during the initialization of an AlbaHost after the application is started,
    /// so the application DI container is available. Useful for registering setup or teardown
    /// actions on an AlbaHost
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    Task Start(IAlbaHost host);

    /// <summary>
    /// Allow an extension to alter the application under test before it starts.
    /// Behaves identically for every AlbaHost bootstrapping style.
    /// </summary>
    /// <param name="builder"></param>
    void Configure(IAlbaHostBuilder builder)
    {
    }
}

/// <summary>
/// Hosting-model-agnostic configuration surface handed to Alba extensions
/// before the application under test starts
/// </summary>
public interface IAlbaHostBuilder
{
    /// <summary>
    /// Add or replace services in the application under test
    /// </summary>
    void ConfigureServices(Action<IServiceCollection> configure);

    /// <summary>
    /// Add configuration sources for the application under test. Sources added
    /// here take precedence over the application's own configuration.
    /// </summary>
    void ConfigureConfiguration(Action<IConfigurationBuilder> configure);
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba/IAlbaExtension.cs#L6-L50' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_IAlbaExtension' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

`Configure` runs before the application under test starts and behaves the same regardless of how the
`AlbaHost` was bootstrapped (`IHostBuilder`, `WebApplicationBuilder`, or `WebApplicationFactory`). It
has a default no-op implementation, so extensions that only need post-start logic can implement
`Start` alone.

Extensions are applied when the `AlbaHost` is bootstrapped, either as arguments to any of the `AlbaHost.For(...)` methods or
chained onto the returned builder with `WithExtension(...)`. This sample from the security stub testing passes one as an argument:

<!-- snippet: sample_bootstrapping_with_stub_extension -->
<a id='snippet-sample_bootstrapping_with_stub_extension'></a>
```cs
// This is a Alba extension that can "stub" out authentication
var securityStub = new AuthenticationStub()
    .With("foo", "bar")
    .With(JwtRegisteredClaimNames.Email, "guy@company.com")
    .WithName("jeremy");

// We're calling your real web service's configuration
theHost = await AlbaHost.For<WebAppSecuredWithJwt.Program>(securityStub);
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Security/web_api_authentication_with_stub.cs#L15-L26' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_bootstrapping_with_stub_extension' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The same kind of extension chained onto the builder:

<!-- snippet: sample_bootstrapping_with_stub_scheme_extension -->
<a id='snippet-sample_bootstrapping_with_stub_scheme_extension'></a>
```cs
// Stub out an individual scheme
var securityStub = new AuthenticationStub("custom")
    .With("foo", "bar")
    .With(JwtRegisteredClaimNames.Email, "guy@company.com")
    .WithName("jeremy");

await using var host = await AlbaHost.For<WebAppSecuredWithJwt.Program>()
    .WithExtension(securityStub);
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Security/web_api_authentication_with_individual_stub.cs#L21-L32' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_bootstrapping_with_stub_scheme_extension' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

See [Configuring the Host Fluently](gettingstarted.md#configuring-the-host-fluently) for the full builder surface.

## Time Travel with TimeProviderOverride

Time-sensitive endpoints — token or session expiry, cache TTLs, scheduling, "valid until" business
rules — need a deterministic clock. The `TimeProviderOverride` extension replaces the application's
`TimeProvider` registration with a controllable [FakeTimeProvider](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.time.testing.faketimeprovider)
on every bootstrapping style. The extension *is* the clock: pass it to `AlbaHost.For(...)`, then
drive time from the test.

<!-- snippet: sample_time_provider_override -->
<a id='snippet-sample_time_provider_override'></a>
```cs
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
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/time_provider_override_usage.cs#L15-L40' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_time_provider_override' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The clock is frozen: `GetUtcNow()` returns the same instant until the test moves it with
`Advance(...)` or `SetUtcNow(...)`, or opts into automatic movement via `AutoAdvanceAmount`. Timers
created from the provider (including `Task.Delay(..., timeProvider)` and cache expirations) fire
when `Advance` crosses their due time.

The override only affects code that resolves `TimeProvider` from the container. Calls to
`DateTime.UtcNow` or a directly captured `TimeProvider.System` are not intercepted — that is
inherent to the `TimeProvider` pattern, not specific to Alba.

The extension itself doubles as a compact example of the extension model:

<!-- snippet: sample_TimeProviderOverride -->
<a id='snippet-sample_TimeProviderOverride'></a>
```cs
/// <summary>
/// Alba extension that replaces the application's <see cref="TimeProvider"/> registration
/// with this controllable <see cref="FakeTimeProvider"/> for time-travel testing. Pass an
/// instance to AlbaHost.For(...), then drive time from the test with Advance(...) and
/// SetUtcNow(...). Time is frozen unless AutoAdvanceAmount is set.
/// </summary>
public sealed class TimeProviderOverride : FakeTimeProvider, IAlbaExtension
{
    /// <summary>
    /// Starts the clock at FakeTimeProvider's default of 2000-01-01T00:00:00Z
    /// </summary>
    public TimeProviderOverride()
    {
    }

    /// <summary>
    /// Starts the clock at the supplied instant
    /// </summary>
    public TimeProviderOverride(DateTimeOffset startDateTime) : base(startDateTime)
    {
    }

    void IAlbaExtension.Configure(IAlbaHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(this);
        });
    }

    Task IAlbaExtension.Start(IAlbaHost host)
    {
        return Task.CompletedTask;
    }

    void IDisposable.Dispose()
    {
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba/TimeProviderOverride.cs#L7-L55' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_TimeProviderOverride' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
