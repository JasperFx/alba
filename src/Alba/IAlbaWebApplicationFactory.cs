using System;
using Microsoft.AspNetCore.TestHost;

namespace Alba
{
    public interface IAlbaWebApplicationFactory : IDisposable, IAsyncDisposable
    {
        public TestServer Server { get; }
        public IServiceProvider Services { get; }
    }
}
