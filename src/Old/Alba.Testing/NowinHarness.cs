using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alba.Testing.StaticFiles;
using Nowin;

namespace Alba.Testing
{
    public class NowinHarness : IDisposable
    {
        private IDisposable _disposable;

        public NowinHarness(Func<IDictionary<string, object>, Task> appfunc)
        {
            Port = PortFinder.FindPort(5500);
            var list = new List<IDictionary<string, object>>() { new Dictionary<string, object>() };
            list[0].Add("port", Port.ToString());

            var properties = new Dictionary<string, object>();
            properties.Add("host.Addresses", list);


            OwinServerFactory.Initialize(properties);

            _disposable = OwinServerFactory.Create(appfunc, properties);            
        }

        

        public int Port { get; }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}