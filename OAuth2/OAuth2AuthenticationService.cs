using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;

internal class OAuth2AuthenticationService : IOAuth2AuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly OAuth2AuthenticationServiceSettings _authSettings;
    private readonly JsonSerializerSettings _jsonSettings;
    private string _accessToken;
    private DateTime _refreshTokenUtcTime;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

    public OAuth2AuthenticationService(HttpClient httpClient, OAuth2AuthenticationServiceSettings oAuth2AuthenticationServiceSettings, JsonSerializerSettings jsonSettings)
    {
        _httpClient = httpClient;
        _authSettings = oAuth2AuthenticationServiceSettings;
        _jsonSettings = jsonSettings;
    }

    public async Task AddAuthHeader(HttpRequestMessage request, CancellationToken ct)
    {
        bool acquiredLock = false;
        try
        {
            await _semaphore.WaitAsync();
            acquiredLock = true;

            await AddAuthHeaderCore(request, ct);
        }
        finally
        {
            if (acquiredLock)
            {
                _semaphore.Release();
            }
        }
    }
    private async Task AddAuthHeaderCore(HttpRequestMessage request, CancellationToken ct)
    {
        await AuthenticateAsync(ct);
        request.Headers.Add("Authorization", $"Bearer {_accessToken}");
    }
    private async Task GetAccessTokenAsync(CancellationToken ct)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username", _authSettings.UserName },
                    { "password", _authSettings.Password },
                    { "client_id", _authSettings.ClientId }
        });

        var postResponse = await _httpClient.PostAsync(_authSettings.TokenUrl, content);
        postResponse.EnsureSuccessStatusCode();

        var responseBody = await postResponse.Content.ReadAsStringAsync();
        var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody, _jsonSettings);
        _accessToken = GetJsonKeyValue("access_token", json);
        _refreshTokenUtcTime = DateTime.UtcNow + TimeSpan.FromSeconds(int.Parse(GetJsonKeyValue("expires_in", json))) - TimeSpan.FromSeconds(10);
    }
    private async Task AuthenticateAsync(CancellationToken ct)
    {
        if (DateTime.UtcNow > _refreshTokenUtcTime)
        {
            await GetAccessTokenAsync(ct);
        }
        //todo also refresh
    }
    private string GetJsonKeyValue(string keyName, Dictionary<string, string> json)
    {
        if (!json.ContainsKey(keyName))
        {
            throw new InvalidOperationException($"Token [{keyName}] not found in response");
        }
        if (string.IsNullOrWhiteSpace(json[keyName]))
        {
            throw new InvalidOperationException($"Empty [{keyName}] token found in response");
        }

        return json[keyName];
    }
}