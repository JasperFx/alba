<!--title: Integrating Alba with xUnit.Net-->

::: tip warning
While this is using xUnit.Net, the mechanics should be similar for other unit test frameworks. The key point is just to share the 
hosted application test between tests while still cleaning up after yourself when you're done.
:::

If you are writing only a few Alba specifications in your testing project and your application spins up very quickly, you can just happily write tests like this:

snippet: sample_should_say_hello_world

Do note that your `[Fact]` method needs to be declared as `async Task` to ensure that xUnit finishes the specification before disposing the system or
you'll get *unusual* behavior. Also note that you really need to dispose the `AlbaHost` to shut down your application and dispose any internal services that might be holding on to computer resources.

If your application startup time becomes a performance problem, and especially in larger test suites, you probably want to share the `AlbaHost` object between tests. xUnit helpfully provides the [class fixture feature](https://xunit.github.io/docs/shared-context) for just this use case. 

In this case, build out your `AlbaHost` in a class like this:

snippet: sample_ xUnit_Fixture

Then in your actual xUnit fixture classes, implement the `IClassFixture<T>` class like this:

snippet: sample_using_xUnit_Fixture
