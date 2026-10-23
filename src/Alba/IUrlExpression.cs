using System.Diagnostics.CodeAnalysis;

namespace Alba;

public enum JsonStyle
{
    /// <summary>
    /// Use the MVC Core formatter for JSON serialization
    /// </summary>
    Mvc,
        
    /// <summary>
    /// Use the Minimal API mechanism for JSON serialization
    /// </summary>
    MinimalApi
}
    
public interface IUrlExpression
{

    /// <summary>
    /// Specify the relative url for the scenario
    /// </summary>
    /// <param name="relativeUrl"></param>
    /// <returns></returns>
    SendExpression Url([StringSyntax(StringSyntaxAttribute.Uri)]string relativeUrl);

    /// <summary>
    /// Send the request to the endpoint with this ASP.NET Core route name, generating the url
    /// from the route values. Values that are not route parameters become the query string.
    /// An unknown route name or a missing required value throws before the request is sent.
    /// </summary>
    /// <param name="routeName">The route name from minimal API WithName(), MVC Name = or Wolverine RouteName</param>
    /// <param name="routeValues">An anonymous object, dictionary or RouteValueDictionary</param>
    /// <returns></returns>
    SendExpression RouteNamed(string routeName, object? routeValues = null);

    /// <summary>
    /// Serializes the input object as JSON.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="jsonStyle"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    SendExpression Json<T>(T input, JsonStyle? jsonStyle = null);
    
    /// <summary>
    /// Writes the input string as JSON. Useful if your data is already serialized.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    SendExpression RawJson(string input);
    
    /// <summary>
    /// Writes the input object into Xml to the Http Request, and
    /// if enabled in your Alba system, sets the Url to match the
    /// input type and Http method
    /// </summary>
    /// <param name="input"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    SendExpression Xml<T>(T input) where T : class;

    /// <summary>
    /// Writes the input object to form data in the Http request
    /// </summary>
    /// <param name="input"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    SendExpression FormData<T>(T input) where T : class;

    /// <summary>
    /// Writes multipart form data to the Http request
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    SendExpression MultipartFormData(MultipartFormDataContent content);

    /// <summary>
    /// Writes text to the request body as 'text/plain'
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    SendExpression Text(string text);

    /// <summary>
    /// Writes the dictionary data to form data in the Http request
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    SendExpression FormData(Dictionary<string, string> input);

    /// <summary>
    /// Writes the byte array to the body in the Http request
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    SendExpression ByteArray(byte[] input);

}