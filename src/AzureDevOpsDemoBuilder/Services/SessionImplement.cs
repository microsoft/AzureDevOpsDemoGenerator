using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AzureDevOpsDemoBuilder.Services
{
    public class SessionImplement : ISession
    {
        public string Id => throw new NotImplementedException();

        public bool IsAvailable => throw new NotImplementedException();

        public IEnumerable<string> Keys => throw new NotImplementedException();

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            throw new NotImplementedException();
        }

        public void Set(string key, byte[] value)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            throw new NotImplementedException();
        }
    }

}
