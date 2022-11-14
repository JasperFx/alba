using System.IO;

namespace Alba.Serialization
{
    public interface IJsonStrategy
    {
        Stream Write<T>(T body);
        T Read<T>(ScenarioResult response);
    }
}