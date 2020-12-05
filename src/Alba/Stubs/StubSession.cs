using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Alba.Stubs
{
    public class StubSession : ISession
    {
        public Task LoadAsync()
        {
            throw new NotImplementedException();
        }

        public Task CommitAsync()
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            throw new NotImplementedException();
        }

        public void Set(string key, byte[] value)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public bool IsAvailable { get; }
        public string Id { get; }
        public IEnumerable<string> Keys { get; }
    }
}