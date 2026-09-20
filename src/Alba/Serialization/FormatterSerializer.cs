using Alba.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Alba.Serialization;

public class FormatterSerializer : IJsonStrategy
{
    private readonly IModelMetadataProvider _metadata;
    private readonly InputFormatter _input;
    private readonly OutputFormatter _output;

    public FormatterSerializer(AlbaHost host, InputFormatter jsonInput, OutputFormatter jsonOutput)
    {
        _metadata = host.Services.GetRequiredService<IModelMetadataProvider>();
        _input = jsonInput;
        _output = jsonOutput;
    }

    internal InputFormatter InputFormatter => _input;
    internal OutputFormatter OutputFormatter => _output;

    public async Task<Stream> WriteAsync<T>(T body)
    {
        var stream = new MemoryStream();
        var stubContext = new DefaultHttpContext();
        stubContext.Response.Body = stream;

        // The same writer MVC uses for responses: no encoding preamble, and disposing it
        // leaves the stream open for the request to read
        var outputContext = new OutputFormatterWriteContext(stubContext,
            (s, encoding) => new HttpResponseStreamWriter(s, encoding), typeof(T), body);
        await _output.WriteAsync(outputContext);

        return stream;
    }

    public T Read<T>(ScenarioResult response)
    {
        // The formatter only touches the in-memory body, so blocking here never waits on real I/O
        return ReadAsync<T>(response).GetAwaiter().GetResult();
    }

    public Task<T> ReadAsync<T>(ScenarioResult response)
    {
        return response.ReadAsync(async body =>
        {
            if (body.Length == 0) throw new EmptyResponseException();

            var result = await readWithFormatter<T>(body);

            if (result.HasError)
            {
                body.Position = 0;
                throw new AlbaJsonFormatterException(body.ReadAllText());
            }

            if (result.Model is T value) return value;

            throw new Exception("Unable to deserialize the response body to " + typeof(T).FullName);
        });
    }

    private Task<InputFormatterResult> readWithFormatter<T>(Stream body)
    {
        // The MVC formatter reads the request side of a context
        var standinContext = new DefaultHttpContext();
        standinContext.Request.Body = body;

        // The same reader MVC uses for requests; disposing it leaves the body open for repeated reads
        var inputContext = new InputFormatterContext(standinContext, typeof(T).Name, new ModelStateDictionary(),
            _metadata.GetMetadataForType(typeof(T)),
            (s, encoding) => new HttpRequestStreamReader(s, encoding));

        return _input.ReadAsync(inputContext);
    }
}
