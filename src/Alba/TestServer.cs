// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Context = Microsoft.AspNetCore.Hosting.Internal.HostingApplication.Context;

namespace Alba
{
    public class TestServer : IServer
    {
        private const string DefaultEnvironmentName = "Development";
        private const string ServerName = nameof(TestServer);
        private bool _disposed = false;
        private IHttpApplication<Context> _application;

        public Uri BaseAddress { get; set; } = new Uri("http://localhost/");


        IFeatureCollection IServer.Features { get; } = new FeatureCollection();

        public void Dispose()
        {

        }

        void IServer.Start<TContext>(IHttpApplication<TContext> application)
        {
            _application = new ApplicationWrapper<Context>((IHttpApplication<Context>)application, () =>
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
            });
        }

        private class ApplicationWrapper<TContext> : IHttpApplication<TContext>
        {
            private readonly IHttpApplication<TContext> _application;
            private readonly Action _preProcessRequestAsync;

            public ApplicationWrapper(IHttpApplication<TContext> application, Action preProcessRequestAsync)
            {
                _application = application;
                _preProcessRequestAsync = preProcessRequestAsync;
            }

            public TContext CreateContext(IFeatureCollection contextFeatures)
            {
                return _application.CreateContext(contextFeatures);
            }

            public void DisposeContext(TContext context, Exception exception)
            {
                _application.DisposeContext(context, exception);
            }

            public Task ProcessRequestAsync(TContext context)
            {
                _preProcessRequestAsync();
                return _application.ProcessRequestAsync(context);
            }
        }
    }
}