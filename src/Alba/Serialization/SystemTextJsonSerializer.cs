using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Baseline;
using Microsoft.Extensions.DependencyInjection;

namespace Alba.Serialization
{
    public class SystemTextJsonSerializer : IJsonStrategy
    {
        private readonly JsonSerializerOptions _options;

        public SystemTextJsonSerializer(IAlbaHost host)
        {
            var options = host.Services.GetService<Microsoft.AspNetCore.Http.Json.JsonOptions>();

            _options = options?.SerializerOptions ?? new JsonSerializerOptions();
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
            return JsonSerializer.Deserialize<T>(json, _options);
        }

        public async Task<T> ReadAsync<T>(ScenarioResult scenarioResult)
        {
            var json = await scenarioResult.Context.Response.Body.ReadAllTextAsync();
            return JsonSerializer.Deserialize<T>(json, _options);
        }
    }
}