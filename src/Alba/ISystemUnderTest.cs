using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Alba
{
    public interface ISystemUnderTest : IDisposable
    {
        IUrlLookup Urls { get; set; }

        [Obsolete]
        HttpContext CreateContext();


        // Might be smarter to keep a hold of the RequestDelegate
        IFeatureCollection Features { get; }
        IServiceProvider Services { get; }


        Task<HttpContext> Invoke(Action<HttpContext> setup);


        void BeforeEach(HttpContext context);
        void AfterEach(HttpContext context);

        T FromJson<T>(string json);
        string ToJson(object target);
    }
}