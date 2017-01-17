using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;


namespace Alba.StaticFiles
{
    public class StaticFileMiddleware
    {
        private readonly Func<IDictionary<string, object>, Task> _inner;
        private readonly IStaticFiles _files;
        private readonly AssetSettings _settings;


        public static Func<AppFunc, AppFunc> ToMidFunc(IStaticFiles files = null, AssetSettings settings = null)
        {
            files = files ?? new StaticFiles(Environment.CurrentDirectory);
            settings = settings ?? new AssetSettings();

            return inner =>
            {
                var middleware = new StaticFileMiddleware(inner, files, settings);
                return env => middleware.Invoke(env);
            };
        } 

        public StaticFileMiddleware(AppFunc inner, IStaticFiles files,
            AssetSettings settings)
        {
            _inner = inner;
            _files = files;
            _settings = settings;
        }
        
        public Task Invoke(IDictionary<string, object> environment)
        {
            return DetermineContinuation(environment).ToTask(environment, _inner);
        }
        

        public MiddlewareContinuation DetermineContinuation(IDictionary<string, object> env)
        {
            if (env.IsNotHttpMethod("GET", "HEAD")) return MiddlewareContinuation.Continue();

            var relativeUrl = env.RelativeUrlWithoutQueryString();

            // Gets around what I *think* is a Katana bug 
            if (relativeUrl.StartsWith("http:/")) return MiddlewareContinuation.Continue();

            var file = _files.Find(relativeUrl);
            if (file == null) return MiddlewareContinuation.Continue();

            if (!_settings.IsAllowed(file)) return MiddlewareContinuation.Continue();

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