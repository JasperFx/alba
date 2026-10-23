using System.Text.Json;
using Alba;
using AspireSamples.Api;
using RabbitMQ.Client;
using Shouldly;

namespace AspireSamples;

#region sample_aspire_scenarios
[Collection(nameof(ApiCollection))]
public class deposit_specs(ApiFixture api)
{
    [Fact]
    public async Task deposits_add_up_to_the_balance()
    {
        // The database is shared by every test, so each test uses its own account
        var accountId = Guid.NewGuid();

        await api.Host.Scenario(x =>
        {
            x.Post.Json(new Deposit(accountId, 100m)).ToUrl("/deposits");
            x.StatusCodeShouldBe(201);
        });

        await api.Host.Scenario(x =>
        {
            x.Post.Json(new Deposit(accountId, 50m)).ToUrl("/deposits");
            x.StatusCodeShouldBe(201);
        });

        var balance = await api.Host.GetAsJson<Balance>($"/accounts/{accountId}/balance");
        balance!.Amount.ShouldBe(150m);
    }
}

[Collection(nameof(ApiCollection))]
public class deposit_messaging_specs(ApiFixture api, AspireInfrastructure infrastructure)
{
    [Fact]
    public async Task every_deposit_is_published_to_the_queue()
    {
        var deposit = new Deposit(Guid.NewGuid(), 25m);

        await api.Host.Scenario(x =>
        {
            x.Post.Json(deposit).ToUrl("/deposits");
            x.StatusCodeShouldBe(201);
        });

        // Read the broker directly with the connection string the API was given
        var factory = new ConnectionFactory { Uri = new Uri(infrastructure.MessagingConnectionString) };
        await using var connection = await factory.CreateConnectionAsync(TestContext.Current.CancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: TestContext.Current.CancellationToken);

        (await ReceiveAsync(channel, deposit.AccountId)).ShouldBe(deposit);
    }

    // Other tests publish to the same queue, so skip past their deposits
    private static async Task<Deposit?> ReceiveAsync(IChannel channel, Guid accountId)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (!timeout.IsCancellationRequested)
        {
            var message = await channel.BasicGetAsync(DepositPublisher.QueueName, autoAck: true, timeout.Token);
            if (message == null)
            {
                await Task.Delay(100, timeout.Token);
                continue;
            }

            var deposit = JsonSerializer.Deserialize<Deposit>(message.Body.Span);
            if (deposit?.AccountId == accountId) return deposit;
        }

        return null;
    }
}
#endregion
