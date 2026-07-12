using Alba.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Alba.Serialization;

public class FormatterSerializer : IJsonStrategy
{
    private readonly AlbaHost _host;
    private readonly InputFormatter _input;
    private readonly OutputFormatter _output;

    public FormatterSerializer(AlbaHost host, InputFormatter jsonInput, OutputFormatter jsonOutput)
    {
        _host = host;
        _input = jsonInput;
        _output = jsonOutput;
    }

    internal InputFormatter InputFormatter => _input;
    internal OutputFormatter OutputFormatter => _output;

    public async Task<Stream> WriteAsync<T>(T body)
    {
        var stubContext = new DefaultHttpContext();
        var stream = new Scenario.RewindableStream();
        stubContext.Response.Body = stream; // Has to be rewindable

        var writer = new StreamWriter(stream);
        var outputContext =
            new OutputFormatterWriteContext(stubContext, (_, _) => writer, typeof(T), body);
        await _output.WriteAsync(outputContext);

        return stream;
    }

    public T Read<T>(ScenarioResult response)
    {
        var body = response.Context.Response.Body;
        body.Position = 0;

        if (body.Length == 0) throw new EmptyResponseException();

        // The formatter gets its own copy because it may close the stream
        // it reads from; the response body stays untouched for repeated reads
        var buffer = new MemoryStream();
        body.CopyTo(buffer);
        buffer.Position = 0;
        body.Position = 0;

        // The formatter's ReadAsync only touches the in-memory buffer, so
        // blocking here never waits on real I/O
        var result = readWithFormatter<T>(buffer).GetAwaiter().GetResult();

        return processResult<T>(result, response);
    }

    public async Task<T> ReadAsync<T>(ScenarioResult response)
    {
        var body = response.Context.Response.Body;
        body.Position = 0;

        if (body.Length == 0) throw new EmptyResponseException();

        var buffer = new MemoryStream();
        await body.CopyToAsync(buffer);
        buffer.Position = 0;
        body.Position = 0;

        var result = await readWithFormatter<T>(buffer);

        return processResult<T>(result, response);
    }

    private Task<InputFormatterResult> readWithFormatter<T>(Stream buffer)
    {
        var provider = _host.Services.GetRequiredService<IModelMetadataProvider>();
        var metadata = provider.GetMetadataForType(typeof(T));

        var standinContext = new DefaultHttpContext();
        standinContext.Request.Body = buffer; // Need to trick the MVC conneg services

        var inputContext = new InputFormatterContext(standinContext, typeof(T).Name, new ModelStateDictionary(),
            metadata, (s, _) => new StreamReader(s));

        return _input.ReadAsync(inputContext);
    }

    private static T processResult<T>(InputFormatterResult result, ScenarioResult response)
    {
        var body = response.Context.Response.Body;

        if (result.HasError)
        {
            body.Position = 0;
            var json = body.ReadAllText();
            body.Position = 0;
            throw new AlbaJsonFormatterException(json);
        }

        if (result.Model is T returnValue) return returnValue;

        throw new Exception("Unable to deserialize the response body to " + typeof(T).FullName);
    }
}