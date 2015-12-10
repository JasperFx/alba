using System;
using System.Reflection;

namespace Alba.Urls
{
    public interface IUrlGraph
    {
        void RegisterByHandler(Type handlerType, MethodInfo method, IRoute route);
        void RegisterByInput(Type inputModel, IRouteWithInputModel route);
        void Register(string name, IRoute route);
    }
}