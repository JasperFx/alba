using System;
using Baseline;

namespace Alba.Routing
{
    internal class HandlerMethods
    {
        private readonly LightweightCache<string, Route> _routesByMethod
            = new LightweightCache<string, Route>();

        public Type HandlerType { get; }

        public HandlerMethods(Type handlerType)
        {
            HandlerType = handlerType;
        }


    }
}