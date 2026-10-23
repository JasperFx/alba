using System.Text.Json;
using Npgsql;
using RabbitMQ.Client;

namespace AspireSamples.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton(sp => NpgsqlDataSource.Create(
            sp.GetRequiredService<IConfiguration>().GetConnectionString("ledger")
            ?? throw new InvalidOperationException("The 'ledger' connection string is missing")));
        builder.Services.AddSingleton<DepositPublisher>();
        builder.Services.AddHostedService<LedgerSchema>();

        var app = builder.Build();

        app.MapPost("/deposits", async (Deposit deposit, NpgsqlDataSource db, DepositPublisher publisher) =>
        {
            await using var command = db.CreateCommand("insert into deposits (account_id, amount) values ($1, $2)");
            command.Parameters.AddWithValue(deposit.AccountId);
            command.Parameters.AddWithValue(deposit.Amount);
            await command.ExecuteNonQueryAsync();

            await publisher.PublishAsync(deposit);

            return Results.Created($"/accounts/{deposit.AccountId}/balance", deposit);
        });

        app.MapGet("/accounts/{accountId:guid}/balance", async (Guid accountId, NpgsqlDataSource db) =>
        {
            await using var command = db.CreateCommand("select coalesce(sum(amount), 0) from deposits where account_id = $1");
            command.Parameters.AddWithValue(accountId);

            return new Balance(accountId, (decimal)(await command.ExecuteScalarAsync())!);
        });

        app.Run();
    }
}

public record Deposit(Guid AccountId, decimal Amount);

public record Balance(Guid AccountId, decimal Amount);

/// <summary>
/// Creates the deposits table when the application starts
/// </summary>
public class LedgerSchema(NpgsqlDataSource db) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var command = db.CreateCommand(
            "create table if not exists deposits (id bigserial primary key, account_id uuid not null, amount numeric not null)");
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

/// <summary>
/// Publishes every deposit to the deposits queue
/// </summary>
public class DepositPublisher(IConfiguration configuration) : IAsyncDisposable
{
    public const string QueueName = "deposits";

    private readonly SemaphoreSlim _lock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;

    public async Task PublishAsync(Deposit deposit)
    {
        await _lock.WaitAsync();
        try
        {
            if (_channel == null)
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(configuration.GetConnectionString("messaging")
                                  ?? throw new InvalidOperationException("The 'messaging' connection string is missing"))
                };

                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();
                await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false);
            }

            await _channel.BasicPublishAsync("", QueueName, JsonSerializer.SerializeToUtf8Bytes(deposit));
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null) await _channel.DisposeAsync();
        if (_connection != null) await _connection.DisposeAsync();
    }
}
