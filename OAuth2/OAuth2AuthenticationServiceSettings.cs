public class OAuth2AuthenticationServiceSettings
{
    public string Serever { get; }
    public string TokenUrl { get; }
    public string UserName { get; }
    public string Password { get; }
    public string ClientId { get; }
    public string BearerToken { get; }

    public OAuth2AuthenticationServiceSettings(string server,string tokenUrl, string username, string password, string clientId, string bearerToken)
    {
        Serever = server;
        TokenUrl = tokenUrl;
        UserName = username;
        Password = password;
        ClientId = clientId;
        BearerToken = bearerToken;
    }
}