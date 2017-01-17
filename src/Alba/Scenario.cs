using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alba.Stubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Alba
{
    public class Scenario
    {
        private readonly IList<Func<HttpContext, Task>> _befores = new List<Func<HttpContext, Task>>();
        private readonly IList<Func<HttpContext, Task>> _afters = new List<Func<HttpContext, Task>>();

        private readonly IList<IScenarioAssertion> _assertions = new List<IScenarioAssertion>();

        public Scenario(IFeatureCollection features, IServiceProvider requestServices)
        {
            Context = new StubHttpContext(features, requestServices);
        }

        public HttpContext Context { get; }

        internal async Task RunBeforeActions()
        {
            foreach (var before in _befores)
            {
                await before(Context).ConfigureAwait(false);
            }
        }

        internal async Task RunAfterActions()
        {
            foreach (var before in _afters)
            {
                await before(Context).ConfigureAwait(false);
            }
        }

        // holds on to the http context & IApplicationServer


        public void AssertThat(IScenarioAssertion assertion)
        {
            _assertions.Add(assertion);
        }


        public void Before<T>(Func<T, HttpContext, Task> action)
        {
            throw new NotImplementedException();
        }

        public void After<T>(Func<T, HttpContext, Task> action)
        {
            throw new NotImplementedException();
        }

        public void Before<T>(Func<HttpContext, Task> action)
        {
            throw new NotImplementedException();
        }

        public void After<T>(Func<HttpContext, Task> action)
        {
            throw new NotImplementedException();
        }

        public void Before<T>(Func<T, Task> action)
        {
            throw new NotImplementedException();
        }

        public void After<T>(Func<T, Task> action)
        {
            throw new NotImplementedException();
        }


        public void RunAssertions()
        {
            throw new NotImplementedException();
        }
    }
}