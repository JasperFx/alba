using Alba.Internal;
using Shouldly;

namespace Alba.Testing;

public class ScenarioExtensionsTests : ScenarioContext
{
    [Fact]
    public Task from_http_request_message_with_get_request()
    {
        router.Handlers["/api/test"] = c =>
        {
            c.Response.Write("success");
            return Task.CompletedTask;
        };

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");

        return host.Scenario(scenario =>
        {
            scenario.FromHttpRequestMessage(request);
            scenario.StatusCodeShouldBeOk();
            scenario.ContentShouldBe("success");
        });
    }

    [Fact]
    public Task from_http_request_message_with_post_request_and_content()
    {
        router.Handlers["/api/data"] = c =>
        {
            var body = c.Request.Body.ReadAllBytes();
            var text = System.Text.Encoding.UTF8.GetString(body);
            text.ShouldBe("test data");
            c.Response.Write("received");
            return Task.CompletedTask;
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/data")
        {
            Content = new StringContent("test data", System.Text.Encoding.UTF8, "text/plain")
        };

        return host.Scenario(scenario =>
        {
            scenario.FromHttpRequestMessage(request);
            scenario.StatusCodeShouldBeOk();
            scenario.ContentShouldBe("received");
        });
    }

    [Fact]
    public Task from_http_request_message_with_put_request()
    {
        router.Handlers["/api/update"] = c =>
        {
            c.Response.Write("updated");
            return Task.CompletedTask;
        };

        var request = new HttpRequestMessage(HttpMethod.Put, "http://localhost/api/update");

        return host.Scenario(scenario =>
        {
            scenario.FromHttpRequestMessage(request);
            scenario.StatusCodeShouldBeOk();
            scenario.ContentShouldBe("updated");
        });
    }

    [Fact]
    public Task from_http_request_message_with_delete_request()
    {
        router.Handlers["/api/remove"] = c =>
        {
            c.Response.Write("deleted");
            return Task.CompletedTask;
        };

        var request = new HttpRequestMessage(HttpMethod.Delete, "http://localhost/api/remove");

        return host.Scenario(scenario =>
        {
            scenario.FromHttpRequestMessage(request);
            scenario.StatusCodeShouldBeOk();
            scenario.ContentShouldBe("deleted");
        });
    }

    [Fact]
    public Task from_http_request_message_with_patch_request()
    {
        router.Handlers["/api/patch"] = c =>
        {
            c.Response.Write("patched");
            return Task.CompletedTask;
        };

        var request = new HttpRequestMessage(HttpMethod.Patch, "http://localhost/api/patch");

        return host.Scenario(scenario =>
        {
            scenario.FromHttpRequestMessage(request);
            scenario.StatusCodeShouldBeOk();
            scenario.ContentShouldBe("patched");
        });
    }

    [Fact]
    public Task from_http_request_message_with_head_request()
    {
        router.Handlers["/api/head"] = _ => Task.CompletedTask;

        var request = new HttpRequestMessage(HttpMethod.Head, "http://localhost/api/head");

        return host.Scenario(scenario =>
        {
            scenario.FromHttpRequestMessage(request);
            scenario.StatusCodeShouldBeOk();
        });
    }

    [Fact]
    public Task from_http_request_message_with_query_string()
    {
        router.Handlers["/api/search"] = c =>
        {
            c.Request.Query["q"].ToString().ShouldBe("test");
            c.Response.Write("found");
            return Task.CompletedTask;
        };

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/search?q=test");

        return host.Scenario(scenario =>
        {
            scenario.FromHttpRequestMessage(request);
            scenario.StatusCodeShouldBeOk();
            scenario.ContentShouldBe("found");
        });
    }

    [Fact]
    public Task from_http_request_message_with_custom_headers()
    {
        router.Handlers["/api/headers"] = c =>
        {
            c.Request.Headers["X-Custom-Header"].ToString().ShouldBe("custom-value");
            c.Response.Write("ok");
            return Task.CompletedTask;
        };

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/headers");
        request.Headers.Add("X-Custom-Header", "custom-value");

        return host.Scenario(scenario =>
        {
            scenario.FromHttpRequestMessage(request);
            scenario.StatusCodeShouldBeOk();
            scenario.ContentShouldBe("ok");
        });
    }

    [Fact]
    public Task from_http_request_message_with_bearer_token()
    {
        router.Handlers["/api/secure"] = c =>
        {
            var auth = c.Request.Headers["Authorization"].ToString();
            auth.ShouldBe("Bearer test-token-123");
            c.Response.Write("authorized");
            return Task.CompletedTask;
        };

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/secure");
        request.Headers.Add("Authorization", "Bearer test-token-123");

        return host.Scenario(scenario =>
        {
            scenario.FromHttpRequestMessage(request);
            scenario.StatusCodeShouldBeOk();
            scenario.ContentShouldBe("authorized");
        });
    }

    [Fact]
    public Task from_http_request_message_with_json_content()
    {
        router.Handlers["/api/json"] = c =>
        {
            var body = c.Request.Body.ReadAllBytes();
            var json = System.Text.Encoding.UTF8.GetString(body);
            json.ShouldBe("{\"name\":\"test\"}");
            c.Request.ContentType.ShouldBe("application/json; charset=utf-8");
            c.Response.Write("processed");
            return Task.CompletedTask;
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/json")
        {
            Content = new StringContent("{\"name\":\"test\"}", System.Text.Encoding.UTF8, "application/json")
        };

        return host.Scenario(scenario =>
        {
            scenario.FromHttpRequestMessage(request);
            scenario.StatusCodeShouldBeOk();
            scenario.ContentShouldBe("processed");
        });
    }

    [Fact]
    public Task from_http_request_message_with_byte_array_content()
    {
        var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z/C/HgAGgwJ/lK3Q6wAAAABJRU5ErkJggg==");

        router.Handlers["/api/binary"] = c =>
        {
            var body = c.Request.Body.ReadAllBytes();
            body.ShouldBe(bytes);
            c.Request.ContentType.ShouldBe("image/png");
            c.Response.Write("uploaded");
            return Task.CompletedTask;
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/binary")
        {
            Content = new ByteArrayContent(bytes)
        };
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

        return host.Scenario(scenario =>
        {
            scenario.FromHttpRequestMessage(request);
            scenario.StatusCodeShouldBeOk();
            scenario.ContentShouldBe("uploaded");
        });
    }

    [Fact]
    public Task from_http_request_message_with_relative_uri()
    {
        router.Handlers["/api/relative"] = c =>
        {
            c.Response.Write("relative path");
            return Task.CompletedTask;
        };

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/relative");

        return host.Scenario(scenario =>
        {
            scenario.FromHttpRequestMessage(request);
            scenario.StatusCodeShouldBeOk();
            scenario.ContentShouldBe("relative path");
        });
    }

    [Fact]
    public Task from_http_request_message_with_multiple_headers()
    {
        router.Handlers["/api/multiheader"] = c =>
        {
            c.Request.Headers["X-Custom-1"].ToString().ShouldBe("value1");
            c.Request.Headers["X-Custom-2"].ToString().ShouldBe("value2");
            c.Response.Write("multi");
            return Task.CompletedTask;
        };

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/multiheader");
        request.Headers.Add("X-Custom-1", "value1");
        request.Headers.Add("X-Custom-2", "value2");

        return host.Scenario(scenario =>
        {
            scenario.FromHttpRequestMessage(request);
            scenario.StatusCodeShouldBeOk();
            scenario.ContentShouldBe("multi");
        });
    }

    [Fact]
    public void from_http_request_message_throws_when_request_is_null()
    {
        Should.Throw<ArgumentNullException>(async () =>
        {
            await host.Scenario(scenario =>
            {
                scenario.FromHttpRequestMessage(null!);
            });
        });
    }

    [Fact]
    public void from_http_request_message_throws_when_request_uri_is_null()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, (Uri)null);

        Should.Throw<ArgumentException>(async () =>
        {
            await host.Scenario(scenario =>
            {
                scenario.FromHttpRequestMessage(request);
            });
        });
    }

    [Fact]
    public void from_http_request_message_throws_for_unsupported_method()
    {
        var request = new HttpRequestMessage(new HttpMethod("OPTIONS"), "http://localhost/api/test");

        Should.Throw<NotSupportedException>(async () =>
        {
            await host.Scenario(scenario =>
            {
                scenario.FromHttpRequestMessage(request);
            });
        });
    }

    [Fact]
    public Task from_http_request_message_with_content_headers()
    {
        router.Handlers["/api/content-headers"] = c =>
        {
            c.Request.Headers["Content-Language"].ToString().ShouldBe("en-US");
            c.Response.Write("ok");
            return Task.CompletedTask;
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/content-headers")
        {
            Content = new StringContent("test")
        };
        request.Content.Headers.Add("Content-Language", "en-US");

        return host.Scenario(scenario =>
        {
            scenario.FromHttpRequestMessage(request);
            scenario.StatusCodeShouldBeOk();
            scenario.ContentShouldBe("ok");
        });
    }
}