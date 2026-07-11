namespace Alba.Serialization;

public interface IJsonStrategy
{
    Task<Stream> WriteAsync<T>(T body);
    T Read<T>(ScenarioResult response);
    Task<T> ReadAsync<T>(ScenarioResult scenarioResult);
}