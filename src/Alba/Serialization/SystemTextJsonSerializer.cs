using System.Text.Json;
using Alba.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Alba.Serialization;

public class SystemTextJsonSerializer : IJsonStrategy
{
    private readonly JsonSerializerOptions _options;

    public SystemTextJsonSerializer(IAlbaHost host)
    {
        var options = host.Services.GetService<IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>> ();

        _options = options?.Value.SerializerOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    public Stream Write<T>(T body)
    {
        var stream = new MemoryStream();
        JsonSerializer.Serialize(new Utf8JsonWriter(stream), body, _options);
        return stream;
    }

    public T Read<T>(ScenarioResult response)
    {
        var json = response.Context.Response.Body.ReadAllText();
        var res = JsonSerializer.Deserialize<T>(json, _options);

        if (res is not null) return res;

        throw new AlbaJsonFormatterException(json);
    }

    public async Task<T> ReadAsync<T>(ScenarioResult response)
    {
        var json = await response.Context.Response.Body.ReadAllTextAsync();
        var res = JsonSerializer.Deserialize<T>(json, _options);

        if (res is not null) return res;

        throw new AlbaJsonFormatterException(json);
    }
}