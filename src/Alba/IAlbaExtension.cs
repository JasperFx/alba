using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
#nullable enable
namespace Alba
{
    public interface IAlbaExtension : IDisposable, IAsyncDisposable
    {
        Task Start(IAlbaHost host);
        IHostBuilder Configure(IHostBuilder builder);
    }
}