<!--title: Integrating Alba with xUnit.Net-->

<[info]>
While this is using xUnit.Net, the mechanics should be similar for other unit test frameworks. The key point is just to share the 
hosted application test between tests while still cleaning up after yourself when you're done.
<[/info]>

If you are writing only a few Alba specifications in your testing project and your application spins up very quickly, you can just happily write tests like this:

<[sample:should_say_hello_world]>

Do note that your `[Fact]` method needs to be declared as `async Task` to ensure that xUnit finishes the specification before disposing the system or
you'll get *unusual* behavior. Also note that you really need to dispose the `SystemUnderTest` to shut down your application and dispose any internal services that might be holding on to computer resources.

If your application startup time becomes a performance problem, and especially in larger test suites, you probably want to share the `SystemUnderTest` object between tests. xUnit helpfully provides the [class fixture feature](https://xunit.github.io/docs/shared-context) for just this use case. 

In this case, build out your `SystemUnderTest` in a class like this:

<[sample: xUnit-Fixture]>

Then in your actual xUnit fixture classes, implement the `IClassFixture<T>` class like this:

<[sample:using-xUnit-Fixture]>