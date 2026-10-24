using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Shouldly;
using WebApp.Controllers;

namespace Alba.Testing.Acceptance;

public class ProblemAppFixture : IAsyncLifetime
{
    public IAlbaHost Host = null!;

    public async ValueTask InitializeAsync()
    {
        Host = await AlbaHost.For(WebApplication.CreateBuilder(), app =>
        {
            app.MapPost("/withdrawals", () => Results.Problem(
                title: "Insufficient funds",
                detail: "The balance is 10.00",
                statusCode: 409,
                type: "https://example.com/problems/insufficient-funds",
                extensions: new Dictionary<string, object> { ["balance"] = 10.0m, ["limits"] = new[] { 5, 50 } }));

            app.MapPost("/deposits", () => Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Amount"] = ["Amount must be positive", "Amount must have two decimal places"],
                ["Currency"] = ["Currency is required"]
            }));

            app.MapGet("/plain", () => Results.Json(new { title = "not a problem" }, statusCode: 409));
        });
    }

    public async ValueTask DisposeAsync()
    {
        await Host.DisposeAsync();
    }
}

public class problem_details_assertions(ProblemAppFixture fixture) : IClassFixture<ProblemAppFixture>
{
    private readonly IAlbaHost _host = fixture.Host;

    #region sample_problem_details_assertions
    [Fact]
    public async Task problem_details_members()
    {
        await _host.Scenario(x =>
        {
            x.Post.Url("/withdrawals");
            x.ShouldReturnProblemDetails(409)
                .WithTitle("Insufficient funds")
                .WithDetail("The balance is 10.00")
                .WithType("https://example.com/problems/insufficient-funds")
                .WithExtension("balance", 10m)
                .WithExtension("limits", new[] { 5, 50 });
        });
    }

    [Fact]
    public async Task validation_errors_by_field()
    {
        await _host.Scenario(x =>
        {
            x.Post.Url("/deposits");
            x.ShouldReturnValidationProblem()
                .WithValidationError("Amount", "must be positive")
                .WithValidationError("amount", "two decimal places")
                .WithValidationError("Currency");
        });
    }
    #endregion

    [Fact]
    public async Task mvc_model_validation()
    {
        await using var host = await AlbaHost.For<WebApp.Program>();

        await host.Scenario(x =>
        {
            x.Post.Json(new { amount = -5 }).ToUrl("/api/deposits");
            x.ShouldReturnValidationProblem()
                .WithTitle("One or more validation errors occurred.")
                .WithValidationError(nameof(DepositRequest.Amount), "must be between 0.01 and 1000000")
                .WithValidationError(nameof(DepositRequest.Currency), "required");
        });
    }

    [Fact]
    public async Task failures_name_the_member_and_the_actual_value()
    {
        var ex = await Should.ThrowAsync<ScenarioAssertionException>(() => _host.Scenario(x =>
        {
            x.Post.Url("/withdrawals");
            x.ShouldReturnProblemDetails(409)
                .WithTitle("Overdrawn")
                .WithDetail("anything")
                .WithExtension("balance", 20m)
                .WithExtension("traceKey", "abc");
        }));

        ex.Messages.ShouldBe([
            "Expected the problem details title to be \"Overdrawn\", but it was \"Insufficient funds\"",
            "Expected the problem details detail to be \"anything\", but it was \"The balance is 10.00\"",
            "Expected the problem details member 'balance' to be 20, but it was 10.0",
            "Expected the problem details member 'traceKey' to be \"abc\", but it was not found"
        ]);
    }

    [Fact]
    public async Task validation_failures_list_the_fields_and_messages()
    {
        var ex = await Should.ThrowAsync<ScenarioAssertionException>(() => _host.Scenario(x =>
        {
            x.Post.Url("/deposits");
            x.ShouldReturnValidationProblem()
                .WithValidationError("Reference")
                .WithValidationError("Amount", "too large");
        }));

        ex.Messages.ShouldBe([
            "Expected a validation error for 'Reference', but the errors were for: Amount, Currency",
            "Expected a validation error for 'Amount' containing 'too large', but its messages were: 'Amount must be positive', 'Amount must have two decimal places'"
        ]);
    }

    [Fact]
    public async Task a_problem_without_errors_is_not_a_validation_problem()
    {
        var ex = await Should.ThrowAsync<ScenarioAssertionException>(() => _host.Scenario(x =>
        {
            x.Post.Url("/withdrawals");
            x.ShouldReturnValidationProblem(409);
        }));

        ex.Messages.ShouldBe(["Expected a validation problem response, but the problem details have no 'errors' member"]);
    }

    [Fact]
    public async Task plain_json_is_not_problem_details()
    {
        var ex = await Should.ThrowAsync<ScenarioAssertionException>(() => _host.Scenario(x =>
        {
            x.Get.Url("/plain");
            x.ShouldReturnProblemDetails(409);
        }));

        ex.Messages.Single().ShouldStartWith(
            "Expected a problem details response with content type 'application/problem+json', but the content type was 'application/json");
    }

    [Fact]
    public async Task the_status_code_is_checked()
    {
        var ex = await Should.ThrowAsync<ScenarioAssertionException>(() => _host.Scenario(x =>
        {
            x.Post.Url("/deposits");
            x.ShouldReturnValidationProblem(422);
        }));

        ex.Messages.First().ShouldContain("422");
    }
}
