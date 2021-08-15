using System;
using System.IO;
using System.Xml;
using Microsoft.AspNetCore.Http;

namespace Alba
{
    #region sample_IScenarioResult
    public interface IScenarioResult
    {
        /// <summary>
        ///     Helpers to interrogate or read the HttpResponse.Body
        ///     of the request
        /// </summary>
        [Obsolete("Use the methods directly on IScenarioResult instead")]
        HttpResponseBody ResponseBody { get; }

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
        /// Read the contents of the HttpResponse.Body into an XmlDocument object
        /// </summary>
        /// <returns></returns>
        XmlDocument? ReadAsXml();

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
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T? ReadAsJson<T>();

        T? Read<T>(string contentType);
    }
    #endregion

}