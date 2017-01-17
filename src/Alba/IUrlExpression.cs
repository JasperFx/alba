using System;
using System.Linq.Expressions;

namespace Alba
{
    public interface IUrlExpression
    {
        SendExpression Action<T>(Expression<Action<T>> expression);
        SendExpression Url(string relativeUrl);
        SendExpression Input<T>(T input = null) where T : class;

        SendExpression Json<T>(T input) where T : class;
        SendExpression Xml<T>(T input) where T : class;

        SendExpression FormData<T>(T input) where T : class;
    }
}