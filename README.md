<p align="center">
  <img src="docs/public/alba_logo.png" alt="Alba" width="300">
</p>

# Alba

[![Discord](https://img.shields.io/discord/1074998995086225460?color=blue&label=Chat%20on%20Discord)](https://discord.gg/WMxrvegf8H)
[![Nuget](https://img.shields.io/nuget/v/alba)](https://www.nuget.org/packages/Alba/)
![Nuget](https://img.shields.io/nuget/dt/alba)

Alba is a .NET library for fast, readable integration tests of ASP.NET Core applications. An Alba *scenario* runs an
HTTP request through your whole application in memory with ASP.NET Core's `TestServer` and checks the response with a
declarative API. It works with xUnit, NUnit and TUnit, and Alba 9 supports .NET 10 and .NET 11.

```bash
dotnet add package Alba
```

```cs
await using var host = await AlbaHost.For<Program>();

await host.Scenario(_ =>
{
    _.Post.Json(new CreateItem("Widget")).ToUrl("/items");
    _.StatusCodeShouldBe(201);
    _.JsonPath("$.name").ShouldBe("Widget");
});
```

Check out [the documentation](https://jasperfx.github.io/alba) for more examples. AI coding agents can use the
[Alba agent prompt](https://jasperfx.github.io/alba/agent-prompt.md) and the docs in [llms.txt](https://jasperfx.github.io/alba/llms.txt) form.

To work with the code, just open the solution at `src/Alba.sln` and go. No other setup necessary.

To run the documentation locally, use `build docs` on Windows or `./build.sh docs` on Linux or OSX. The documentation website will require
a recent installation of NPM. The documentation is built with [VitePress](https://vitepress.vuejs.org/).

## Support Plans

<div align="center">
    <img src="https://www.jasperfx.net/logo.png" alt="JasperFx logo" width="70%">
</div>

While Alba is open source, [JasperFx Software offers paid support and consulting contracts](https://bit.ly/3szhwT2) for Alba. 



