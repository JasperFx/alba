using System.Net.ServerSentEvents;
using Microsoft.AspNetCore.Routing;
using MinimalApiWithOakton;
using Shouldly;

namespace Alba.Testing.Acceptance;

public class MinimalApiHostFixture : IAsyncLifetime
{
    public IAlbaHost Host = null!;

    public async ValueTask InitializeAsync()
    {
        Host = await AlbaHost.For<MinimalApiWithOakton.Program>();
    }

    public async ValueTask DisposeAsync()
    {
        await Host.DisposeAsync();
    }
}

public class scenarios_by_route_name : IClassFixture<MinimalApiHostFixture>
{
    private readonly IAlbaHost _host;

    public scenarios_by_route_name(MinimalApiHostFixture fixture)
    {
        _host = fixture.Host;
    }

    [Fact]
    public async Task get_by_minimal_api_route_name()
    {
        var result = await _host.Scenario(s =>
        {
            s.Get.RouteNamed("GetAccount", new { accountId = "ab cd" });
            s.StatusCodeShouldBeOk();
        });

        var account = await result.ReadAsJsonAsync<AccountSummary>();
        account.AccountId.ShouldBe("ab cd");
        account.Query.ShouldBe("");
    }

    [Fact]
    public async Task route_name_lookup_ignores_case()
    {
        var account = await (await _host.Scenario(s => s.Get.RouteNamed("getaccount", new { accountId = "5" })))
            .ReadAsJsonAsync<AccountSummary>();

        account.AccountId.ShouldBe("5");
    }

    [Fact]
    public async Task values_that_are_not_route_parameters_become_the_query_string()
    {
        var result = await _host.Scenario(s =>
        {
            s.Get.RouteNamed("GetAccount", new { accountId = "5", include = "deposits" })
                .QueryString("page", "2");
        });

        var account = await result.ReadAsJsonAsync<AccountSummary>();
        account.AccountId.ShouldBe("5");
        account.Query.ShouldBe("?include=deposits&page=2");
    }

    [Fact]
    public async Task post_json_to_an_endpoint_with_only_route_name_metadata()
    {
        // The endpoint is registered the way Wolverine registers RouteName
        var result = await _host.Scenario(s =>
        {
            s.Post.Json(new MakeDeposit(100m)).ToRouteNamed("MakeDeposit", new { accountId = 42 });
        });

        result.Context.Request.Path.Value.ShouldBe("/api/accounts/42/deposits");

        var deposit = await result.ReadAsJsonAsync<DepositMade>();
        deposit.ShouldBe(new DepositMade(42, 100m));
    }

    [Fact]
    public async Task route_values_from_dictionaries()
    {
        var fromObjectDictionary = await _host.Scenario(s =>
            s.Get.RouteNamed("GetAccount", new Dictionary<string, object> { ["accountId"] = "1" }));
        (await fromObjectDictionary.ReadAsJsonAsync<AccountSummary>()).AccountId.ShouldBe("1");

        var fromStringDictionary = await _host.Scenario(s =>
            s.Get.RouteNamed("GetAccount", new Dictionary<string, string> { ["accountId"] = "2" }));
        (await fromStringDictionary.ReadAsJsonAsync<AccountSummary>()).AccountId.ShouldBe("2");

        var fromRouteValueDictionary = await _host.Scenario(s =>
            s.Get.RouteNamed("GetAccount", new RouteValueDictionary { ["accountId"] = "3" }));
        (await fromRouteValueDictionary.ReadAsJsonAsync<AccountSummary>()).AccountId.ShouldBe("3");
    }

    [Fact]
    public async Task stream_server_sent_events_by_route_name()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        await using var stream = await _host.StreamServerSentEvents(x => x.Get.RouteNamed("FiniteCounters"), timeout.Token);

        var events = new List<SseItem<string>>();
        await foreach (var item in stream.ReadEvents(timeout.Token))
        {
            events.Add(item);
        }

        events.Count.ShouldBe(3);
    }

    [Fact]
    public async Task unknown_route_name_fails_before_the_request_with_the_known_route_names()
    {
        await using var host = await AlbaHost.For<MinimalApiWithOakton.Program>();
        var requests = 0;
        host.BeforeEach(_ => requests++);

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            host.Scenario(s => s.Get.RouteNamed("Nope", new { accountId = "1" })));

        ex.Message.ShouldContain("No endpoint has the route name 'Nope'.");
        ex.Message.ShouldContain("FiniteCounters: GET /sse/finite");
        ex.Message.ShouldContain("GetAccount: GET /api/accounts/{accountId}");
        ex.Message.ShouldContain("MakeDeposit: POST /api/accounts/{accountId:int}/deposits");

        requests.ShouldBe(0);
    }

    [Fact]
    public async Task missing_required_route_value_names_the_value()
    {
        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            _host.Scenario(s => s.Post.Json(new MakeDeposit(1m)).ToRouteNamed("MakeDeposit")));

        ex.Message.ShouldContain("Could not generate a url for the route name 'MakeDeposit'");
        ex.Message.ShouldContain("Route: POST /api/accounts/{accountId:int}/deposits");
        ex.Message.ShouldContain("Missing required route values: accountId");
        ex.Message.ShouldContain("Supplied route values: (none)");
    }

    [Fact]
    public async Task route_value_that_fails_a_constraint_points_at_the_constraints()
    {
        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            _host.Scenario(s => s.Post.Json(new MakeDeposit(1m)).ToRouteNamed("MakeDeposit", new { accountId = "abc" })));

        ex.Message.ShouldContain("Check that the supplied values satisfy the route constraints.");
        ex.Message.ShouldContain("Supplied route values: accountId=abc");
    }

    [Fact]
    public async Task blank_route_name_is_rejected()
    {
        await Should.ThrowAsync<ArgumentException>(() => _host.Scenario(s => s.Get.RouteNamed(" ")));
    }
}

public class scenarios_by_mvc_route_name
{
    [Fact]
    public async Task get_by_mvc_attribute_route_name()
    {
        await using var host = await AlbaHost.For<WebApp.Program>();

        await host.Scenario(s =>
        {
            s.Get.RouteNamed("GetMvcAccount", new { accountId = 3 });
            s.ContentShouldBe("account 3");
        });
    }
}
