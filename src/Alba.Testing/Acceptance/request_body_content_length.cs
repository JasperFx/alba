using System.Text;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Alba.Testing.Acceptance;

public class request_body_content_length : IAsyncLifetime
{
    private const string NonAscii = "Håkan är på väg – 日本語";

    private IAlbaHost _host = null!;

    public async ValueTask InitializeAsync()
    {
        var builder = new HostBuilder().ConfigureWebHost(x =>
        {
            // Echoes the declared content length and the real byte count of the body
            x.Configure(app => app.Run(async c =>
            {
                var body = await new StreamReader(c.Request.Body, Encoding.UTF8).ReadToEndAsync();
                await c.Response.WriteAsync($"{c.Request.ContentLength}:{Encoding.UTF8.GetByteCount(body)}");
            }));
        });

        _host = await AlbaHost.For(builder);
    }

    public async ValueTask DisposeAsync()
    {
        await _host.DisposeAsync();
    }

    [Fact]
    public async Task text_body_declares_the_utf8_byte_count()
    {
        var expected = Encoding.UTF8.GetByteCount(NonAscii);
        expected.ShouldBeGreaterThan(NonAscii.Length);

        await _host.Scenario(x =>
        {
            x.Post.Text(NonAscii).ToUrl("/");
            x.ContentShouldBe($"{expected}:{expected}");
        });
    }

    [Fact]
    public async Task xml_body_declares_the_utf8_byte_count()
    {
        var result = await _host.Scenario(x => { x.Post.Xml(new Message { Name = NonAscii }).ToUrl("/"); });

        var parts = result.ReadAsText().Split(':');
        parts[0].ShouldBe(parts[1]);
    }

    [XmlRoot("Message")]
    public class Message
    {
        public string Name { get; set; } = string.Empty;
    }
}
