using Microsoft.Extensions.Configuration;

namespace Alba;

/// <summary>
/// Used to override configuration values for tests. Required due to https://github.com/dotnet/aspnetcore/issues/37680
/// </summary>
public sealed class ConfigurationOverride : IAlbaExtension
{
    private readonly IEnumerable<KeyValuePair<string, string?>> _configDictionary;

    internal ConfigurationOverride(IEnumerable<KeyValuePair<string, string?>> configDictionary)
    {
        _configDictionary = configDictionary;
    }

    public static ConfigurationOverride Create(IEnumerable<KeyValuePair<string, string?>> configDictionary)
    {
        return new ConfigurationOverride(configDictionary);
    }

    public void Dispose()
    {
       
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public Task Start(IAlbaHost host)
    {
        return Task.CompletedTask;
    }

    public void Configure(IAlbaHostBuilder builder)
    {
        builder.ConfigureConfiguration(config =>
        {
            config.AddInMemoryCollection(_configDictionary);
        });
    }
}
