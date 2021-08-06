using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Alba
{
    public interface IScenarioRunner : IDisposable
    {
        IUrlLookup Urls { get; set; }


        IServiceProvider Services { get; }


        Task<HttpContext> Invoke(Action<HttpContext> setup);


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
    }
}