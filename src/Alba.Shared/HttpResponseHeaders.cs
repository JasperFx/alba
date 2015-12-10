using System.Net;
using Baseline;

namespace Alba
{
    public class HttpResponseHeaders : HttpGeneralHeaders
    {
        public static readonly string AcceptRanges = "Accept-Ranges";
        public static readonly string Age = "Age";
        public static readonly string ContentLength = "Content-Length";
        public static readonly string ContentMd5 = "Content-Md5";
        public static readonly string ContentType = "Content-Type";
        public static readonly string ETag = "ETag";
        public static readonly string KeepAlive = "KeepAlive";
        public static readonly string Location = "Location";
        public static readonly string ProxyAuthenticate = "Proxy-Authenticate";
        public static readonly string ProxyAuthorize = "Proxy-Authorize";
        public static readonly string RetryAfter = "Retry-After";
        public static readonly string Server = "Server";
        public static readonly string SetCookie = "Set-Cookie";
        public static readonly string Vary = "Vary";
        public static readonly string WwwAuthenticate = "WWW-Authenticate";

        public static readonly string ContentDisposition = "Content-Disposition";

        private static readonly Cache<HttpResponseHeader, string> _headerNames = new Cache<HttpResponseHeader, string>();

        static HttpResponseHeaders()
        {
            _headerNames[HttpResponseHeader.AcceptRanges] = AcceptRanges;
            _headerNames[HttpResponseHeader.Age] = Age;
            _headerNames[HttpResponseHeader.Allow] = Allow;
            _headerNames[HttpResponseHeader.CacheControl] = CacheControl;
            _headerNames[HttpResponseHeader.Connection] = Connection;
            _headerNames[HttpResponseHeader.ContentEncoding] = ContentEncoding;
            _headerNames[HttpResponseHeader.ContentLanguage] = ContentLanguage;
            _headerNames[HttpResponseHeader.ContentLength] = ContentLength;
            _headerNames[HttpResponseHeader.ContentLocation] = ContentLocation;
            _headerNames[HttpResponseHeader.ContentMd5] = ContentMd5;
            _headerNames[HttpResponseHeader.ContentRange] = ContentRange;
            _headerNames[HttpResponseHeader.ContentType] = ContentType;
            _headerNames[HttpResponseHeader.Date] = Date;
            _headerNames[HttpResponseHeader.ETag] = ETag;
            _headerNames[HttpResponseHeader.Expires] = Expires;
            _headerNames[HttpResponseHeader.KeepAlive] = KeepAlive;
            _headerNames[HttpResponseHeader.LastModified] = LastModified;
            _headerNames[HttpResponseHeader.Location] = Location;
            _headerNames[HttpResponseHeader.Pragma] = Pragma;
            _headerNames[HttpResponseHeader.ProxyAuthenticate] = ProxyAuthenticate;
            _headerNames[HttpResponseHeader.RetryAfter] = RetryAfter;
            _headerNames[HttpResponseHeader.Server] = Server;
            _headerNames[HttpResponseHeader.SetCookie] = SetCookie;
            _headerNames[HttpResponseHeader.Trailer] = Trailer;
            _headerNames[HttpResponseHeader.TransferEncoding] = TransferEncoding;
            _headerNames[HttpResponseHeader.Upgrade] = Upgrade;
            _headerNames[HttpResponseHeader.Vary] = Vary;
            _headerNames[HttpResponseHeader.Via] = Via;
            _headerNames[HttpResponseHeader.Warning] = Warning;
            _headerNames[HttpResponseHeader.WwwAuthenticate] = WwwAuthenticate;
        }

        protected HttpResponseHeaders()
        {
        }

        public static string HeaderNameFor(HttpResponseHeader header)
        {
            return _headerNames[header];
        }
    }
}