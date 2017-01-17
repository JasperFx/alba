using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alba.StaticFiles
{
    public class MiddlewareContinuation
    {
        public static MiddlewareContinuation Continue(Action action = null)
        {
            return new MiddlewareContinuation { DoNext = DoNext.Continue, Action = action };
        }

        public static MiddlewareContinuation StopHere(Action action = null)
        {
            return new MiddlewareContinuation { DoNext = DoNext.Stop, Action = action };
        }

        public DoNext DoNext;
        public Action Action;

        public void AssertOnlyContinuesToTheInner()
        {
            if (Action != null)
            {
                throw new Exception("There is an action on this continuation");
            }

            if (DoNext == DoNext.Stop)
            {
                throw new Exception("The continuation is 'DoNext.Stop'");
            }
        }

        public Task ToTask(IDictionary<string, object> environment, Func<IDictionary<string, object>, Task> inner)
        {
            if (DoNext == DoNext.Continue)
            {
                if (Action != null)
                {
                    Action();
                }

                return inner(environment);
            }

            Action = Action ?? (() => { });

            return Task.Factory.StartNew(Action);
        }

        // TODO -- this might be useful later
        private static Task completedTask()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            return tcs.Task;
        }
    }
}