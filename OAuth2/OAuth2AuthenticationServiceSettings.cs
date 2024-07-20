public class OAuth2AuthenticationServiceSettings
{
    public string TokenUrl { get; }
    public string UserName { get; }
    public string Password { get; }
    public string ClientId { get; }
    public string BearerToken { get; }

    public OAuth2AuthenticationServiceSettings(string tokenUrl, string username, string password, string clientId, string bearerToken)
    {
        TokenUrl = tokenUrl;
        UserName = username;
        Password = password;
        ClientId = clientId;
        BearerToken = bearerToken;
    }
}