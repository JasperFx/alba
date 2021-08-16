---
title:Security Extensions
editLink:true
---

::: tip warning
Alba 3.0+ removed the previous, built in support for windows authentication.
:::

## Stub out all authentication

To just stub out all possible authentication inside your ASP.Net Core system in testing, you can use the new
`AuthenticationStub` to automatically authenticate every request and build out a `ClaimsPrincipal` to your specification.

Here's a sample of bootstrapping an `AlbaHost` with the `AuthenticationStub`:

<!-- snippet: sample_bootstrapping_with_stub_extension -->
<a id='snippet-sample_bootstrapping_with_stub_extension'></a>
```cs
// This is calling your real web service's configuration
var hostBuilder = Program.CreateHostBuilder(new string[0]);

// This is a new Alba v5 extension that can "stub" out
// JWT token authentication
var securityStub = new AuthenticationStub()
    .With("foo", "bar")
    .With(JwtRegisteredClaimNames.Email, "guy@company.com")
    .WithName("jeremy");

// AlbaHost was "SystemUnderTest" in previous versions of
// Alba
theHost = new AlbaHost(hostBuilder, securityStub);
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Security/web_api_authentication_with_stub.cs#L21-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_bootstrapping_with_stub_extension' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

When you need to test scenarios with different claims than the "baseline" claims defined on the `AuthenticationStub`
object you created in the `AlbaHost` setup, you can use the `WithClaim()` method as shown below to add additional 
claims to the principal for only one scenario as shown below:

<!-- snippet: sample_specify_specific_claims -->
<a id='snippet-sample_specify_specific_claims'></a>
```cs
[Fact]
public async Task can_modify_claims_per_scenario()
{
    var input = new Numbers
    {
        Values = new[] {2, 3, 4}
    };

    var response = await theHost.Scenario(x =>
    {
        // This is a custom claim that would only be used for the 
        // JWT token in this individual test
        x.WithClaim(new Claim("color", "green"));
        x.Post.Json(input).ToUrl("/math");
        x.StatusCodeShouldBeOk();
    });

    var principal = response.Context.User;
    principal.ShouldNotBeNull();
    
    principal.Claims.Single(x => x.Type == "color")
        .Value.ShouldBe("green");
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Security/web_api_authentication_with_stub.cs#L90-L116' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_specify_specific_claims' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Stub out JWT authentication

If you have an application that uses JWT, bearer tokens as your primary means of authentication, you can use the new
`JwtSecurityStub` to automatically add a valid JWT token string to the HTTP `Authorization` header before any scenarios are
executed. Similar to the `AuthenticationStub`, the `JwtSecurityStub` allows you to specify a baseline set of
claims that should be in the JWT token:

<!-- snippet: sample_setup_jwt_stub -->
<a id='snippet-sample_setup_jwt_stub'></a>
```cs
// This is calling your real web service's configuration
var hostBuilder = Program.CreateHostBuilder(new string[0]);

// This is a new Alba v5 extension that can "stub" out
// JWT token authentication
var jwtSecurityStub = new JwtSecurityStub()
    .With("foo", "bar")
    .With(JwtRegisteredClaimNames.Email, "guy@company.com");

// AlbaHost was "SystemUnderTest" in previous versions of
// Alba
theHost = new AlbaHost(hostBuilder, jwtSecurityStub);
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Security/web_api_authentication_with_jwt.cs#L21-L36' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_setup_jwt_stub' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The `JwtSecurityStub` reaches into your application's configuration to find the security signing key for JWT tokens, and
uses that key to sign the keys that it generates. The `JwtSecurityStub` will also disable any callouts to validate
the JWT tokens with a real Open Id Connect server **so you can test your service without an external token identity server running.**

The `JwtSecurityStub` will also honor the `WithClaim()` method to add additional claims on a scenario by scenario basis
as shown in the previous section.

## Integration with JWT Authentication

If you want to test your ASP.Net Core web services that are authenticated by an [Open Id Connect](https://openid.net/connect/) workflow **and**
you also want to be testing through the authentication from the real OIDC identity server, Alba v5 comes with new
extensions to automatically fetch and apply JWT tokens to scenario tests.

To use the OIDC [Client Credentials workflow](https://auth0.com/docs/flows/client-credentials-flow), you can use the `OpenConnectClientCredentials` extension:

<!-- snippet: sample_OpenConnectClientCredentials -->
<a id='snippet-sample_openconnectclientcredentials'></a>
```cs
oidc = new OpenConnectClientCredentials
{
    // These three properties are mandatory, and
    // would refer to matching configuration in your
    // OIDC server
    ClientId = Config.ClientId,
    ClientSecret = Config.ClientSecret,
    Scope = Config.ApiScope
};

theHost = Program.CreateHostBuilder(new string[0])
    .StartAlba(oidc);
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Security/OpenConnectClientCredentialsTests.cs#L23-L38' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_openconnectclientcredentials' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

To use the OIDC [Resource Owner Password Grant](https://docs.identityserver.io/en/release/quickstarts/2_resource_owner_passwords.html) workflow, 
you can use the `OpenConnectUserPassword` extension:

<!-- snippet: sample_applying_OpenConnectUserPassword -->
<a id='snippet-sample_applying_openconnectuserpassword'></a>
```cs
oidc = new OpenConnectUserPassword
{
    // All of these properties are mandatory
    ClientId = Config.ClientId,
    ClientSecret = Config.ClientSecret,
    UserName = "alice",
    Password = "alice"
};

theHost = WebAppSecuredWithJwt.Program.CreateHostBuilder(new string[0])
    .StartAlba(oidc);
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Security/OpenConnectUserPasswordTests.cs#L23-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_applying_openconnectuserpassword' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

With the `OpenConnectUserPassword` extension, you can also use a different user name and password for a single scenario with the `Scenario.UserAndPasswordIs(user, password)`
extension method as shown below:

<!-- snippet: sample_override_user_password -->
<a id='snippet-sample_override_user_password'></a>
```cs
[Fact]
public async Task post_to_a_secured_endpoint_with_jwt_with_overridden_user_and_password()
{
    // Building the input body
    var input = new Numbers
    {
        Values = new[] {2, 3, 4}
    };

    var response = await theHost.Scenario(x =>
    {
        // Alba deals with Json serialization for us
        x.Post.Json(input).ToUrl("/math");
        
        // Override the user credentials for just this scenario
        x.UserAndPasswordIs("bob", "bob");
        
        // Enforce that the HTTP Status Code is 200 Ok
        x.StatusCodeShouldBeOk();
    });

    var output = response.ReadAsJson<Result>();
    output.Sum.ShouldBe(9);
    output.Product.ShouldBe(24);

    var user = response.Context.User;
    user.FindFirst("name").Value.ShouldBe("Bob Smith");
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Security/OpenConnectUserPasswordTests.cs#L146-L177' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_override_user_password' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Windows Authentication

To add support for windows authentication in your Alba specifications, we recommend checking out the [AspNetCore.TestHost.WindowsAuth](https://github.com/IntelliTect/AspNetCore.TestHost.WindowsAuth) project.
