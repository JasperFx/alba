using Microsoft.AspNetCore.TestHost;

namespace Alba;

internal interface IAlbaWebApplicationFactory : IDisposable, IAsyncDisposable
{
    public TestServer Server { get; }
    public IServiceProvider Services { get; }
}