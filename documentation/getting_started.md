<!--Title:Getting Started-->
<!--Url:getting_started-->

::: tip warning
As of 4.0+, Alba only supports netcoreapp3.1+ applications. We have dropped all support for any version of ASP.NET Core before 3.1. Use Alba 3.x if you still require 2.1 support.
:::

Alba is a class library that you use in combination with unit testing tools like [xUnit.Net](https://xunit.github.io) to author integration tests
against ASP.NET Core HTTP endpoints that actually exercises the full application stack by running HTTP requests through your ASP.NET system in memory. As of version 2.0, Alba uses the built in [TestHost](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-2.2) internally
to greatly improve its compatibility with many quirks of the ASP.NET Core model. 

## ASP.NET Core 3.1 & 5.0

If you start a new ASP.NET Core project with `dotnet new webapi`, you'll get this code in your `Program` file:

snippet: sample_WebApi3StandardTemplate

To connect that to Alba, create a `SystemUnderTest` like this using the definition of your `IHostBuilder`:

snippet: sample_Quickstart3

In reality though, you probably want to check out the <[linkto:documentation/xunit]> to do it a little more efficiently.


## Writing your first specification

For the purpose of this sample, let's say you generate a new web api project with the standard `dotnet new webapi` template. If you do that, you'll have this bootstrapping code in your `Program.Main()` method:

```
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
}
```

In this default setup, your application is more or less defined by the `Startup` class and there are no other customizations to the `HostBuilder`. 

Back in your test project, the easiest, and probably most common, usage of Alba is to send and verify JSON message bodies to `Controller` actions. To that end, let's say you have this very contrived controller and web models:

snippet: sample_MathController

First off, let's test the GET method in that controller above by passing a url and verifying the results:

snippet: sample_ get_json

So what just happened in that test? First off, the call to `SystemUnderTest.For<T>()` bootstraps your web application using the `Startup` type from your web application. Behind the scenes, Alba is using the same `Host.CreateDefaultBuilder().ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<T>())` mechanism, but the difference is that Alba uses [TestServer](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.testhost.testserver) as a replacement for Kestrel (i.e., Alba does not spin up Kestrel during testing so there's no port conflicts). See <[linkto:documentation/bootstrapping]> for more information on advanced configuration options.

The call to `system.GetAsJson<OperationResult>("/math/add/3/4")` is performing these steps internally:

1. Formulate an [HttpRequest](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httprequest?view=aspnetcore-5.0) object that will be passed to the application
1. Execute the web request against your application, which will run all configured middleware and any MVC controller classes that match the requested url
1. Assert that the response status code is `200 OK`
1. Read the raw JSON coming off the [HttpResponse](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpresponse?view=aspnetcore-5.0)
1. Deserialize the raw JSON to the requested `OperationResult` type using the Json serialization settings of the running application
1. Returns the resulting `OperationResult`

Alright then, let's try posting JSON in and examining the JSON out:

snippet: sample_post_json_get_json

It's a little more complicated, but the same goal is realized here. Allow the test author to work in terms of the application model objects while still exercising the entire HTTP middleware stack.

Don't stop here though, Alba also gives you the ability to declaratively assert on elements of the `HttpResponse` like expected header values, status codes, and assertions against the response body. In addition, Alba provides a lot of helper facilities to work with the raw `HttpResponse` data.


## Testing Hello, World

Now let's say that you built the obligatory hello world application for ASP.Net Core shown below:

snippet: sample_HelloWorldApp

We can now use Alba to declare an integration test for our Hello, World application within an [xUnit](http://xunit.github.io/)
testing project:

snippet: sample_should_say_hello_world

The sample up above bootstraps the application defined by our `Startup` and executes a *Scenario* against the running system.
A Scenario in Alba defines how the HTTP request should be constructed (the request body, headers, url) and optionally gives you
the ability to express assertions against the expected HTTP response.

Alba comes with plenty of helpers in its [fluent interface](https://www.martinfowler.com/bliki/FluentInterface.html) to work with the `HttpRequest` and `HttpResponse`, or you can work directly with the underlying ASP.Net Core objects:

snippet: sample_should_say_hello_world_with_raw_objects

Do note that Alba is not directly coupled to xUnit and would be usable within any .Net unit testing library.



To see where to go from here, see the <[linkto:documentation]>
