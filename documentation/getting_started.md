<!--Title:Getting Started-->
<!--Url:getting_started-->

<[info]>
As of Alba 2.0+, there is now only the main Alba library and it only supports netcoreapp2.1+ applications. We have dropped all support for any version of ASP.Net Core before 2.1. The Alba.AspNetCore2 package has been deprecated, please switch to Alba proper.
<[/info]>

Alba is a class library that you use in combination with unit testing tools like [xUnit.Net](https://xunit.github.io) to author integration tests
against ASP.Net Core HTTP endpoints that actually exercises the full application stack by running HTTP requests through your ASP.Net system in memory. As of version 2.0, Alba uses the built in [TestHost](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-2.2) internally
to greatly improve its compatibility with many quirks of the ASP.Net Core model. 

<[warning]>
With the advent of the Microsoft.AspNetCore.All metapackage that is part of the new official guidance, ASP.Net Core projects are *very* susceptible
to Nuget version incompatibility issues and [diamond dependency conflicts](https://en.wikipedia.org/wiki/Dependency_hell) at runtime. Please see the comments in the csproj file shown below for workarounds
<[/warning]>

To start using Alba to write integration tests, make sure you have a test project for your web application and add a Nuget reference to Alba. If your application is called "WebApp," the csproj file should look like this (**please note the comments for workarounds to possible Nuget issues**):

<pre>
&lt;Project Sdk="Microsoft.NET.Sdk"&gt;
  &lt;PropertyGroup&gt;
    &lt;TargetFrameworks&gt;netcoreapp2.1;netcoreapp2.2&lt;/TargetFrameworks&gt;
    &lt;DebugType&gt;portable&lt;/DebugType&gt;

    &lt;!--THIS IS IMPORTANT TO PREVENT NUGET CONFLICTS BETWEEN ALBA
        AND YOUR APPLICATION  --&gt;
    &lt;TargetLatestRuntimePatch&gt;true&lt;/TargetLatestRuntimePatch&gt;
  &lt;/PropertyGroup&gt;
  &lt;ItemGroup&gt;
    &lt;!-- The reference to your ASP.Net Core application project  --&gt;
    &lt;ProjectReference Include="..\WebApp\WebApp.csproj" /&gt;
  &lt;/ItemGroup&gt;
  &lt;ItemGroup&gt;
    &lt;PackageReference Include="Microsoft.NET.Test.Sdk" Version="15.5.0" /&gt;
    &lt;PackageReference Include="xunit" Version="2.4.0" /&gt;
    &lt;PackageReference Include="xunit.runner.visualstudio" Version="2.4.0" /&gt;

    &lt;!-- This is still important to reference explicitly even though it's
         a transitive dependency of your web application  --&gt;
    &lt;PackageReference Include="Microsoft.AspNetCore.All" /&gt;
  &lt;/ItemGroup&gt;


&lt;/Project&gt;
</pre>

In addition, another workaround to potential binding conflicts is to explicitly specify the version of `Microsoft.AspNetCore.All` in your test project.

## Writing your first specification

For the purpose of this sample, let's say you generate a new web api project with the standard `dotnet new webapi` template. If you do that, you'll have this bootstrapping code in your `Program.Main()` method:

```
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
```

In this default setup, your application is more or less defined by the `Startup` class and there are no other customizations to the `WebHostBuilder`. 

Back in your test project, the easiest, and probably most common, usage of Alba is to send and verify JSON message bodies to `Controller` actions. To that end, let's say you have this very contrived controller and web models:

<[sample:MathController]>

First off, let's test the GET method in that controller above by passing a url and verifying the results:

<[sample: get-json]>

So what just happened in that test? First off, the call to `SystemUnderTest.For<T>()` bootstraps your web application using the `Startup` type from your web application. Behind the scenes, Alba is using the same `WebHost.CreateDefaultBuilder().UseStartup<T>()` mechanism, but the difference is that Alba uses [TestServer](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.testhost.testserver?view=aspnetcore-2.2) as a replacement for Kestrel (i.e., Alba does not spin up Kestrel during testing so there's no port conflicts). See <[linkto:documentation/bootstrapping]> for more information on advanced configuration options.

The call to `system.GetAsJson<OperationResult>("/math/add/3/4")` is performing these steps internally:

1. Formulate an [HttpRequest](https://docs.microsoft.com/en-us/dotnet/api/system.web.httprequest?view=netframework-4.7.2) object that will be passed to the application
1. Execute the web request against your application, which will run all configured middleware and any MVC controller classes that match the requested url
1. Assert that the response status code is `200 OK`
1. Read the raw JSON coming off the [HttpResponse](https://docs.microsoft.com/en-us/dotnet/api/system.web.httpresponse?view=netframework-4.7.2)
1. Deserialize the raw JSON to the requested `OperationResult` type using the Json serialization settings of the running application
1. Returns the resulting `OperationResult`

Alright then, let's try posting JSON in and examining the JSON out:

<[sample:post-json-get-json]>

It's a little more complicated, but the same goal is realized here. Allow the test author to work in terms of the application model objects while still exercising the entire HTTP middleware stack.

Don't stop here though, Alba also gives you the ability to declaratively assert on elements of the `HttpResponse` like expected header values, status codes, and assertions against the response body. In addition, Alba provides a lot of helper facilities to work with the raw `HttpResponse` data.


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
