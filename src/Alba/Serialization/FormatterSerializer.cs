using System;
using System.IO;
using System.Threading.Tasks;
using Alba.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Alba.Serialization
{
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

        public Stream Write<T>(T body)
        {
            var stubContext = new DefaultHttpContext();
            var stream = new Scenario.RewindableStream();
            stubContext.Response.Body = stream; // Has to be rewindable

            var writer = new StreamWriter(stream);
            var outputContext =
                new OutputFormatterWriteContext(stubContext, (_, _) => writer, typeof(T), body);
            _output.WriteAsync(outputContext).GetAwaiter().GetResult();

            return stream;
        }

        public T Read<T>(ScenarioResult response)
        {
            var provider = _host.Services.GetRequiredService<IModelMetadataProvider>();
            var metadata = provider.GetMetadataForType(typeof(T));

            var standinContext = new DefaultHttpContext();
            var buffer = new MemoryStream();
            response.Context.Response.Body.CopyTo(buffer);
            buffer.Position = 0;

            var copy = new MemoryStream();
            buffer.CopyTo(copy);
            buffer.Position = 0;

            try
            {
                standinContext.Request.Body = buffer; // Need to trick the MVC conneg services

                if (buffer.Length == 0) throw new EmptyResponseException();

                var inputContext = new InputFormatterContext(standinContext, typeof(T).Name, new ModelStateDictionary(), metadata, (s, _) => new StreamReader(s));
                var result = _input.ReadAsync(inputContext).GetAwaiter().GetResult();

                if (result.HasError)
                {
                    copy.Position = 0;
                    var json = copy.ReadAllText();
                    throw new AlbaJsonFormatterException(json);
                }

                if (result.Model is T returnValue) return returnValue;

                throw new Exception("Unable to deserialize the response body to " + typeof(T).FullName);
            }
            finally
            {
                // This is to enable repeated reads
                copy.Position = 0;
                response.Context.Response.Body = copy;
            }
        }

        public async Task<T> ReadAsync<T>(ScenarioResult response)
        {
            var provider = _host.Services.GetRequiredService<IModelMetadataProvider>();
            var metadata = provider.GetMetadataForType(typeof(T));

            var standinContext = new DefaultHttpContext();
            var buffer = new MemoryStream();
            await response.Context.Response.Body.CopyToAsync(buffer);
            buffer.Position = 0;

            var copy = new MemoryStream();
            await buffer.CopyToAsync(copy);
            buffer.Position = 0;

            try
            {
                standinContext.Request.Body = buffer; // Need to trick the MVC conneg services

                if (buffer.Length == 0) throw new EmptyResponseException();

                var inputContext = new InputFormatterContext(standinContext, typeof(T).Name, new ModelStateDictionary(), metadata, (s, _) => new StreamReader(s));
                var result = await _input.ReadAsync(inputContext);

                if (result.HasError)
                {
                    copy.Position = 0;
                    var json = await copy.ReadAllTextAsync();
                    throw new AlbaJsonFormatterException(json);
                }

                if (result.Model is T returnValue) return returnValue;

                throw new Exception("Unable to deserialize the response body to " + typeof(T).FullName);
            }
            finally
            {
                // This is to enable repeated reads
                copy.Position = 0;
                response.Context.Response.Body = copy;
            }
        }
    }
}