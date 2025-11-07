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

        // Get the relative URL (path + query)
        var relativeUrl = request.RequestUri.IsAbsoluteUri 
            ? request.RequestUri.PathAndQuery 
            : request.RequestUri.ToString();

        // Map HTTP method to the appropriate expression
        IUrlExpression urlExpression = request.Method.Method.ToUpperInvariant() switch
        {
            "GET" => scenario.Get,
            "POST" => scenario.Post,
            "PUT" => scenario.Put,
            "DELETE" => scenario.Delete,
            "PATCH" => scenario.Patch,
            "HEAD" => scenario.Head,
            _ => throw new NotSupportedException($"HTTP method '{request.Method}' is not supported")
        };

        // Set the URL
        var sendExpression = urlExpression.Url(relativeUrl);
        
        // Copy headers (excluding Content headers which are handled separately)
        foreach (var header in request.Headers)
        {
            var headerValue = string.Join(", ", header.Value);
            
            scenario.WithRequestHeader(header.Key, headerValue);
        }

        // Handle request content if present
        if (request.Content != null)
        {
            var contentBytes = request.Content.ReadAsStream();
            
            // Copy content-type header from content
            if (request.Content.Headers.ContentType != null)
            {
                scenario.WithRequestHeader("Content-Type", request.Content.Headers.ContentType.ToString());
            }
            
            // Copy other content headers
            foreach (var header in request.Content.Headers)
            {
                if (!header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    var headerValue = string.Join(", ", header.Value);
                    scenario.WithRequestHeader(header.Key, headerValue);
                }
            }

            // Write content to scenario
            scenario.Stream(contentBytes);
        }

        return sendExpression;
    }
}

