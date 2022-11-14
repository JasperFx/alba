using System;
using System.Threading.Tasks;
using Lamar;
using MinimalApiWithOakton;
using Oakton;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Alba.Testing.MimimalApi;

public class end_to_end_with_json_serialization : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private IAlbaHost _host;

    public end_to_end_with_json_serialization(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        OaktonEnvironment.AutoStartHost = true;
        _host = await AlbaHost.For<MinimalApiWithOakton.Program>();

        var container = (IContainer)_host.Services;
        _output.WriteLine(container.WhatDoIHave());
    }

    public Task DisposeAsync()
    {
        return _host.StopAsync();
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
}