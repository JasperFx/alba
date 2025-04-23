using Alba.Internal;
using IdentityModel.Client;

namespace Alba.Security;

internal class UserPassword
{
    public string UserName { get; }

    public UserPassword(string userName, string password)
    {
        UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        Password = password ?? throw new ArgumentNullException(nameof(password));
    }

    public string Password { get; }
}

public static class OpenConnectExtensions
{
    /// <summary>
    /// When used in conjunction with the OpenConnectUserPassword extension, this
    /// method can be used to use the supplied credentials for just this scenario
    /// </summary>
    /// <param name="scenario"></param>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    public static void UserAndPasswordIs(this Scenario scenario, string userName, string password)
    {
        var customization = new UserPassword(userName, password);
        scenario.Items.Add(OpenConnectExtension.OverrideKey, customization);
    }
}
    
public class OpenConnectUserPassword : OpenConnectExtension
{
    public override void AssertValid()
    {
        if (ClientId.IsEmpty()) throw new Exception($"{nameof(ClientId)} cannot be null");
        if (ClientSecret.IsEmpty()) throw new Exception($"{nameof(ClientSecret)} cannot be null");
        if (UserName.IsEmpty()) throw new Exception($"{nameof(UserName)} cannot be null");
        if (Password.IsEmpty()) throw new Exception($"{nameof(Password)} cannot be null");
    }
        
    /// <summary>
    /// The default UserName to use for authenticating each service call
    /// </summary>
    public string? UserName { get; set; }
        
    /// <summary>
    /// The default Password to use for authenticating each service call
    /// </summary>
    public string? Password { get; set; }

    public override Task<TokenResponse> FetchToken(HttpClient client, DiscoveryDocumentResponse? disco,
        object? tokenCustomization)
    {
        if (disco == null) throw new ArgumentNullException(nameof(disco), "Unable to load the token discovery document");
            
        if (tokenCustomization is UserPassword u)
        {
            return client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                UserName = u.UserName,
                Password = u.Password
            });
        }
            
        return client.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = disco.TokenEndpoint,
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            UserName = UserName,
            Password = Password
        });
    }
}