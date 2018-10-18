using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Alba
{
    public interface ISystemUnderTest : IDisposable
    {
        IUrlLookup Urls { get; set; }

        HttpContext CreateContext();


        // Might be smarter to keep a hold of the RequestDelegate
        IFeatureCollection Features { get; }
        IServiceProvider Services { get; }


        Task Invoke(HttpContext context);


        Task BeforeEach(HttpContext context);
        Task AfterEach(HttpContext context);

        T FromJson<T>(string json);
        string ToJson(object target);
    }
}