# Server-Sent Events

Alba can test [server-sent event](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events) endpoints
two ways: finite streams through an ordinary scenario, and live streams through a dedicated streaming API. Events
are surfaced as `SseItem<T>` from `System.Net.ServerSentEvents`.

## Finite Streams

If the endpoint writes a bounded number of events and completes, a regular scenario works — the response is fully
buffered like any other. Parse the buffered body with `ReadAsServerSentEvents()`:

<!-- snippet: sample_sse_buffered -->
<a id='snippet-sample_sse_buffered'></a>
```cs
public async Task read_a_finite_event_stream(IAlbaHost host)
{
    // The endpoint writes a bounded number of events and completes,
    // so an ordinary scenario buffers the whole response
    var result = await host.Scenario(x =>
    {
        x.Get.Url("/sse/finite");
    });

    // Parse the buffered body as server-sent events
    var events = result.ReadAsServerSentEvents();

    // Or deserialize each data payload with the application's
    // JSON options
    var typed = result.ReadAsServerSentEvents<CounterEvent>();
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/ServerSentEvents.cs#L7-L24' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_sse_buffered' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The typed overload `ReadAsServerSentEvents<T>()` deserializes each `data:` payload as JSON using the application's
own `JsonSerializerOptions`, exactly like `ReadAsJson<T>()` does. Both reads are repeatable.

## Live Streams

Endpoints that stream indefinitely (or that you want to observe mid-stream) can't be buffered. Use
`StreamServerSentEvents` instead of `Scenario`:

<!-- snippet: sample_sse_streaming -->
<a id='snippet-sample_sse_streaming'></a>
```cs
public async Task stream_live_events(IAlbaHost host)
{
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

    // Returns as soon as the response headers arrive;
    // the response body is never buffered
    await using var stream = await host.StreamServerSentEvents(x =>
    {
        x.Get.Url("/sse/infinite");
    }, timeout.Token);

    // Events are yielded as the application writes them
    await foreach (var item in stream.ReadEvents(timeout.Token))
    {
        if (item.Data == "done") break;
    }

    // Disposing the stream aborts the request, cancelling the
    // endpoint's HttpContext.RequestAborted token
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/ServerSentEvents.cs#L26-L47' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_sse_streaming' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The call returns as soon as the application flushes its response headers, and `ReadEvents()` /
`ReadEvents<T>()` yield each event as the application writes it. The event stream can only be consumed once.

Disposing the `SseStreamResult` aborts the in-flight request, which cancels the endpoint's
`HttpContext.RequestAborted` token — a well-behaved streaming endpoint will observe that and stop. Any
`AfterEach`/`AfterEachAsync` actions run at disposal with a `null` `HttpContext`.

All `BeforeEach`/`BeforeEachAsync` actions and Alba's security extensions (such as `JwtSecurityStub`, including
per-scenario claims via `WithClaim`) apply to streams exactly as they do to scenarios.

## Limitations

* Response assertions (`ContentShouldContain`, header assertions, custom assertions) are not supported with
  `StreamServerSentEvents` and are rejected up front — assert on the streamed events instead. The expected status
  code *is* honored: a mismatch (for example a 401 from a missed auth setup) throws with the actual response body
  instead of presenting an empty stream.
* The response must have the `text/event-stream` content type.
* The `CancellationToken` argument to `StreamServerSentEvents` is the escape hatch for an endpoint that never
  starts its response.
