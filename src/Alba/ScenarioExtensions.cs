namespace Alba;

public static class ScenarioExtensions
{
    /// <summary>
    /// Send a request using an HttpRequestMessage
    /// </summary>
    /// <param name="scenario">The Alba scenario</param>
    /// <param name="request">The HttpRequestMessage to send</param>
    /// <returns>SendExpression to continue configuring the scenario</returns>
    public static SendExpression FromHttpRequestMessage(this Scenario scenario, HttpRequestMessage request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.RequestUri == null)
            throw new ArgumentException("HttpRequestMessage must have a RequestUri", nameof(request));

        var relativeUrl = request.RequestUri.IsAbsoluteUri
            ? request.RequestUri.PathAndQuery
            : request.RequestUri.ToString();

        // Any verb the application routes, standard or custom
        var method = request.Method.Method.ToUpperInvariant();
        scenario.ConfigureHttpContext(c => c.HttpMethod(method));
        var sendExpression = ((IUrlExpression)scenario).Url(relativeUrl);

        foreach (var header in request.Headers)
        {
            scenario.WithRequestHeader(header.Key, string.Join(", ", header.Value));
        }

        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                scenario.WithRequestHeader(header.Key, string.Join(", ", header.Value));
            }

            scenario.Stream(request.Content.ReadAsStream());
        }

        return sendExpression;
    }
}
