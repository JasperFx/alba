using System.Net;
using System.Reflection;
using Baseline;

namespace Alba
{
    public class HttpRequestHeaders : HttpGeneralHeaders
    {
        public static readonly string Accept = "Accept";
        public static readonly string AcceptCharset = "Accept-Charset";
        public static readonly string AcceptEncoding = "Accept-Encoding";
        public static readonly string AcceptLanguage = "Accept-Language";
        public static readonly string Authorization = "Authorization";
        public static readonly string Cookie = "Cookie";
        public static readonly string ContentLength = "Content-Length";
        public static readonly string ContentMd5 = "Content-MD5";
        public static readonly string ContentType = "Content-Type";
        public static readonly string Expect = "Expect";
        public static readonly string From = "From";
        public static readonly string Host = "Host";
        public static readonly string IfMatch = "If-Match";
        public static readonly string IfModifiedSince = "If-Modified-Since";
        public static readonly string IfNoneMatch = "If-None-Match";
        public static readonly string IfRange = "If-Range";
        public static readonly string IfUnmodifiedSince = "If-Unmodified-Since";
        public static readonly string MaxForwards = "Max-Forwards";
        public static readonly string ProxyAuthorization = "Proxy-Authorization";
        public static readonly string Range = "Range";
        public static readonly string Referer = "Referer";
        public static readonly string Te = "TE";
        public static readonly string UserAgent = "User-Agent";


        // http 1.0
        public static readonly string KeepAlive = "keep-alive";


        // Not sure this is still used
        public static readonly string Translate = "Translate";


        private static readonly Cache<HttpRequestHeader, string> _headerNames = new Cache<HttpRequestHeader, string>();

        private static readonly Cache<string, string> _propertyAliases = new Cache<string, string>(name =>
        {
            var prop = typeof(HttpRequestHeaders).GetField(name, BindingFlags.Static | BindingFlags.Public);
            return prop == null ? name : (string)prop.GetValue(null);
        });

        static HttpRequestHeaders()
        {
            _headerNames[HttpRequestHeader.CacheControl] = CacheControl;
            _headerNames[HttpRequestHeader.Connection] = Connection;
            _headerNames[HttpRequestHeader.Date] = Date;
            _headerNames[HttpRequestHeader.KeepAlive] = KeepAlive;
            _headerNames[HttpRequestHeader.Pragma] = Pragma;
            _headerNames[HttpRequestHeader.Trailer] = Trailer;
            _headerNames[HttpRequestHeader.TransferEncoding] = TransferEncoding;
            _headerNames[HttpRequestHeader.Upgrade] = Upgrade;
            _headerNames[HttpRequestHeader.Via] = Via;
            _headerNames[HttpRequestHeader.Warning] = Warning;
            _headerNames[HttpRequestHeader.Allow] = Allow;
            _headerNames[HttpRequestHeader.ContentLength] = ContentLength;
            _headerNames[HttpRequestHeader.ContentType] = ContentType;
            _headerNames[HttpRequestHeader.ContentEncoding] = ContentEncoding;
            _headerNames[HttpRequestHeader.ContentLanguage] = ContentLanguage;
            _headerNames[HttpRequestHeader.ContentLocation] = ContentLocation;
            _headerNames[HttpRequestHeader.ContentMd5] = ContentMd5;
            _headerNames[HttpRequestHeader.ContentRange] = ContentRange;
            _headerNames[HttpRequestHeader.Expires] = Expires;
            _headerNames[HttpRequestHeader.LastModified] = LastModified;
            _headerNames[HttpRequestHeader.Accept] = Accept;
            _headerNames[HttpRequestHeader.AcceptCharset] = AcceptCharset;
            _headerNames[HttpRequestHeader.AcceptEncoding] = AcceptEncoding;
            _headerNames[HttpRequestHeader.AcceptLanguage] = AcceptLanguage;
            _headerNames[HttpRequestHeader.Authorization] = Authorization;
            _headerNames[HttpRequestHeader.Cookie] = Cookie;
            _headerNames[HttpRequestHeader.Expect] = Expect;
            _headerNames[HttpRequestHeader.From] = From;
            _headerNames[HttpRequestHeader.Host] = Host;
            _headerNames[HttpRequestHeader.IfMatch] = IfMatch;
            _headerNames[HttpRequestHeader.IfModifiedSince] = IfModifiedSince;
            _headerNames[HttpRequestHeader.IfNoneMatch] = IfNoneMatch;
            _headerNames[HttpRequestHeader.IfRange] = IfRange;
            _headerNames[HttpRequestHeader.IfUnmodifiedSince] = IfUnmodifiedSince;
            _headerNames[HttpRequestHeader.MaxForwards] = MaxForwards;
            _headerNames[HttpRequestHeader.ProxyAuthorization] = ProxyAuthorization;
            _headerNames[HttpRequestHeader.Referer] = Referer;
            _headerNames[HttpRequestHeader.Range] = Range;
            _headerNames[HttpRequestHeader.Te] = Te;
            _headerNames[HttpRequestHeader.Translate] = Translate;
            _headerNames[HttpRequestHeader.UserAgent] = UserAgent;
        }

        protected HttpRequestHeaders()
        {
        }

        public static string HeaderNameFor(HttpRequestHeader header)
        {
            return _headerNames[header];
        }

        public static string HeaderDictionaryNameForProperty(string propertyName)
        {
            return _propertyAliases[propertyName];
        }
    }
}