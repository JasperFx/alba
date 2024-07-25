using System.Diagnostics.CodeAnalysis;
using System.Text;
using Alba.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
 
namespace Alba;

public static class HttpContextExtensions
{
    public static void ContentType(this HttpRequest request, string mimeType)
    {
        request.ContentType = mimeType;
    }

    public static void ContentType(this HttpResponse response, string mimeType)
    {
        response.Headers.ContentType = mimeType;
    }

    public static void RelativeUrl(this HttpContext context, [StringSyntax(StringSyntaxAttribute.Uri)]string? relativeUrl)
    {
        if (relativeUrl != null && relativeUrl.Contains("?"))
        {
            var parts = relativeUrl.Trim().Split('?');
            context.Request.Path = parts[0];

            if (parts[1].IsNotEmpty())
            {
                context.Request.QueryString = QueryString.Create(QueryHelpers.ParseQuery(parts[1]));
            }
        }
        else
        {
            context.Request.Path = relativeUrl;
        }
    }

    /// <summary>
    /// Set the Authorization header value to "Bearer [jwt]"
    /// </summary>
    /// <param name="context"></param>
    /// <param name="jwt"></param>
    public static void SetBearerToken(this HttpContext context, string jwt)
    {
        context.Request.Headers.Authorization = $"Bearer {jwt}";
    }

    public static void Accepts(this HttpContext context, string mimeType)
    {
        context.Request.Headers.Accept = mimeType;
    }

    public static void HttpMethod(this HttpContext context, string method)
    {
        context.Request.Method = method;
    }

    public static void StatusCode(this HttpContext context, int statusCode)
    {
        context.Response.StatusCode = statusCode;
    }

    public static void Write(this HttpResponse response, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        response.Body.Write(bytes, 0, bytes.Length);
        response.Body.Flush();
    }

}