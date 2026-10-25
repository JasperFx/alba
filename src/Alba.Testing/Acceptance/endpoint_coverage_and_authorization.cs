using System.Net.ServerSentEvents;
using Alba.Extensions;
using Alba.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Alba.Testing.Acceptance;

public class endpoint_coverage_and_authorization
{
    private static async Task<IAlbaHost> startAccountsApp(AuthorizationPolicy fallbackPolicy = null,
        EndpointCoverage coverage = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddAuthentication("Bearer");
        builder.Services.AddAuthorization(o =>
        {
            o.AddPolicy("admin", p => p.RequireAuthenticatedUser());
            o.FallbackPolicy = fallbackPolicy;
        });

        return await AlbaHost.For(builder, app =>
        {
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/accounts/{id:int}", (int id) => new { id }).RequireAuthorization();
            app.MapPost("/accounts", () => Results.Created("/accounts/1", null)).RequireAuthorization("admin");
            app.MapDelete("/accounts/{id:int}", [Authorize, AllowAnonymous] (int id) => Results.NoContent());
            app.MapGet("/health", () => "ok").AllowAnonymous();
            app.MapGet("/public", () => "hello");
            app.MapGet("/conflict", () => Results.Conflict());
            app.MapMethods("/accounts/{id:int}/close", ["PUT", "PATCH"], (int id) => Results.NoContent()).RequireAuthorization();
            app.MapGet("/events", () => TypedResults.ServerSentEvents(new[] { new SseItem<string>("one") }.ToAsyncEnumerable()))
                .AllowAnonymous();
        }, coverage == null ? [new AuthenticationStub()] : [new AuthenticationStub(), coverage]);
    }

    private static readonly string[] AllAccountEndpoints =
    [
        "DELETE /accounts/{id:int}",
        "GET /accounts/{id:int}",
        "GET /conflict",
        "GET /events",
        "GET /health",
        "GET /public",
        "POST /accounts",
        "PUT, PATCH /accounts/{id:int}/close"
    ];

    [Fact]
    public async Task nothing_is_exercised_before_the_first_request()
    {
        var coverage = new EndpointCoverage();
        await using var host = await startAccountsApp(coverage: coverage);

        coverage.NotExercised().ShouldBe(AllAccountEndpoints);
    }

    [Fact]
    public async Task endpoints_count_once_routing_selects_them_whatever_the_response()
    {
        var coverage = new EndpointCoverage();
        await using var host = await startAccountsApp(coverage: coverage);

        await host.Scenario(x => x.Get.Url("/accounts/5"));
        await host.Scenario(x => x.Get.Url("/accounts/6"));
        await host.Scenario(x =>
        {
            x.Get.Url("/conflict");
            x.StatusCodeShouldBe(409);
        });
        await host.Scenario(x =>
        {
            x.Patch.Url("/accounts/5/close");
            x.StatusCodeShouldBe(204);
        });

        // Requests that match no endpoint do not count against any endpoint
        await host.Scenario(x =>
        {
            x.Get.Url("/nowhere");
            x.StatusCodeShouldBe(404);
        });

        await using (var stream = await host.StreamServerSentEvents(x => x.Get.Url("/events"), TestContext.Current.CancellationToken))
        {
            await foreach (var _ in stream.ReadEvents(TestContext.Current.CancellationToken)) { }
        }

        // Requests through the TestServer's own HttpClient count as well
        using var client = host.Server.CreateClient();
        (await client.GetStringAsync("/health", TestContext.Current.CancellationToken)).ShouldBe("ok");

        coverage.NotExercised().ShouldBe([
            "DELETE /accounts/{id:int}",
            "GET /public",
            "POST /accounts"
        ]);
    }

    [Fact]
    public async Task coverage_is_shared_by_every_host_it_is_passed_to()
    {
        var coverage = new EndpointCoverage();

        // Two hosts of the same application, as in a pool of hosts for concurrent tests
        await using var first = await startAccountsApp(coverage: coverage);
        await using var second = await startAccountsApp(coverage: coverage);

        await first.Scenario(x => x.Get.Url("/health"));
        await second.Scenario(x => x.Get.Url("/public"));

        coverage.NotExercised().ShouldNotContain("GET /health");
        coverage.NotExercised().ShouldNotContain("GET /public");
    }

    [Fact]
    public async Task coverage_survives_the_hosts_and_ignores_hosts_without_it()
    {
        var coverage = new EndpointCoverage();

        await using (var host = await startAccountsApp(coverage: coverage))
        {
            await host.Scenario(x => x.Get.Url("/health"));
        }

        // A host that was not given the coverage does not count
        await using (var other = await startAccountsApp())
        {
            await other.Scenario(x => x.Get.Url("/public"));
        }

        // A host started after another was disposed still counts
        await using (var later = await startAccountsApp(coverage: coverage))
        {
            await later.Scenario(x =>
            {
                x.Get.Url("/conflict");
                x.StatusCodeShouldBe(409);
            });
        }

        coverage.NotExercised().ShouldBe(AllAccountEndpoints.Except(["GET /health", "GET /conflict"]));
    }

    [Fact]
    public async Task endpoints_allowing_anonymous_requests()
    {
        await using var host = await startAccountsApp();

        // AllowAnonymous wins over Authorize, as it does in the authorization middleware
        host.EndpointsAllowingAnonymous().ShouldBe([
            "DELETE /accounts/{id:int}",
            "GET /conflict",
            "GET /events",
            "GET /health",
            "GET /public"
        ]);
    }

    [Fact]
    public async Task a_fallback_policy_protects_endpoints_without_authorization()
    {
        await using var host = await startAccountsApp(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        host.EndpointsAllowingAnonymous().ShouldBe([
            "DELETE /accounts/{id:int}",
            "GET /events",
            "GET /health"
        ]);
    }

    [Fact]
    public async Task mvc_endpoints_are_described_by_their_route()
    {
        var coverage = new EndpointCoverage();
        await using var host = await AlbaHost.For<WebApp.Program>(coverage);

        var endpoints = coverage.NotExercised();

        endpoints.ShouldContain("POST /api/Files/upload");
        endpoints.ShouldAllBe(x => x.Contains(" /") || x.StartsWith('/'));

        await host.Scenario(x =>
        {
            x.Post.MultipartFormData(m => m.Field("AdditionalContent", "x").File("Files", "hi"u8.ToArray(), "a.txt"))
                .ToUrl("/api/files/upload");
        });

        coverage.NotExercised().ShouldNotContain("POST /api/Files/upload");
    }
}
