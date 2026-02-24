using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Alba;

internal interface IAlbaWebApplicationFactory : IDisposable, IAsyncDisposable
{
    public TestServer Server { get; }
    public IServiceProvider Services { get; }
    IHost? CreatedHost { get; }
}