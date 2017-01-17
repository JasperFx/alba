using System;
using System.Collections.Generic;
using System.Linq;
using Alba.StaticFiles;

namespace Alba
{
    public static class ContentNegotiationExtensions
    {
        public static DateTime? IfModifiedSince(this IDictionary<string, object> request)
        {
            return request.RequestHeaders().Get(HttpRequestHeaders.IfModifiedSince)
                .TryParseHttpDate();
        }

        public static DateTime? IfUnModifiedSince(this IDictionary<string, object> request)
        {
            return request.RequestHeaders().Get(HttpRequestHeaders.IfUnmodifiedSince)
                .TryParseHttpDate();
        }

        public static IEnumerable<string> IfMatch(this IDictionary<string, object> request)
        {
            return request.RequestHeaders().GetAll(HttpRequestHeaders.IfMatch).GetCommaSeparatedHeaderValues();
        }

        public static IEnumerable<string> IfNoneMatch(this IDictionary<string, object> request)
        {
            return request.RequestHeaders().GetAll(HttpRequestHeaders.IfNoneMatch).GetCommaSeparatedHeaderValues();
        }


        public static IDictionary<string, object> IfNoneMatch(this IDictionary<string, object> env, string etag)
        {
            env.RequestHeaders().Replace(HttpRequestHeaders.IfNoneMatch, etag);
            return env;
        }

        public static IDictionary<string, object> IfMatch(this IDictionary<string, object> env, string etag)
        {
            env.RequestHeaders().Replace(HttpRequestHeaders.IfMatch, etag);
            return env;
        }

        public static IDictionary<string, object> IfModifiedSince(this IDictionary<string, object> env, DateTime time)
        {
            env.RequestHeaders().Replace(HttpRequestHeaders.IfModifiedSince, time.ToUniversalTime().ToString("r"));
            return env;
        }

        public static IDictionary<string, object> IfUnModifiedSince(this IDictionary<string, object> env, DateTime time)
        {
            env.RequestHeaders().Replace(HttpRequestHeaders.IfUnmodifiedSince, time.ToUniversalTime().ToString("r"));
            return env;
        }

        public static EtagMatch EtagMatches(this IEnumerable<string> values, string etag)
        {
            if (values == null || !values.Any()) return EtagMatch.None;

            return values.Any(x => x.Equals(etag, StringComparison.Ordinal) || x == "*")
                ? EtagMatch.Yes
                : EtagMatch.No;

        }

        
        // THE FOLLOWING METHODS ARE TESTED THROUGH THE STATIC MIDDLEWARE 
        public static bool IfUnModifiedSinceHeaderAndModifiedSince(this IDictionary<string, object> request, IStaticFile file)
        {
            var ifUnModifiedSince = request.IfUnModifiedSince();
            return ifUnModifiedSince.HasValue && file.LastModified() > ifUnModifiedSince.Value;
        }

        public static bool IfModifiedSinceHeaderAndNotModified(this IDictionary<string, object> request, IStaticFile file)
        {
            var ifModifiedSince = request.IfModifiedSince();
            return ifModifiedSince.HasValue && file.LastModified().ToUniversalTime() <= ifModifiedSince.Value;
        }

        public static bool IfNoneMatchHeaderMatchesEtag(this IDictionary<string, object> request, IStaticFile file)
        {
            return request.IfNoneMatch().EtagMatches(file.Etag()) == EtagMatch.Yes;
        }

        public static bool IfMatchHeaderDoesNotMatchEtag(this IDictionary<string, object> request, IStaticFile file)
        {
            return request.IfMatch().EtagMatches(file.Etag()) == EtagMatch.No;
        }
        
    }
}