using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alba
{
    internal class AlbaServiceProvider : IServiceProvider, ISupportRequiredService
    {
        private readonly Stack<IServiceProvider> _serviceProviders = new Stack<IServiceProvider>();

        public AlbaServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProviders.Push(serviceProvider);
        }

        public object GetRequiredService(Type serviceType)
        {
            object service = null;
            foreach (var serviceProvider in _serviceProviders)
            {
                try
                {
                    service = serviceProvider.GetRequiredService(serviceType);
                    break;
                }
                catch (InvalidOperationException)
                {
                }
            }
            if (service == null)
            {
                throw new InvalidOperationException($"Could not resolve required service {serviceType.Name}");
            }
            return service;
        }

        public object GetService(Type serviceType)
        {
            object service = null;
            foreach (var serviceProvider in _serviceProviders)
            {
                try
                {
                    service = serviceProvider.GetService(serviceType);
                    break;
                }
                catch (InvalidOperationException)
                {
                }
            }
            if (service == null)
            {
                throw new InvalidOperationException($"Coiuld not resolve service {serviceType.Name}");
            }
            return service;
        }

        public void AddServices(IServiceCollection services)
        {
            _serviceProviders.Push(services.BuildServiceProvider());
        }

        public void Teardown()
        {
            while (_serviceProviders.Count >= 2)
            {
                _serviceProviders.Pop();
            }
        }
    }
}
