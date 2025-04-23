# Alternative Bootstrapping Methods

Whilst `AlbaHost.For<T>` is the preferred way of bootstrapping, other options exist in Alba to cover some older & alternative scenarios.


## Initializing AlbaHost with IHostBuilder Directly

To bootstrap a **ASP.NET Core Startup.cs-style** application, create a `AlbaHost` using the definition of your `IHostBuilder` as shown below:

<!-- snippet: sample_Quickstart3 -->
<a id='snippet-sample_quickstart3'></a>
```cs
[Fact]
public async Task build_host_from_Program()    
{
    // Bootstrap your application just as your real application does
    var hostBuilder = Program.CreateHostBuilder(Array.Empty<string>());

    await using var host = new AlbaHost(hostBuilder);

    // Just as a sample, I'll run a scenario against
    // a "hello, world" application's root url
    await host.Scenario(s =>
    {
        s.Get.Url("/");
        s.ContentShouldBe("Hello, World!");
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Quickstart3.cs#L29-L46' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_quickstart3' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

::: tip
There are both synchronous and asynchronous methods to bootstrap an `AlbaHost`. Depending on your test harness, I recommend using 
the asynchronous version whenever applicable.
:::

Or alternatively, you can use one of the Alba extension methods off of `IHostBuilder` to start an `AlbaHost` object in a fluent interface
style:

<!-- snippet: sample_shorthand_bootstrapping -->
<a id='snippet-sample_shorthand_bootstrapping'></a>
```cs
[Fact]
public async Task fluent_interface_bootstrapping()    
{
    await using var host = await Program
        .CreateHostBuilder(Array.Empty<string>())
        .StartAlbaAsync();

    // Just as a sample, I'll run a scenario against
    // a "hello, world" application's root url
    await host.Scenario(s =>
    {
        s.Get.Url("/");
        s.ContentShouldBe("Hello, World!");
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Quickstart3.cs#L49-L65' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_shorthand_bootstrapping' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The `AlbaHost` is an extension of the standard .Net Core [IHost](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihost?view=dotnet-plat-ext-5.0) interface with a few additions for testing support.
While you can always access the underlying `TestServer` through the `IAlbaHost.Server` property, you're mostly going to be using the `Scenario()` method to write Alba "Scenario" tests.

::: tip
If you build a `AlbaHost` with `AlbaHost.ForStartup<T>()`, it will try to guess at the content root path by the name of assembly
  that holds the `Startup` class, but you may need to override that yourself. 
:::
