#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alba
{
    /// <inheritdoc cref="WebApplicationFactory{TEntryPoint}"/>
    public class AlbaWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IAlbaWebApplicationFactory where TEntryPoint : class
    {
        private readonly IAlbaExtension[] _extensions;
        public AlbaWebApplicationFactory(IAlbaExtension[] extensions)
        {
            _extensions = extensions;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddHttpContextAccessor();
            });

            base.ConfigureWebHost(builder);
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            foreach (var extension in _extensions)
            {
                extension.Configure(builder);
            }

            return base.CreateHost(builder);
        }

    }
}

#endif