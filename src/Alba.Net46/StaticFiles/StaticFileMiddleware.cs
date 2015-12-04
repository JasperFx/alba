using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alba.StaticFiles
{
    public class StaticFileMiddleware
    {
        private readonly IStaticFiles _files;
        private readonly AssetSettings _settings;

        public StaticFileMiddleware(Func<IDictionary<string, object>, Task> inner, IStaticFiles files,
            AssetSettings settings)
        {
            _files = files;
            _settings = settings;
        }

        public MiddlewareContinuation Invoke(IDictionary<string, object> env)
        {
            if (env.IsNotHttpMethod("GET", "HEAD")) return MiddlewareContinuation.Continue();

            var relativeUrl = env.RelativeUrlWithoutQueryString();

            // Gets around what I *think* is a Katana bug 
            if (relativeUrl.StartsWith("http:/")) return MiddlewareContinuation.Continue();

            var file = _files.Find(relativeUrl);
            if (file == null) return MiddlewareContinuation.Continue();

            if (env.IsHead())
            {
                return new WriteFileHeadContinuation(env, file, 200);
            }

            if (env.IfMatchHeaderDoesNotMatchEtag(file))
            {
                return new WriteStatusCodeContinuation(env, 412, "If-Match test failed");
            }

            if (env.IfNoneMatchHeaderMatchesEtag(file))
            {
                return new WriteFileHeadContinuation(env, file, 304);
            }

            if (env.IfModifiedSinceHeaderAndNotModified(file))
            {
                return new WriteFileHeadContinuation(env, file, 304);
            }

            if (env.IfUnModifiedSinceHeaderAndModifiedSince(file))
            {
                return new WriteStatusCodeContinuation(env, 412, "File has been modified");
            }

            // Write headers here.

            return new WriteFileContinuation(env, file, _settings);
        }
    }
}