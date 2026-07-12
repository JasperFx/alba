using Alba.Testing.Sse;

namespace Alba.Testing.Samples
{
    public class ServerSentEvents
    {
        #region sample_sse_buffered
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
        #endregion

        #region sample_sse_streaming
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
        #endregion
    }
}
