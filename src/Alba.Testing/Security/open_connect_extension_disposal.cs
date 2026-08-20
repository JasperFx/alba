using Alba.Security;
using Shouldly;

namespace Alba.Testing.Security;

public class open_connect_extension_disposal
{
    [Fact]
    public async Task can_be_disposed_before_an_alba_host_starts_it()
    {
        // AssertValid() runs before the HttpClient is built, so a missing ClientId
        // leaves an extension that still has to dispose cleanly
        var extension = new OpenConnectClientCredentials();

        await Should.NotThrowAsync(async () => await ((IAsyncDisposable)extension).DisposeAsync());
        Should.NotThrow(() => ((IDisposable)extension).Dispose());
    }

    [Fact]
    public void fetching_a_token_before_start_explains_itself()
    {
        var extension = new OpenConnectUserPassword();

        var ex = Should.Throw<InvalidOperationException>(() => extension.FetchToken(null));

        ex.Message.ShouldContain("discovery document");
    }
}
