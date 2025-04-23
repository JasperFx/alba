namespace Alba.Testing.Samples;

public class Extensions
{
    public async Task ConfigurationExtension()
    {
        #nullable enable
        #region sample_configuration_extension

        var configValues = new Dictionary<string, string?>()
        {
            { "ConnectionStrings:Postgres", "MyOverriddenValue" }
        };
        
        var host = await AlbaHost.For<WebAppSecuredWithJwt.Program>(builder =>
        {
            builder.ConfigureServices(c =>
            {
                // services config
            });
        }, ConfigurationOverride.Create(configValues));
        #endregion
    }
}