using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Alba.StaticFiles;
using Baseline.Testing;
using Nowin;
using Shouldly;
using Xunit;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Alba.Testing.StaticFiles
{

    public class NowinHarness : IDisposable
    {
        private IDisposable _disposable;

        public NowinHarness(AppFunc appfunc)
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


    public class StaticFileMiddleware_end_to_end_Tests
    {
        [Fact]
        public void write_a_js_file_that_can_be_found()
        {
            new FileSystem().WriteStringToFile("static.js", "var y = 0;");

            var middleware = new StaticFileMiddleware(env => { throw new Exception("Should not get here"); }, new Alba.StaticFiles.StaticFiles(AppDomain.CurrentDomain.BaseDirectory), new AssetSettings());


            using (var server = new NowinHarness(middleware.Invoke))
            {
                var client = new WebClient();

                var path = $"http://localhost:{server.Port}/static.js";
                client.DownloadString(path).ShouldBe("var y = 0;");
            }
        }
    }

    public static class PortFinder
    {
        public static int FindPort(int start)
        {
            for (int i = start; i < start + 50; i++)
            {
                if (tryPort(i)) return i;
            }

            throw new InvalidOperationException("Could not find a port to bind to");
        }

        private static bool tryPort(int port)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            var endpoint = new IPEndPoint(IPAddress.Any, port);

            try
            {
                socket.Bind(endpoint);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                socket.SafeDispose();
            }

        }
    }
}