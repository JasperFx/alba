using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Alba.Urls
{
    public interface IMethodRoute<THandler> : IRoute
    {
        IDictionary<string, string> ToParameters(Expression<Action<THandler>> expression);
    }
}