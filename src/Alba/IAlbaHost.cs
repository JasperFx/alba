using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
 
namespace Alba;

public interface IAlbaHost : IHost, IAsyncDisposable
{
    /// <summary>
    ///     Define and execute an integration test by running an Http request through
    ///     your ASP.Net Core system
    /// </summary>
    /// <param name="configure"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    Task<IScenarioResult> Scenario(Action<Scenario> configure);

    /// <summary>
    /// Execute some kind of action before each scenario. This is NOT additive
    /// </summary>
    /// <param name="beforeEach"></param>
    /// <returns></returns>
    IAlbaHost BeforeEach(Action<HttpContext> beforeEach);

    /// <summary>
    /// Execute some clean up action immediately after executing each HTTP execution. This is NOT additive
    /// </summary>
    /// <param name="afterEach"></param>
    /// <returns></returns>
    IAlbaHost AfterEach(Action<HttpContext?> afterEach);

    /// <summary>
    /// Run some kind of set up action immediately before executing an HTTP request
    /// </summary>
    /// <param name="beforeEach"></param>
    /// <returns></returns>
    IAlbaHost BeforeEachAsync(Func<HttpContext, Task> beforeEach);

    /// <summary>
    /// Execute some clean up action immediately after executing each HTTP execution. This is NOT additive
    /// </summary>
    /// <param name="afterEach"></param>
    /// <returns></returns>
    IAlbaHost AfterEachAsync(Func<HttpContext?, Task> afterEach);

    /// <summary>
    ///     The underlying TestServer for additional functionality
    /// </summary>
    TestServer Server { get; }

        
}