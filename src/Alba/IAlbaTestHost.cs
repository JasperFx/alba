using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;

namespace Alba
{
    public interface IAlbaTestHost : IHost
    {
        IUrlLookup Urls { get; set; }

        /// <summary>
        /// Deserializes an object using the ASP.Net Core JsonSerializerSettings
        /// for this application
        /// </summary>
        /// <param name="json"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T FromJson<T>(string json);
        
        /// <summary>
        /// Serializes an object using the ASP.Net Core JsonSerializerSettings
        /// for this application
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        string ToJson(object target);

        /// <summary>
        ///     Define and execute an integration test by running an Http request through
        ///     your ASP.Net Core system
        /// </summary>
        /// <param name="system"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        Task<IScenarioResult> Scenario(
                Action<Scenario> configure)
            // ENDSAMPLE
            ;

        /// <summary>
        /// Execute some kind of action before each scenario. This is NOT additive
        /// </summary>
        /// <param name="beforeEach"></param>
        /// <returns></returns>
        SystemUnderTest BeforeEach(Action<HttpContext> beforeEach);

        /// <summary>
        /// Execute some clean up action immediately after executing each HTTP execution. This is NOT additive
        /// </summary>
        /// <param name="afterEach"></param>
        /// <returns></returns>
        SystemUnderTest AfterEach(Action<HttpContext?> afterEach);

        /// <summary>
        /// Run some kind of set up action immediately before executing an HTTP request
        /// </summary>
        /// <param name="beforeEach"></param>
        /// <returns></returns>
        SystemUnderTest BeforeEachAsync(Func<HttpContext, Task> beforeEach);

        /// <summary>
        /// Execute some clean up action immediately after executing each HTTP execution. This is NOT additive
        /// </summary>
        /// <param name="afterEach"></param>
        /// <returns></returns>
        SystemUnderTest AfterEachAsync(Func<HttpContext?, Task> afterEach);
    }
}