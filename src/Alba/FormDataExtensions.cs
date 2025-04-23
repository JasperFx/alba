using Microsoft.AspNetCore.Http;

namespace Alba;

public static class FormDataExtensions
{
    /// <summary>
    /// Write the dictionary values to the HttpContext.Request.Body.
    /// Also sets content-length and content-type header to
    /// application/x-www-form-urlencoded
    /// </summary>
    /// <param name="context"></param>
    /// <param name="values"></param>
    public static void WriteFormData(this HttpContext context, Dictionary<string, string> values)
    {
        using var form = new FormUrlEncodedContent(values);

        form.CopyTo(context.Request.Body, null, CancellationToken.None);

        context.Request.Headers.ContentType = form.Headers.ContentType!.ToString();
        context.Request.Headers.ContentLength = form.Headers.ContentLength;

    }

        
    /// <summary>
    /// Writes the <see cref="MultipartFormDataContent"/> to the provided HttpContext, along with the
    /// required headers.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="content"></param>
    public static void WriteMultipartFormData(this HttpContext context, MultipartFormDataContent content)
    {
        content.CopyTo(context.Request.Body, null, CancellationToken.None);
        foreach (var kv in content.Headers)
        {
            context.Request.Headers.Append(kv.Key, kv.Value.ToArray());
        }
    }
}