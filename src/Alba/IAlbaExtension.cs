using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Alba
{
    public interface IAlbaExtension
    {
        Task Start(IAlbaHost host);
        IHostBuilder Configure(IHostBuilder builder);
    }
}