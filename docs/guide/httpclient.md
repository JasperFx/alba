# Stubbing Outbound HTTP

Applications often call other services, such as a payment provider, an email API or another team's service.
`HttpClientStub` is an Alba extension that replaces those calls with stubbed responses and records every request, so
scenarios run the real application without reaching the real services.

It replaces the primary handler of clients created through `IHttpClientFactory`: named clients, typed clients and the
default `HttpClient`. The application's own delegating handlers still run, and a primary handler the application
configures is replaced.

In this test, the application's `/checkout` endpoint charges a card through a named client, `payments`:

<!-- snippet: sample_http_client_stub -->
<a id='snippet-sample_http_client_stub'></a>
```cs
var payments = new HttpClientStub("payments")
    .When(HttpMethod.Post, "/charges").RespondWithJson(new Charge("ch_1", 25m));

await using var host = await startCheckoutApp(payments);

await host.Scenario(x =>
{
    x.Post.Json(new Checkout(25m)).ToUrl("/checkout");
    x.JsonPath("$.chargeId").ShouldBe("ch_1");
});

// Every outbound request is recorded with its body
var request = payments.Requests.Single();
request.Uri.ShouldBe(new Uri("https://payments.example.com/charges?capture=true"));
request.Headers.GetValues("x-api-key").ShouldBe(["test-key"]);
request.ReadAsJson<Checkout>()!.Amount.ShouldBe(25m);
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Acceptance/http_client_stub_usage.cs#L67-L84' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_http_client_stub' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Choosing the clients

- `new HttpClientStub("payments")` stubs the named client `payments`.
- A typed client's name is its class name, so `new HttpClientStub(nameof(WeatherClient))` stubs
  `AddHttpClient<WeatherClient>()`.
- `new HttpClientStub()` stubs every client from the factory. Each recorded request has a `ClientName`.

Pass the stubs to `AlbaHost.For(...)` alongside any other extensions.

## Stubbing responses

`When(HttpMethod.Post, "/charges")` matches a method and path. The path is matched without regard to case. The query
string is only compared when the path includes one, and an absolute url such as
`"https://payments.example.com/charges"` also matches the host. `When(request => ..., "a description")` matches with a
predicate over the recorded request, including its body.

Each rule then responds with one of:

- `RespondWith(HttpStatusCode.NoContent)`: an empty body
- `RespondWithJson(value)`: the value serialized with the stub's `JsonOptions`, the web defaults unless you change them
- `RespondWithText("...")`
- `RespondWith(request => new HttpResponseMessage(...))`: a response built from the request
- `Throws(new HttpRequestException("Connection refused"))`: a simulated network failure

A rule with no response configured answers with an empty 200. These methods return the stub, so a fixture can chain
several rules when it creates the stub.

When several rules match a request, the one added last wins. A fixture can stub the usual responses once, and a test
can add a rule that overrides one of them. `ClearRequests()` empties the recorded requests, and `Reset()` also removes
every rule.

A request that matches no rule throws an `InvalidOperationException` inside the application, naming the request and
the rules that exist. The request is still recorded.

## Tips

- The stub is shared state. Tests that add rules or check `Requests` should not run in parallel against the same host.
  Tests that share an xUnit fixture already run one at a time.
- A resilience handler such as `AddStandardResilienceHandler()` retries stubbed 5xx responses after a delay. Stub a 4xx
  response, or leave the resilience handler out of the test host, when testing error handling.
- Only clients from `IHttpClientFactory` are stubbed. An `HttpClient` that the application creates with `new` is not.
