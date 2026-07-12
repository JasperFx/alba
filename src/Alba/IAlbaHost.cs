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
    ///     Open a live server-sent event stream against the application without
    ///     buffering the response. All BeforeEach/BeforeEachAsync actions apply as
    ///     they do for scenarios. Response assertions other than the expected status
    ///     code are not supported; assert on the streamed events instead. Disposing
    ///     the result aborts the request on the server.
    /// </summary>
    /// <param name="configure"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    Task<SseStreamResult> StreamServerSentEvents(Action<Scenario> configure,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a synchronous action against the HttpContext immediately before each
    /// scenario's HTTP request executes. This is additive for each call made.
    /// </summary>
    /// <param name="beforeEach"></param>
    /// <returns></returns>
    IAlbaHost BeforeEach(Action<HttpContext> beforeEach);

    /// <summary>
    /// Execute some clean up action immediately after executing each HTTP execution. This is additive for each call made.
    /// </summary>
    /// <param name="afterEach"></param>
    /// <returns></returns>
    IAlbaHost AfterEach(Action<HttpContext?> afterEach);

    /// <summary>
    /// Run an asynchronous set up action against the Scenario before its HTTP request
    /// executes. All asynchronous actions run before any synchronous BeforeEach action.
    /// To modify the outgoing HttpContext, register a callback through
    /// <see cref="Alba.Scenario.ConfigureHttpContext"/>. This is additive for each call made.
    /// </summary>
    /// <param name="beforeEach"></param>
    /// <returns></returns>
    IAlbaHost BeforeEachAsync(Func<Scenario, Task> beforeEach);

    /// <summary>
    /// Execute some clean up action immediately after executing each HTTP execution. This is additive for each call made.
    /// </summary>
    /// <param name="afterEach"></param>
    /// <returns></returns>
    IAlbaHost AfterEachAsync(Func<HttpContext?, Task> afterEach);

    /// <summary>
    ///     The underlying TestServer for additional functionality
    /// </summary>
    TestServer Server { get; }

        
}