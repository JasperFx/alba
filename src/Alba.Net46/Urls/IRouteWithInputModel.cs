using System.Collections.Generic;

namespace Alba.Urls
{
    public interface IRouteWithInputModel : IRoute
    {
        IDictionary<string, string> ToParameters(object model);

        void ApplyValues(object input, IDictionary<string, string> rawValues);
    }
}