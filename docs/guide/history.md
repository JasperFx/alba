# History

What is now Alba was originally created in the early 2010's as a mechanism to test the content negotiation features in [FubuMVC](https://fubumvc.github.io), an alternative web application framework that predates ASP.NET Core. 
We needed to run the entire web handling stack including all the middleware and HTTP endpoints just as the application would be configured. 
The [early *scenario* support in FubuMVC](https://jeremydmiller.com/2015/11/05/testing-http-handlers-with-no-web-server-in-sight/) was a way to run HTTP requests end to end
completely in memory and make declarative checks on expected HTTP behavior.

As FubuMVC wound down as a project, the scenario testing code was first extracted out into its own library called *Alba*, then ported to depend on an [OWIN-based](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/owin?view=aspnetcore-5.0) execution pipeline.
And then again with the advent of ASP.NET Core and the deprecation of OWIN, Alba was again re-wired to use the newer HTTP abstractions from the ASP.NET Core [HttpContext](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext?view=aspnetcore-5.0) type.
At this point, Alba is a value added wrapper around the [ASP.NET Core TestServer](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0).

The *scenario* testing in Alba was inspired by the [testing support in the Scala PlayFramework](https://www.playframework.com/documentation/2.8.x/ScalaFunctionalTestingWithSpecs2). 


