using System.Net.Http.Headers;

namespace Alba;

/// <summary>
/// Builds a multipart/form-data request body of form fields and file uploads
/// </summary>
public sealed class MultipartFormDataBuilder
{
    internal MultipartFormDataBuilder()
    {
    }

    internal MultipartFormDataContent Content { get; } = new();

    /// <summary>
    /// Add a form field
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public MultipartFormDataBuilder Field(string name, string value)
    {
        Content.Add(new StringContent(value), name);
        return this;
    }

    /// <summary>
    /// Add a file upload. Add several files with the same name to bind them to a collection.
    /// </summary>
    /// <param name="name">The form field name the application binds the file from</param>
    /// <param name="content"></param>
    /// <param name="fileName"></param>
    /// <param name="contentType">Inferred from the file name extension when not supplied</param>
    /// <returns></returns>
    public MultipartFormDataBuilder File(string name, byte[] content, string fileName, string? contentType = null)
    {
        return addFile(new ByteArrayContent(content), name, fileName, contentType);
    }

    /// <summary>
    /// Add a file upload read from the stream when the request is sent. Add several files with
    /// the same name to bind them to a collection.
    /// </summary>
    /// <param name="name">The form field name the application binds the file from</param>
    /// <param name="content"></param>
    /// <param name="fileName"></param>
    /// <param name="contentType">Inferred from the file name extension when not supplied</param>
    /// <returns></returns>
    public MultipartFormDataBuilder File(string name, Stream content, string fileName, string? contentType = null)
    {
        return addFile(new StreamContent(content), name, fileName, contentType);
    }

    private MultipartFormDataBuilder addFile(HttpContent file, string name, string fileName, string? contentType)
    {
        if (contentType == null && !MimeType.FileExtensionContentTypeProvider.TryGetContentType(fileName, out contentType))
        {
            contentType = "application/octet-stream";
        }

        file.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        Content.Add(file, name, fileName);
        return this;
    }
}
