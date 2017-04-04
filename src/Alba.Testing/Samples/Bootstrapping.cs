using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Alba.Testing.Samples
{
    public class Bootstrapping
    {
        public void DoStuff()
        {
            // SAMPLE: SimplisticSystemUnderTest
            var system = SystemUnderTest.ForStartup<Startup>();
            // ENDSAMPLE

            // SAMPLE: configuring-IHostingEnvironment
            system.Environment.ApplicationName = "My Application";
            system.Environment.ContentRootPath = "c:\\somewhere\\else";
            // ENDSAMPLE


            // SAMPLE: configuration-overrides
            system.Configure(builder =>
            {
                builder
                    .UseKestrel()
                    .UseUrls("http://localhost:5025");
            });
            // ENDSAMPLE

            // SAMPLE: configuration-overriding-services
            system.ConfigureServices(services =>
            {
                services.AddTransient<IExternalService, StubbedExternalService>();
            });
            // ENDSAMPLE
        }


    }

    public interface IExternalService{}
    public class StubbedExternalService : IExternalService{}
}