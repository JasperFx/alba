using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Alba.Routing;

namespace Alba.Scenarios
{
    public interface IScenarioSupport
    {
        string RootUrl { get; }
        T Get<T>();

        string ToJson(object document);
        T FromJson<T>(string json);
        T FromJson<T>(Stream stream);

        Task Invoke(Dictionary<string, object> env);

        IUrlRegistry Urls { get; }
    }
}