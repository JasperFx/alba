using System;
using System.Collections.Generic;
using System.Linq.Expressions;
#nullable enable
namespace Alba
{
    public interface IUrlExpression
    {

        /// <summary>
        /// Specify the relative url for the scenario
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <returns></returns>
        SendExpression Url(string relativeUrl);

        /// <summary>
        /// Writes the input object into Json to the Http Request, and
        /// if enabled in your Alba system, sets the Url to match the
        /// input type and Http method
        /// </summary>
        /// <param name="input"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        SendExpression Json<T>(T input) where T : class;

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


    }
}