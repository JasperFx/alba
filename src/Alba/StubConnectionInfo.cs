using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Alba
{
    public class StubConnectionInfo : ConnectionInfo
    {
        public override Task<X509Certificate2> GetClientCertificateAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public override IPAddress RemoteIpAddress { get; set; }
        public override int RemotePort { get; set; }
        public override IPAddress LocalIpAddress { get; set; }
        public override int LocalPort { get; set; }
        public override X509Certificate2 ClientCertificate { get; set; }
    }
}