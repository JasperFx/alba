using System.Xml;
using Microsoft.AspNetCore.Http;

namespace Alba;

#region sample_IScenarioResult
public interface IScenarioResult
{
    /// <summary>
    ///     The raw HttpContext used during the scenario
    /// </summary>
    HttpContext Context { get; }

    /// <summary>
    /// Read the contents of the HttpResponse.Body as text
    /// </summary>
    /// <returns></returns>
    string ReadAsText();

    /// <summary>
    /// Read the contents of the HttpResponse.Body as text
    /// </summary>
    /// <returns></returns>
    Task<string> ReadAsTextAsync();

    /// <summary>
    /// Read the contents of the HttpResponse.Body into an XmlDocument object
    /// </summary>
    /// <returns></returns>
    XmlDocument? ReadAsXml();

    /// <summary>
    /// Read the contents of the HttpResponse.Body into an XmlDocument object
    /// </summary>
    /// <returns></returns>
    Task<XmlDocument?> ReadAsXmlAsync();

    /// <summary>
    /// Deserialize the contents of the HttpResponse.Body into an object
    /// of type T using the built in XmlSerializer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T? ReadAsXml<T>() where T : class;

    /// <summary>
    /// Deserialize the contents of the HttpResponse.Body into an object
    /// of type T using the configured Json serializer
    /// </summary>
    /// <exception cref="AlbaJsonFormatterException">Throws if the response cannot be deserialized.</exception>
    /// <exception cref="EmptyResponseException">Throws if the response is empty.</exception>
    T ReadAsJson<T>();

    /// <summary>
    /// Deserialize the contents of the HttpResponse.Body into an object
    /// of type T using the configured Json serializer
    /// </summary>
    /// <exception cref="AlbaJsonFormatterException">Throws if the response cannot be deserialized.</exception>
    /// <exception cref="EmptyResponseException">Throws if the response is empty.</exception>
    Task<T> ReadAsJsonAsync<T>();
        
}
#endregion