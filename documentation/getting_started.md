<!--Title:Getting Started-->
<!--Url:getting_started-->

Alba itself is just a class library. All you really need to do to start with Alba is to install the Nuget to your testing
project for your application like so:

<pre>
PM> Install-Package Alba
</pre>

Or, using paket:

<pre>
paket add nuget Alba
</pre>

## How does Alba work?

Alba is a test helper within some kind of unit testing tool (xUnit, NUnit, etc) that provides a little help to
bootstrap an ASP.Net Core application in memory *the way that the application itself is configured* and to subsequently
execute and verify HTTP requests through the full application's middleware pipeline.

You can choose to spin up a Kestrel web host as part of bootstrapping a system with Alba, but the Alba *Scenario's* work
by directly invoking the ASP.Net Core application's underlying [RequestDelegate](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware) to exercise the full stack of middleware **without actually
making an HTTP request to a running web server**.


## Testing Hello, World

Now let's say that you built the obligatory hello world application for ASP.Net Core shown below:

<[sample:HelloWorldApp]>

We can now use Alba to declare an integration test for our Hello, World application within an [xUnit](http://xunit.github.io/)
testing project:

<[sample:should_say_hello_world]>

The sample up above bootstraps the application defined by our `Startup` and executes a *Scenario* against the running system.
A Scenario in Alba defines how the HTTP request should be constructed (the request body, headers, url) and optionally gives you
the ability to express assertions against the expected HTTP response.

Alba comes with plenty of helpers in its [fluent interface](https://www.martinfowler.com/bliki/FluentInterface.html) to work with the `HttpRequest` and `HttpResponse`, or you can work directly with the underlying ASP.Net Core objects:

<[sample:should_say_hello_world_with_raw_objects]>

Do note that Alba is not directly coupled to xUnit and would be usable within any .Net unit testing library.



To see where to go from here, see the <[linkto:documentation]>