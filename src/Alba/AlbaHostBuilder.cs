using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alba;

/// <summary>
/// Fluent, awaitable configuration of the Alba specific parts of a host: configuration
/// overrides and extensions. They are recorded in call order and applied when the
/// application starts, which happens when the builder is awaited
/// </summary>
public sealed class AlbaHostBuilder
{
    private readonly IAlbaBootstrapper _bootstrapper;
    private readonly List<Action<IAlbaHostBuilder>> _steps = new();
    private readonly List<IAlbaExtension> _extensions = new();
    private Task<IAlbaHost>? _started;

    internal AlbaHostBuilder(IAlbaBootstrapper bootstrapper)
    {
        _bootstrapper = bootstrapper;
    }

    /// <summary>
    /// Override a single configuration value in the application under test. Overrides
    /// take precedence over the application's own configuration files
    /// </summary>
    public AlbaHostBuilder WithConfiguration(string key, string? value)
    {
        ArgumentNullException.ThrowIfNull(key);
        return WithConfiguration([new KeyValuePair<string, string?>(key, value)]);
    }

    /// <summary>
    /// Override configuration values in the application under test. Overrides take
    /// precedence over the application's own configuration files
    /// </summary>
    public AlbaHostBuilder WithConfiguration(IEnumerable<KeyValuePair<string, string?>> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return record(b => b.ConfigureConfiguration(c => c.AddInMemoryCollection(values)));
    }

    /// <summary>
    /// Apply an Alba extension to the application under test
    /// </summary>
    public AlbaHostBuilder WithExtension(IAlbaExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        record(extension.Configure);
        _extensions.Add(extension);
        return this;
    }

    /// <summary>
    /// Apply Alba extensions to the application under test
    /// </summary>
    public AlbaHostBuilder WithExtensions(params IAlbaExtension[] extensions)
    {
        foreach (var extension in extensions) WithExtension(extension);
        return this;
    }

    /// <summary>
    /// Run each scenario's request in the test's async context, so <see cref="AsyncLocal{T}"/> values
    /// such as the test framework's current test, <see cref="System.Diagnostics.Activity.Current"/> and
    /// the current culture are visible to the application while it handles the request. Output the
    /// application writes during the request is then attributed to the test that sent it
    /// </summary>
    public AlbaHostBuilder WithTestContextFlow()
    {
        return record(b => b.ConfigureServices(s => s.Configure<TestServerOptions>(o => o.PreserveExecutionContext = true)));
    }

    /// <summary>
    /// Start the application. Awaiting the builder does the same thing, and
    /// every await returns the same running host
    /// </summary>
    public Task<IAlbaHost> StartAsync()
    {
        return _started ??= startDetached();
    }

    // The application starts without the caller's async context, so hosted services and other
    // background work don't keep the test that happened to start the host (its test context,
    // current Activity or culture) for the rest of the test run
    private Task<IAlbaHost> startDetached()
    {
        var extensions = _extensions.ToArray();

        using (ExecutionContext.SuppressFlow())
        {
            return Task.Run(() => _bootstrapper.StartAsync(_steps, extensions));
        }
    }

    public TaskAwaiter<IAlbaHost> GetAwaiter()
    {
        return StartAsync().GetAwaiter();
    }

    private AlbaHostBuilder record(Action<IAlbaHostBuilder> step)
    {
        if (_started is not null)
        {
            throw new InvalidOperationException(
                "The AlbaHost has already been started and can no longer be configured");
        }

        _steps.Add(step);
        return this;
    }
}

/// <summary>
/// Starts an application for one bootstrapping style, applying the recorded
/// configuration steps to that style's IAlbaHostBuilder
/// </summary>
internal interface IAlbaBootstrapper
{
    Task<IAlbaHost> StartAsync(IReadOnlyList<Action<IAlbaHostBuilder>> steps, IAlbaExtension[] extensions);
}
