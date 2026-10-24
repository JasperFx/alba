using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Alba.Testing.Acceptance;

public record Checkout(decimal Amount);
public record Charge(string Id, decimal Amount);
public record Forecast(string Summary);

public class WeatherClient(HttpClient client)
{
    public Task<Forecast> Today() => client.GetFromJsonAsync<Forecast>("/forecast/today");
}

// Throws if used, so a test that reaches the real primary handler fails
public class UnreachableHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("The real primary handler was used");
    }
}

public class http_client_stub_usage
{
    private static async Task<IAlbaHost> startCheckoutApp(params IAlbaExtension[] extensions)
    {
        // Development shows exceptions from the application in the response body
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });

        builder.Services.AddHttpClient("payments", c => c.BaseAddress = new Uri("https://payments.example.com"))
            .ConfigurePrimaryHttpMessageHandler(() => new UnreachableHandler());
        builder.Services.AddHttpClient<WeatherClient>(c => c.BaseAddress = new Uri("https://weather.example.com"));

        return await AlbaHost.For(builder, app =>
        {
            app.MapPost("/checkout", async (Checkout checkout, IHttpClientFactory clients) =>
            {
                var payments = clients.CreateClient("payments");
                payments.DefaultRequestHeaders.Add("x-api-key", "test-key");

                try
                {
                    var response = await payments.PostAsJsonAsync("/charges?capture=true", new { amount = checkout.Amount });
                    if (!response.IsSuccessStatusCode) return Results.StatusCode(502);

                    var charge = await response.Content.ReadFromJsonAsync<Charge>();
                    return Results.Ok(new { chargeId = charge!.Id });
                }
                catch (HttpRequestException)
                {
                    return Results.StatusCode(503);
                }
            });

            app.MapGet("/weather", async (WeatherClient weather) => await weather.Today());
        }, extensions);
    }

    [Fact]
    public async Task stub_a_named_client()
    {
        #region sample_http_client_stub
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
        #endregion

        request.ClientName.ShouldBe("payments");
        request.Method.ShouldBe(HttpMethod.Post);
        request.ContentType!.MediaType.ShouldBe("application/json");
        request.ReadAsText().ShouldBe("""{"amount":25}""");
    }

    [Fact]
    public async Task stub_a_typed_client_by_its_class_name()
    {
        var weather = new HttpClientStub(nameof(WeatherClient))
            .When(HttpMethod.Get, "/forecast/today").RespondWithJson(new Forecast("Sunny"));

        await using var host = await startCheckoutApp(weather);

        (await host.GetAsJson<Forecast>("/weather"))!.Summary.ShouldBe("Sunny");
    }

    [Fact]
    public async Task stub_every_client()
    {
        var stub = new HttpClientStub()
            .When(HttpMethod.Get, "https://weather.example.com/forecast/today").RespondWithJson(new Forecast("Rain"))
            .When(HttpMethod.Post, "/charges").RespondWithJson(new Charge("ch_2", 5m));

        await using var host = await startCheckoutApp(stub);

        (await host.GetAsJson<Forecast>("/weather"))!.Summary.ShouldBe("Rain");
        await host.Scenario(x =>
        {
            x.Post.Json(new Checkout(5m)).ToUrl("/checkout");
            x.JsonPath("$.chargeId").ShouldBe("ch_2");
        });

        stub.Requests.Select(x => x.ClientName).ShouldBe([nameof(WeatherClient), "payments"]);
    }

    [Fact]
    public async Task the_last_matching_response_wins()
    {
        var payments = new HttpClientStub("payments")
            .When(HttpMethod.Post, "/charges").RespondWithJson(new Charge("ch_default", 1m));

        await using var host = await startCheckoutApp(payments);

        // A test can override a response that the fixture stubbed
        payments.When(HttpMethod.Post, "/charges").RespondWith(HttpStatusCode.PaymentRequired);

        await host.Scenario(x =>
        {
            x.Post.Json(new Checkout(1m)).ToUrl("/checkout");
            x.StatusCodeShouldBe(502);
        });
    }

    [Fact]
    public async Task match_the_query_string_when_the_path_has_one()
    {
        var payments = new HttpClientStub("payments")
            .When(HttpMethod.Post, "/charges?capture=false").RespondWith(HttpStatusCode.BadRequest)
            .When(HttpMethod.Post, "/charges?capture=true").RespondWithJson(new Charge("ch_3", 1m));

        await using var host = await startCheckoutApp(payments);

        await host.Scenario(x =>
        {
            x.Post.Json(new Checkout(1m)).ToUrl("/checkout");
            x.JsonPath("$.chargeId").ShouldBe("ch_3");
        });
    }

    [Fact]
    public async Task match_with_a_predicate_and_build_the_response_from_the_request()
    {
        var payments = new HttpClientStub("payments")
            .When(r => r.ReadAsJson<Checkout>()?.Amount > 100, "charges over 100")
            .RespondWith(r => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new Charge("ch_big", r.ReadAsJson<Checkout>()!.Amount))
            });

        await using var host = await startCheckoutApp(payments);

        await host.Scenario(x =>
        {
            x.Post.Json(new Checkout(500m)).ToUrl("/checkout");
            x.JsonPath("$.chargeId").ShouldBe("ch_big");
        });
    }

    [Fact]
    public async Task simulate_a_network_failure()
    {
        var payments = new HttpClientStub("payments")
            .When(HttpMethod.Post, "/charges").Throws(new HttpRequestException("Connection refused"));

        await using var host = await startCheckoutApp(payments);

        await host.Scenario(x =>
        {
            x.Post.Json(new Checkout(1m)).ToUrl("/checkout");
            x.StatusCodeShouldBe(503);
        });
    }

    [Fact]
    public async Task unmatched_requests_fail_inside_the_application_and_are_recorded()
    {
        var payments = new HttpClientStub("payments")
            .When(HttpMethod.Get, "/refunds").RespondWith(HttpStatusCode.OK);

        await using var host = await startCheckoutApp(payments);

        await host.Scenario(x =>
        {
            x.Post.Json(new Checkout(1m)).ToUrl("/checkout");
            x.StatusCodeShouldBe(500);
            x.ContentShouldContain(
                "No stubbed response for POST https://payments.example.com/charges?capture=true sent through the HttpClient 'payments'. Stubbed responses: GET /refunds");
        });

        payments.Requests.Single().ToString().ShouldBe("POST https://payments.example.com/charges?capture=true");
    }

    [Fact]
    public async Task clearing_requests_and_resetting()
    {
        var payments = new HttpClientStub("payments")
            .When(HttpMethod.Post, "/charges").RespondWithJson(new Charge("ch_4", 1m));

        await using var host = await startCheckoutApp(payments);

        await host.Scenario(x => x.Post.Json(new Checkout(1m)).ToUrl("/checkout"));
        payments.ClearRequests();
        payments.Requests.ShouldBeEmpty();

        // The stubbed response survives ClearRequests()
        await host.Scenario(x => x.Post.Json(new Checkout(1m)).ToUrl("/checkout"));
        payments.Requests.Count.ShouldBe(1);

        payments.Reset();
        payments.Requests.ShouldBeEmpty();
        await host.Scenario(x =>
        {
            x.Post.Json(new Checkout(1m)).ToUrl("/checkout");
            x.StatusCodeShouldBe(500);
            x.ContentShouldContain("No responses are stubbed.");
        });
    }
}
