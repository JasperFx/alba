using System;
using Baseline;

namespace Alba.Urls
{
    internal class HandlerMethods
    {
        private readonly LightweightCache<string, IRoute> _routesByMethod
            = new LightweightCache<string, IRoute>();

        public Type HandlerType { get; }

        public HandlerMethods(Type handlerType)
        {
            HandlerType = handlerType;
        }


    }
}