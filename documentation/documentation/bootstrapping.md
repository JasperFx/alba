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

## Overriding the Content Root Path

If you use Alba's `SystemUnderTest.UseStartup<T>()` helper, Alba can guess at what the content root path of the application can be by using the name of the assembly that contains your `T` class. If that guessing isn't right, you can explicitly tell Alba what the content root path should be to search for content with either of these options:

<[sample:override_the_content_path]>

On a related note, Alba does not set any default value for the `EnvironmentName` property. That can be overridden through `IWebHostBuilder.UseEnvironment()` as normal.

## Hosting and Other Configuration

If you also want to run real HTTP requests through your system in a test harness, you have more opportunities to configure the underlying [IWebHostBuilder](https://docs.microsoft.com/en-us/aspnet/core/api/microsoft.aspnetcore.hosting.iwebhostbuilder) like so:

<[sample:configuration-overrides]>

A couple notes:

* Alba does not do anything to set the hosting environment, but you can do that yourself against `IWebHostBuilder`
* If you build a `SystemUnderTest` with `SystemUnderTest.ForStartup<T>()`, it will try to guess at the content root path by the name of assembly
  that holds the `Startup` class, but you may need to override that yourself. 

My shop is also using Alba within [Storyteller](http://storyteller.github.io) specifications where we use a mix of headless
Alba Scenario's and full HTTP requests for testing.


## Best Practices

Spinning up your ASP.Net Core system can become slow as the application grows, so you probably want to share the instance of 
`SystemUnderTest` between unit tests to avoid the cost of constantly bootstrapping and tearing down the underlying application.
As an example, if you're an xUnit user, you can take advantage of their [shared context](https://xunit.github.io/docs/shared-context.html) support to manage the lifetime of your `SystemUnderTest`.

