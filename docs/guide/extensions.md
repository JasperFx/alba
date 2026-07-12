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

When you are initializing an `AlbaHost`, you can pass in an optional array of extensions like this sample from the security stub
testing:

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

## Configuration Extension

In some scenarios it may be more convienent to override a configuration value rather than modifying the service configuration. Due to a current [limitation](https://github.com/dotnet/aspnetcore/issues/37680) in the ASP.NET Core test host, overriding your application's configuration values requires a workaround. Alba includes this workaround out of the box via the `ConfigurationOverride` extension.
For example, to override an application's Postgres connection string:

<!-- snippet: sample_configuration_extension -->
<a id='snippet-sample_configuration_extension'></a>
```cs
var configValues = new Dictionary<string, string?>()
{
    { "ConnectionStrings:Postgres", "MyOverriddenValue" }
};

var host = await AlbaHost.For<WebAppSecuredWithJwt.Program>(builder =>
{
    builder.ConfigureServices(c =>
    {
        // services config
    });
}, ConfigurationOverride.Create(configValues));
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Extensions.cs#L8-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_configuration_extension' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
