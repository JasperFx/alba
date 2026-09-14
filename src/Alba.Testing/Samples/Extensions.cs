using Alba.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Alba.Testing.Samples;

public class Extensions
{
    public async Task FluentConfiguration()
    {
        #region sample_fluent_configuration

        await using var host = await AlbaHost.For<WebAppSecuredWithJwt.Program>(x =>
            {
                // Service registrations and other ASP.NET Core host
                // customization belong in this action
                x.ConfigureServices(services =>
                {
                    // services.AddSingleton<IExternalService, StubbedExternalService>();
                });
            })
            // Override configuration values. These are visible to the
            // application's own startup code
            .WithConfiguration("ConnectionStrings:Postgres", "MyOverriddenValue")
            // Apply Alba extensions
            .WithExtension(new AuthenticationStub().WithName("jeremy"));

        #endregion
    }
}
