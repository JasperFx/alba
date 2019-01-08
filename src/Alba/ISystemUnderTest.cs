using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Alba
{
    public interface ISystemUnderTest : IDisposable
    {
        IUrlLookup Urls { get; set; }


        IServiceProvider Services { get; }


        Task<HttpContext> Invoke(Action<HttpContext> setup);


        Task BeforeEach(HttpContext context);
        Task AfterEach(HttpContext context);

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
    }
}