using System;
using System.Collections.Generic;

namespace Alba.StaticFiles
{
    public abstract class WriterContinuation : MiddlewareContinuation
    {
        protected WriterContinuation(IDictionary<string, object> env, DoNext doNext)
        {
            if (env == null) throw new ArgumentNullException(nameof(env));

            DoNext = doNext;

            Action = () => {
                Write(env);
                env.Flush();
            };
        }

        public abstract void Write(IDictionary<string, object> response);
    }
}