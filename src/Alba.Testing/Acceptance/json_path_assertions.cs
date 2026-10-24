using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Alba.Testing.Acceptance;

public enum OrderStatus
{
    Placed,
    Shipped
}

public class JsonOrderAppFixture : IAsyncLifetime
{
    public IAlbaHost Host = null!;

    public async ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        Host = await AlbaHost.For(builder, app =>
        {
            app.MapGet("/order", () => new
            {
                id = 7,
                status = OrderStatus.Shipped,
                customer = new { name = "Alba", email = (string)null },
                lines = new[]
                {
                    new { sku = "A-1", quantity = 2 },
                    new { sku = "B-2", quantity = 1 }
                },
                total = 150m
            });

            app.MapGet("/raw", () => Results.Text("""{"total":150.0,"odd name":"yes"}""", "application/json"));
            app.MapGet("/text", () => "not json");
        });
    }

    public async ValueTask DisposeAsync()
    {
        await Host.DisposeAsync();
    }
}

public class json_path_assertions(JsonOrderAppFixture fixture) : IClassFixture<JsonOrderAppFixture>
{
    private readonly IAlbaHost _host = fixture.Host;

    #region sample_json_path_assertions
    [Fact]
    public async Task values_at_paths()
    {
        await _host.Scenario(x =>
        {
            x.Get.Url("/order");
            x.JsonPath("$.id").ShouldBe(7);
            x.JsonPath("$.customer.name").ShouldBe("Alba");
            x.JsonPath("$.lines[1].sku").ShouldBe("B-2");
            x.JsonPath("$['customer']['name']").ShouldBe("Alba");

            // The leading "$." is optional
            x.JsonPath("lines[0].quantity").ShouldBe(2);
        });
    }
    #endregion

    #region sample_json_path_structural_comparison
    [Fact]
    public async Task objects_compare_structurally_regardless_of_property_order()
    {
        await _host.Scenario(x =>
        {
            x.Get.Url("/order");
            x.JsonPath("$.lines[0]").ShouldBe(new { quantity = 2, sku = "A-1" });
            x.JsonPath("$.customer").ShouldBe(new { email = (string)null, name = "Alba" });
        });
    }
    #endregion

    [Fact]
    public async Task the_whole_body_at_the_root()
    {
        await _host.Scenario(x =>
        {
            x.Get.Url("/order");
            x.JsonPath("$").ShouldBe(new
            {
                total = 150m,
                lines = new[]
                {
                    new { sku = "A-1", quantity = 2 },
                    new { sku = "B-2", quantity = 1 }
                },
                customer = new { name = "Alba", email = (string)null },
                status = OrderStatus.Shipped,
                id = 7
            });
        });
    }

    [Fact]
    public async Task expected_values_use_the_application_json_settings()
    {
        // The application writes enums as strings, so the expected enum must be serialized the same way
        await _host.Scenario(x =>
        {
            x.Get.Url("/order");
            x.JsonPath("$.status").ShouldBe(OrderStatus.Shipped);
            x.JsonPath("$.status").ShouldBe("Shipped");
        });
    }

    [Fact]
    public async Task numbers_compare_by_value()
    {
        await _host.Scenario(x =>
        {
            x.Get.Url("/raw");
            x.JsonPath("$.total").ShouldBe(150);
            x.JsonPath("$['odd name']").ShouldBe("yes");
        });
    }

    #region sample_json_path_existence_and_counts
    [Fact]
    public async Task existence_and_counts()
    {
        await _host.Scenario(x =>
        {
            x.Get.Url("/order");
            x.JsonPath("$.customer.email").ShouldExist().ShouldBe<string>(null);
            x.JsonPath("$.customer.phone").ShouldNotExist();
            x.JsonPath("$.lines[2]").ShouldNotExist();
            x.JsonPath("$.lines").ShouldHaveCount(2);
        });
    }
    #endregion

    [Fact]
    public async Task every_failure_is_reported_with_the_body()
    {
        var ex = await Should.ThrowAsync<ScenarioAssertionException>(() => _host.Scenario(x =>
        {
            x.Get.Url("/order");
            x.JsonPath("$.customer.name").ShouldBe("Wolverine");
            x.JsonPath("$.lines[5].sku").ShouldBe("C-3");
            x.JsonPath("$.lines").ShouldHaveCount(3);
            x.JsonPath("$.customer").ShouldHaveCount(1);
            x.JsonPath("$.id").ShouldNotExist();
            x.JsonPath("$.missing").ShouldExist();
        }));

        ex.Messages.ShouldBe([
            "Expected JSON at '$.customer.name' to be \"Wolverine\", but it was \"Alba\"",
            "Expected JSON at '$.lines[5].sku' to be \"C-3\", but it was not found",
            "Expected an array of 3 items at '$.lines', but it had 2",
            "Expected an array of 1 items at '$.customer', but it was {\"name\":\"Alba\",\"email\":null}",
            "Expected no JSON at '$.id', but it was 7",
            "Expected JSON at '$.missing', but the path was not found"
        ]);

        ex.Message.ShouldContain("Actual body text was:");
    }

    [Fact]
    public async Task a_body_that_is_not_json()
    {
        var ex = await Should.ThrowAsync<ScenarioAssertionException>(() => _host.Scenario(x =>
        {
            x.Get.Url("/text");
            x.JsonPath("$.id").ShouldExist();
        }));

        ex.Messages.ShouldBe(["Expected JSON at '$.id', but the response body is not valid JSON"]);
    }

    [Theory]
    [InlineData("$.lines[?(@.sku)]")]
    [InlineData("$..sku")]
    [InlineData("$.lines[")]
    [InlineData("$.lines[-1]")]
    [InlineData(" ")]
    public async Task unsupported_paths_fail_before_the_request(string path)
    {
        await Should.ThrowAsync<ArgumentException>(() => _host.Scenario(x =>
        {
            x.Get.Url("/order");
            x.JsonPath(path);
        }));
    }
}
