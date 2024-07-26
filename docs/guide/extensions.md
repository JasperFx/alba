# Extension Model

Alba has an extension model based on this interface:

<!-- snippet: sample_IAlbaExtension -->
<a id='snippet-sample_ialbaextension'></a>
```cs
/// <summary>
/// Models an extension to an AlbaHost
/// </summary>
public interface IAlbaExtension : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Called during the initialization of an AlbaHost after the application is started,
    /// so the application DI container is available. Useful for registering setup or teardown
    /// actions on an AlbaHOst
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    Task Start(IAlbaHost host);
        
    /// <summary>
    /// Allow an extension to alter the application's
    /// IHostBuilder prior to starting the application
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    IHostBuilder Configure(IHostBuilder builder);
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba/IAlbaExtension.cs#L7-L32' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_ialbaextension' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

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
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Security/web_api_authentication_with_stub.cs#L21-L32' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_bootstrapping_with_stub_extension' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
