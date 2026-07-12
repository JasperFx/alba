using Shouldly;

namespace Alba.Testing.Sse;

public class buffered_sse_reading : IAsyncLifetime
{
    private IAlbaHost _host = null!;

    public async ValueTask InitializeAsync()
    {
        _host = await AlbaHost.For<MinimalApiWithOakton.Program>();
    }

    public async ValueTask DisposeAsync()
    {
        await _host.DisposeAsync();
    }

    [Fact]
    public async Task reads_events_from_a_buffered_response()
    {
        var result = await _host.Scenario(x => x.Get.Url("/sse/finite"));

        var events = result.ReadAsServerSentEvents();

        events.Count.ShouldBe(3);
        events[0].EventType.ShouldBe("count");
        events[0].EventId.ShouldBe("1");
        events[0].Data.ShouldBe("{\"number\":1}");
        events[2].EventId.ShouldBe("3");
    }

    [Fact]
    public async Task reads_typed_events_with_the_application_json_options()
    {
        var result = await _host.Scenario(x => x.Get.Url("/sse/finite"));

        var events = result.ReadAsServerSentEvents<CounterEvent>();

        // Number is a public field, so this only works when the application's
        // IncludeFields option is applied
        events.Select(e => e.Data.Number).ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public async Task reading_is_repeatable()
    {
        var result = await _host.Scenario(x => x.Get.Url("/sse/finite"));

        result.ReadAsServerSentEvents().Count.ShouldBe(3);
        result.ReadAsServerSentEvents().Count.ShouldBe(3);
    }

    [Fact]
    public async Task body_without_sse_framing_yields_no_events()
    {
        var result = await _host.Scenario(x =>
        {
            x.Get.Url("/");
        });

        result.ReadAsServerSentEvents().Count.ShouldBe(0);
    }
}

public class CounterEvent
{
    public int Number;
}
