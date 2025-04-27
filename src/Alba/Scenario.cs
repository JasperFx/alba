using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Claims;
using Alba.Assertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Alba;

/// <summary>
/// Models both the setup and expectations for a single HTTP request made through
/// an AlbaHost
/// </summary>
public class Scenario : IUrlExpression
{
    private readonly ScenarioAssertionException _assertionRecords = new();

    private readonly List<IScenarioAssertion> _assertions = new();
    private readonly List<Action<HttpContext>> _setups = new();
    private readonly AlbaHost _system;
    private int _expectedStatusCode = 200;
    private bool _ignoreStatusCode;
        
    internal Scenario(AlbaHost system)
    {
        _system = system ?? throw new ArgumentNullException(nameof(system));
        Body = new HttpRequestBody(this);

        ConfigureHttpContext(c =>
        {
            c.Features.Set<IHttpRequestBodyDetectionFeature>(new EnableRequestBody());
            c.Request.Body = new MemoryStream();
        });
    }

    internal Dictionary<string, object> Items { get; } = new();

    /// <summary>
    /// Helpers to write content to the HttpRequest
    /// </summary>
    public HttpRequestBody Body { get; }

    /// <summary>
    /// Specify an HTTP GET Url
    /// </summary>
    public IUrlExpression Get
    {
        get
        {
            ConfigureHttpContext(context => context.HttpMethod("GET"));
            return this;
        }
    }

        
    /// <summary>
    /// Specify an HTTP PUT Url
    /// </summary>
    public IUrlExpression Put
    {
        get
        {
            ConfigureHttpContext(context => context.HttpMethod("PUT"));
            return this;
        }
    }

    /// <summary>
    /// Specify an HTTP DELETE Url
    /// </summary>
    public IUrlExpression Delete
    {
        get
        {
            ConfigureHttpContext(context => context.HttpMethod("DELETE"));
            return this;
        }
    }

    /// <summary>
    /// Specify an HTTP POST Url
    /// </summary>
    public IUrlExpression Post
    {
        get
        {
            ConfigureHttpContext(context => context.HttpMethod("POST"));
            return this;
        }
    }

    /// <summary>
    /// Specify an HTTP PATCH Url
    /// </summary>
    public IUrlExpression Patch
    {
        get
        {
            ConfigureHttpContext(context => context.HttpMethod("PATCH"));
            return this;
        }
    }

    /// <summary>
    /// Specify an HTTP HEAD Url
    /// </summary>
    public IUrlExpression Head
    {
        get
        {
            ConfigureHttpContext(context => context.HttpMethod("HEAD"));
            return this;
        }
    }

    internal List<Claim> Claims { get; } = new();
    internal List<string> RemovedClaims { get; } = new(); 
    internal Exception? Exception { get; set; }


    SendExpression IUrlExpression.Url([StringSyntax(StringSyntaxAttribute.Uri)]string relativeUrl)
    {
        ConfigureHttpContext(context => context.RelativeUrl(relativeUrl));
        return new SendExpression(this);
    }

    SendExpression IUrlExpression.Json<T>(T input, JsonStyle? jsonStyle)
    {
        WriteJson(input, jsonStyle);

        ConfigureHttpContext(x => x.Accepts(MimeType.Json.Value));

        return new SendExpression(this);
    }

    SendExpression IUrlExpression.Xml<T>(T input)
    {
        Body.XmlInputIs(input);

        return new SendExpression(this);
    }

    SendExpression IUrlExpression.FormData<T>(T target)
    {
        var values = new Dictionary<string, string>();

        var properties = typeof(T).GetProperties().Where(x => x.CanWrite && x.CanRead);

        foreach (var prop in properties)
        {
            var rawValue = prop.GetValue(target, null);

            values.Add(prop.Name, rawValue?.ToString() ?? string.Empty);
        }

        var fields = typeof(T).GetFields();

        foreach (var field in fields)
        {
            var rawValue = field.GetValue(target);

            values.Add(field.Name, rawValue?.ToString() ?? string.Empty);
        }

        Body.WriteFormData(values);

        return new SendExpression(this);
    }

    SendExpression IUrlExpression.MultipartFormData(MultipartFormDataContent content)
    {
        Body.WriteMultipartFormData(content);

        return new SendExpression(this);
    }

    SendExpression IUrlExpression.FormData(Dictionary<string, string> input)
    {
        Body.WriteFormData(input);

        return new SendExpression(this);
    }

    /// <summary>
    /// Write the supplied byte array to the body of the request
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public SendExpression ByteArray(byte[] input)
    {
        ConfigureHttpContext(x =>
        {
            var content = new ByteArrayContent(input);
            content.CopyTo(x.Request.Body, null, CancellationToken.None);
            x.Request.Headers.ContentLength = content.Headers.ContentLength;
        });

        return new SendExpression(this);
    }

    /// <summary>
    /// Write the supplied text to the body of the request
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public SendExpression Text(string text)
    {
        Body.TextIs(text);
        ConfigureHttpContext(context => context.Request.ContentType = MimeType.Text.Value);
        ConfigureHttpContext(context => context.Request.ContentLength = text.Length);

        return new SendExpression(this);
    }

    /// <summary>
    ///     Register any kind of custom setup of the HttpContext within the request
    /// </summary>
    /// <param name="configure"></param>
    public void ConfigureHttpContext(Action<HttpContext> configure)
    {
        _setups.Add(configure);
    }

    /// <summary>
    /// Write the supplied input model to the request body using the configured
    /// formatter in the underlying application that supports the supplied content type
    /// </summary>
    /// <param name="input">An input model that should be serialized to the HTTP request body</param>
    /// <param name="jsonStyle"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public void WriteJson<T>(T input, JsonStyle? jsonStyle)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
            
        var jsonStrategy = _system.DefaultJson;
        if (jsonStyle == JsonStyle.Mvc) jsonStrategy = _system.MvcStrategy;
        if (jsonStyle == JsonStyle.MinimalApi) jsonStrategy = _system.MinimalApiStrategy;
            
        ConfigureHttpContext(c =>
        {
                
            var stream = jsonStrategy!.Write(input);

            c.Request.ContentType = "application/json";
            c.Request.Body = stream;
            c.Request.Body.Position = 0;
            c.Request.ContentLength = c.Request.Body.Length;
        });
    }

    /// <summary>
    ///     Add an assertion to the Scenario that will be executed after the request
    /// </summary>
    /// <param name="assertion"></param>
    /// <returns></returns>
    public Scenario AssertThat(IScenarioAssertion assertion)
    {
        _assertions.Add(assertion);

        return this;
    }

    internal void RunAssertions(HttpContext context)
    {
        var assertionContext = new AssertionContext(context, _assertionRecords);
        if (!_ignoreStatusCode)
        {
            new StatusCodeAssertion(_expectedStatusCode).Assert(this, assertionContext);
        }

        foreach (var assertion in _assertions) assertion.Assert(this, assertionContext);
        
        _assertionRecords.AssertAll();
    }

    /// <summary>
    ///     Verify the expected Http Status Code
    /// </summary>
    /// <param name="httpStatusCode"></param>
    /// <returns></returns>
    public Scenario StatusCodeShouldBe(HttpStatusCode httpStatusCode)
    {
        _expectedStatusCode = (int) httpStatusCode;
        return this;
    }

    /// <summary>
    ///     Verify the expected Http Status Code
    /// </summary>
    /// <returns></returns>
    public void StatusCodeShouldBe(int statusCode)
    {
        _expectedStatusCode = statusCode;
    }

    /// <summary>
    ///     Just ignore the Http Status Code when doing assertions against
    ///     the response
    /// </summary>
    public void IgnoreStatusCode()
    {
        _ignoreStatusCode = true;
    }


    /// <summary>
    /// Write the dictionary as form data to the HTTP request body
    /// and set the matching request content types
    /// </summary>
    /// <param name="input"></param>
    public void WriteFormData(Dictionary<string, string> input)
    {
        ConfigureHttpContext(c => c.WriteFormData(input));
    }


    /// <summary>
    /// Specify an expectation for the response headers
    /// </summary>
    /// <param name="headerKey"></param>
    /// <returns></returns>
    public HeaderExpectations Header(string headerKey)
    {
        return new(this, headerKey);
    }

    internal void Rewind()
    {
        ConfigureHttpContext(context => context.Request.Body.Position = 0);
    }

    /// <summary>
    ///     Only for internal Alba testing, but this writes its input
    ///     to an HttpContext
    /// </summary>
    /// <param name="context"></param>
    internal void SetupHttpContext(HttpContext context)
    {
        foreach (var setup in _setups) setup(context);
    }

    /// <summary>
    ///     Set a value for a request header
    /// </summary>
    /// <param name="headerKey"></param>
    /// <param name="value"></param>
    public void WithRequestHeader(string headerKey, string value)
    {
        ConfigureHttpContext(c => c.Request.Headers[headerKey] = value);
    }

    /// <summary>
    ///     Remove all values for a request header
    /// </summary>
    /// <param name="headerKey"></param>
    public void RemoveRequestHeader(string headerKey)
    {
        ConfigureHttpContext(c => c.Request.Headers.Remove(headerKey));
    }

    /// <summary>
    ///     Add an additional claim to the HttpContext. This requires use of the JwtSecurityStub
    /// </summary>
    /// <param name="claim"></param>
    public void WithClaim(Claim claim)
    {
        Claims.Add(claim);
    }

    /// <summary>
    ///     Removes a default claim from the HttpContext. This requires use of the JwtSecurityStub.
    ///     Claims are removed before scenario-specific claims are added via <see cref="WithClaim"/>
    /// </summary>
    /// <param name="name"></param>
    public void RemoveClaim(string name)
    {
        RemovedClaims.Add(name);
    }

    /// <summary>
    ///     Set the Authorization header value to "Bearer [jwt]" on the HTTP request
    /// </summary>
    /// <param name="jwt"></param>
    public void WithBearerToken(string jwt)
    {
        ConfigureHttpContext(c => c.SetBearerToken(jwt));
    }

    internal class RewindableStream : MemoryStream
    {
        protected override void Dispose(bool disposing)
        {
            // Nothing!
        }
    }
}

internal class EnableRequestBody : IHttpRequestBodyDetectionFeature
{
    public bool CanHaveBody { get; } = true;
}