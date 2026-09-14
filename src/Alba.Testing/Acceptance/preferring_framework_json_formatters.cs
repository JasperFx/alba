using Alba.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using WebApp.Controllers;

namespace Alba.Testing.Acceptance;

public class preferring_framework_json_formatters
{
    // Mimics Microsoft.AspNetCore.OData's formatters (GH-116): registered at index 0,
    // advertising "application/json", refusing non-OData requests via CanRead/CanWriteResult,
    // and blowing up when driven directly the way Alba drives formatters
    private sealed class FakeODataInputFormatter : InputFormatter
    {
        public FakeODataInputFormatter()
        {
            SupportedMediaTypes.Add("application/json");
            SupportedMediaTypes.Add("application/json;odata.metadata=minimal");
        }

        public override bool CanRead(InputFormatterContext context) => false;

        public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
            => throw new UriFormatException("Invalid URI: simulated OData formatter failure");
    }

    private sealed class FakeODataOutputFormatter : OutputFormatter
    {
        public FakeODataOutputFormatter()
        {
            SupportedMediaTypes.Add("application/json");
            SupportedMediaTypes.Add("application/json;odata.metadata=minimal");
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context) => false;

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
            => throw new UriFormatException("Invalid URI: simulated OData formatter failure");
    }

    private static AlbaHostBuilder createHost() =>
        AlbaHost.For<WebApp.Program>(x =>
        {
            x.ConfigureServices(services => services.PostConfigure<MvcOptions>(o =>
            {
                o.InputFormatters.Insert(0, new FakeODataInputFormatter());
                o.OutputFormatters.Insert(0, new FakeODataOutputFormatter());
            }));
        });

    [Fact]
    public async Task json_round_trip_ignores_third_party_json_formatters_registered_first()
    {
        await using var host = await createHost();

        var posted = await host
            .PostJson(new OperationRequest { Type = OperationType.Multiply, One = 3, Two = 4 }, "/math")
            .Receive<OperationResult>();

        posted.Answer.ShouldBe(12);

        (await host.GetAsJson<OperationResult>("/math/add/3/4"))!.Answer.ShouldBe(7);
    }

    [Fact]
    public async Task mvc_strategy_still_selects_the_applications_newtonsoft_formatters()
    {
        // WebApp uses AddControllers().AddNewtonsoftJson(); the MVC strategy exists to
        // respect the application's configured serializer, so ranking must land on Newtonsoft
        await using var host = await createHost();

        var strategy = ((AlbaHost)host).MvcStrategy.ShouldBeOfType<FormatterSerializer>();
        strategy.InputFormatter.GetType().Name.ShouldBe("NewtonsoftJsonInputFormatter");
        strategy.OutputFormatter.GetType().Name.ShouldBe("NewtonsoftJsonOutputFormatter");
    }
}
