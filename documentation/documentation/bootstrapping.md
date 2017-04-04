<!--title: Bootstrapping and Configuration-->

Alba uses its `SystemUnderTest` class to manage the lifecycle of an ASP.Net Core application. The easiest way to use this
class is to directly use the `Startup` class for your application like so:

<[sample:SimplisticSystemUnderTest]>

Alba won't bootstrap your application until the first time you try to execute a *Scenario*, so you have the option
to apply additional configuration to how the system under test will execute.

## Running a Scenario

Once you have a `SystemUnderTest` object, you're ready to execute *Scenario's* through your system inside of tests.
Below is a scenario for the "hello, world" application:

<[sample:should_say_hello_world]>

The `Scenario()` method has this signature:

<[sample:ScenarioSignature]>

The single `Action<Scenario>` argument should completely configure the ASP.Net `HttpContext` for the request and apply
any of the declarative response assertions. The actual HTTP request happens inside of the `Scenario()` method, and the response contains the raw `HttpContext` and a helper object to work with the response body:

<[sample:IScenarioResult]>

If the existing *Scenario* assertions aren't enough to verify your test case, you can work directly against the raw response:

<[sample:should_say_hello_world_with_raw_objects]>

Do note that Alba quietly "rewinds" the `HttpContext.Response.Body` stream so that you can more readily read and work with the contents.

## Bootstrapping without Startup

It's a little bit uglier, but you can bypass having a `Startup` code and work directly against the ASP.Net Core `IWebHostBuilder`
interface like this:

<[sample:programmatic-bootstrapping]>

## The Hosting Environment

By default, Alba will try to guess the content root path by trying to find a directory "parallel" to the current testing project
that matches the name of the Assembly that contains the designated `Startup` class. To make that concrete, say that your
web application is in the directory `src/MyWebApp` and you are running the Alba tests in a project at `src/MyWebApp.Tests`.
In this case, Alba is able to find and apply the `src/MyWebApp` directory as the matching content root path. Otherwise, it just
uses the current directory and you may have to help Alba out a little bit by modifying the `IHostingEnvironment` for the application.

You can customize the [hosting environment](https://docs.microsoft.com/en-us/aspnet/core/api/microsoft.aspnetcore.hosting.ihostingenvironment) settings like so:

<[sample:configuring-IHostingEnvironment]>

Alba does not set any default value for the `EnvironmentName` property.

## Hosting and Other Configuration

If you also want to run real HTTP requests through your system in a test harness, you have more opportunities to configure the underlying [IWebHostBuilder](https://docs.microsoft.com/en-us/aspnet/core/api/microsoft.aspnetcore.hosting.iwebhostbuilder) like so:

<[sample:configuration-overrides]>

My shop is also using Alba within [Storyteller](http://storyteller.github.io) specifications where we use a mix of headless
Alba Scenario's and full HTTP requests for testing.

## Service Registrations

Maybe more commonly, you'll want to override the default service registrations with controlled stubs for your tests. In my shop's case, one of our biggest systems interacts with an external web service that would be very problematic in controlled automated
testing scenarios. In order to reliably test **our** application, we override the service gateway wrapper with a stubbed version that
can be easily controlled within the test harness. In Alba, you can use `SystemUnderTest.ConfigureServices()` to make service registration changes:

<[sample:configuration-overriding-services]>

## Best Practices

Spinning up your ASP.Net Core system can become slow as the application grows, so you probably want to share the instance of 
`SystemUnderTest` between unit tests to avoid the cost of constantly bootstrapping and tearing down the underlying application.
As an example, if you're an xUnit user, you can take advantage of their [shared context](https://xunit.github.io/docs/shared-context.html) support to manage the lifetime of your `SystemUnderTest`.

