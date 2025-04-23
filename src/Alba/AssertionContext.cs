using System.Diagnostics.CodeAnalysis;
using System.Text;
using Alba.Internal;
using Microsoft.AspNetCore.Http;

namespace Alba;

public sealed class AssertionContext
{
    private readonly ScenarioAssertionException _assertionException;
    public HttpContext HttpContext { get; }
    public AssertionContext(HttpContext httpContext, ScenarioAssertionException assertionException)
    {
        HttpContext = httpContext;
        _assertionException = assertionException;
    }
    
    /// <summary>
    /// Add an assertion failure message
    /// </summary>
    /// <param name="message"></param>
    public void AddFailure(string message) => _assertionException.Add(message);
    
    private string? _body;

    /// <summary>
    /// Reads the response body and returns it as a string
    /// </summary>
    /// <returns>A string with the content of the body</returns>
    [MemberNotNull(nameof(_body))]
    public string ReadBodyAsString()
    {
        // Hardening for GH-95
        try
        {
            var stream = HttpContext.Response.Body;
            if (_body == null)
            {
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }
                
                _body = Encoding.UTF8.GetString(stream.ReadAllBytes());
                
                // reset the position so users can do follow up activities without tripping up.
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }
                
                _assertionException.AddBody(_body);
            }
        }
        catch (Exception)
        {
            _body = string.Empty;
        }

        return _body;
    }
    

}