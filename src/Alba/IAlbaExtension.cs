using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alba;

#region sample_IAlbaExtension

/// <summary>
/// Models an extension to an AlbaHost
/// </summary>
public interface IAlbaExtension : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Called during the initialization of an AlbaHost after the application is started,
    /// so the application DI container is available. Useful for registering setup or teardown
    /// actions on an AlbaHost
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    Task Start(IAlbaHost host);

    /// <summary>
    /// Allow an extension to alter the application under test before it starts.
    /// Behaves identically for every AlbaHost bootstrapping style.
    /// </summary>
    /// <param name="builder"></param>
    void Configure(IAlbaHostBuilder builder)
    {
    }
}

/// <summary>
/// Hosting-model-agnostic configuration surface handed to Alba extensions
/// before the application under test starts
/// </summary>
public interface IAlbaHostBuilder
{
    /// <summary>
    /// Add or replace services in the application under test
    /// </summary>
    void ConfigureServices(Action<IServiceCollection> configure);

    /// <summary>
    /// Add configuration sources for the application under test. Sources added
    /// here take precedence over the application's own configuration.
    /// </summary>
    void ConfigureConfiguration(Action<IConfigurationBuilder> configure);
}

#endregion
