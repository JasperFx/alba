namespace Alba.Serialization;

public interface IJsonStrategy
{
    Stream Write<T>(T body);
    T Read<T>(ScenarioResult response);
    Task<T> ReadAsync<T>(ScenarioResult scenarioResult);
}