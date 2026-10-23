using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using JasperFx;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Http.Json;

namespace MinimalApiWithOakton
{
    public class Program
    {
        public static Task<int> Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseLamar();

            // Read before Build so tests can prove overridden configuration reaches startup code
            var greeting = builder.Configuration["Alba:Greeting"] ?? "Hello from appsettings";

            // Configure JSON options.
            builder.Services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.IncludeFields = true;
            });

            builder.Services.AddSingleton<SseAbortTracker>();
            builder.Services.AddSingleton(TimeProvider.System);

            var app = builder.Build();
            app.MapGet("/", () => "Hello World!");
            app.MapGet("/args", () => Results.Ok(args));
            app.MapGet("/greeting", () => greeting);
            app.MapPost("/go", (PostedMessage input) => new OutputMessage(input.Id));

            app.MapGet("/time", (TimeProvider timeProvider) => new CurrentTime(timeProvider.GetUtcNow()));

            app.MapMethods("/api/query", ["QUERY"], async (HttpContext context) =>
            {
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync(context.RequestAborted);
                return $"I ran a QUERY with value {body}";
            });

            app.MapMethods("/api/options", ["OPTIONS"], (HttpContext context) =>
            {
                context.Response.Headers.Allow = "GET, POST, OPTIONS";
                return Results.NoContent();
            });

            app.MapGet("/sse/finite", () => TypedResults.ServerSentEvents(
                Enumerable.Range(1, 3)
                    .Select(i => new SseItem<Counter>(new Counter { Number = i }, "count") { EventId = i.ToString() })
                    .ToAsyncEnumerable()))
                .WithName("FiniteCounters");

            app.MapGet("/sse/infinite", (SseAbortTracker tracker) =>
                TypedResults.ServerSentEvents(InfiniteCounters(tracker)));

            app.MapGet("/sse/echo-header", (HttpContext context) => TypedResults.ServerSentEvents(
                new[] { context.Request.Headers["x-sse-echo"].ToString() }.ToAsyncEnumerable()));

            app.MapGet("/api/accounts/{accountId}", (string accountId, HttpContext context) =>
                    new AccountSummary(accountId, context.Request.QueryString.Value ?? ""))
                .WithName("GetAccount");

            // Wolverine's RouteName only adds RouteNameMetadata, with no endpoint name
            app.MapPost("/api/accounts/{accountId:int}/deposits", (int accountId, MakeDeposit deposit) =>
                    new DepositMade(accountId, deposit.Amount))
                .WithMetadata(new RouteNameMetadata("MakeDeposit"));

            return app.RunJasperFxCommands(args);
        }

        private static async IAsyncEnumerable<SseItem<Counter>> InfiniteCounters(SseAbortTracker tracker,
            [EnumeratorCancellation] CancellationToken cancellation = default)
        {
            try
            {
                var number = 0;
                while (true)
                {
                    number++;
                    yield return new SseItem<Counter>(new Counter { Number = number });
                    await Task.Delay(25, cancellation);
                }
            }
            finally
            {
                tracker.MarkAborted();
            }
        }
    }

    public record PostedMessage(Guid Id);
    public record OutputMessage(Guid Id);
    public record CurrentTime(DateTimeOffset UtcNow);
    public record AccountSummary(string AccountId, string Query);
    public record MakeDeposit(decimal Amount);
    public record DepositMade(int AccountId, decimal Amount);

    public class Counter
    {
        public int Number;
    }

    /// <summary>
    /// Lets tests observe that an SSE request was aborted server-side
    /// </summary>
    public class SseAbortTracker
    {
        private TaskCompletionSource _aborted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task Aborted => _aborted.Task;

        public void MarkAborted() => _aborted.TrySetResult();

        public void Reset() => _aborted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}

