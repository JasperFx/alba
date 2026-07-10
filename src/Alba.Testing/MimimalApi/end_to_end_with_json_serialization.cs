using Lamar;
using MinimalApiWithOakton;
using Shouldly;

namespace Alba.Testing.MimimalApi;

public class end_to_end_with_json_serialization : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private IAlbaHost _host;

    public end_to_end_with_json_serialization(ITestOutputHelper output)
    {
        _output = output;
    }

    public async ValueTask InitializeAsync()
    {
        _host = await AlbaHost.For<MinimalApiWithOakton.Program>();

        var container = (IContainer)_host.Services;
        _output.WriteLine(container.WhatDoIHave());
    }

    public async ValueTask DisposeAsync()
    {
        await _host.StopAsync();
    }

    [Fact]
    public async Task automatic_json_serialization()
    {
        var guid = Guid.NewGuid();

        var result = await _host.PostJson(new PostedMessage(guid), "/go")
            .Receive<OutputMessage>();

        result.Id.ShouldBe(guid);
    }
    
    [Fact]
    public async Task automatic_json_serialization_2()
    {
        var guid = Guid.NewGuid();

        var result = await _host.PostJson(new PostedMessage(guid), "/go", JsonStyle.MinimalApi)
            .Receive<OutputMessage>();

        result.Id.ShouldBe(guid);
    }

    [Fact]
    public async Task response_body_can_be_read_repeatedly_after_reading_json()
    {
        // Reading the body as JSON must not consume the one-shot TestHost response stream,
        // so that a subsequent read (e.g. snapshot tooling capturing the raw response)
        // still sees the body. See https://github.com/JasperFx/alba/issues/234
        var guid = Guid.NewGuid();

        var result = await _host.Scenario(x =>
        {
            x.Post.Json(new PostedMessage(guid)).ToUrl("/go");
        });

        (await result.ReadAsJsonAsync<OutputMessage>()).Id.ShouldBe(guid);

        (await result.ReadAsTextAsync()).ShouldNotBeNullOrEmpty();

        (await result.ReadAsJsonAsync<OutputMessage>()).Id.ShouldBe(guid);
    }

    [Fact]
    public async Task response_body_can_be_read_repeatedly_after_reading_json_synchronously()
    {
        var guid = Guid.NewGuid();

        var result = await _host.Scenario(x =>
        {
            x.Post.Json(new PostedMessage(guid)).ToUrl("/go");
        });

        result.ReadAsJson<OutputMessage>().Id.ShouldBe(guid);

        result.ReadAsText().ShouldNotBeNullOrEmpty();

        result.ReadAsJson<OutputMessage>().Id.ShouldBe(guid);
    }
}