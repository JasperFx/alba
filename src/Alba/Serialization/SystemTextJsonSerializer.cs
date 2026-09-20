using System.Text.Json;
using Alba.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Alba.Serialization;

public class SystemTextJsonSerializer : IJsonStrategy
{
    private readonly JsonSerializerOptions _options;

    internal JsonSerializerOptions Options => _options;

    public SystemTextJsonSerializer(IAlbaHost host)
    {
        var options = host.Services.GetService<IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>();

        _options = options?.Value.SerializerOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    public Task<Stream> WriteAsync<T>(T body)
    {
        var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, body, _options);
        return Task.FromResult<Stream>(stream);
    }

    public T Read<T>(ScenarioResult response)
    {
        return response.Read(body => requireValue(JsonSerializer.Deserialize<T>(body, _options), body));
    }

    public Task<T> ReadAsync<T>(ScenarioResult response)
    {
        return response.ReadAsync(async body =>
            requireValue(await JsonSerializer.DeserializeAsync<T>(body, _options), body));
    }

    private static T requireValue<T>(T? value, Stream body)
    {
        if (value is not null) return value;

        body.Position = 0;
        throw new AlbaJsonFormatterException(body.ReadAllText());
    }
}
