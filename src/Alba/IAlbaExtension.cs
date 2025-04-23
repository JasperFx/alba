using Microsoft.Extensions.Hosting;
 
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
    /// actions on an AlbaHOst
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    Task Start(IAlbaHost host);
        
    /// <summary>
    /// Allow an extension to alter the application's
    /// IHostBuilder prior to starting the application
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    IHostBuilder Configure(IHostBuilder builder);
}

#endregion