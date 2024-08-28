using EIR_9209_2.Service;
using Newtonsoft.Json;

public class OAuth2AuthenticationService : IOAuth2AuthenticationService, IDisposable
{
    private readonly IHttpClientFactory _httpClient;
    private readonly OAuth2AuthenticationServiceSettings _authSettings;
    private readonly JsonSerializerSettings _jsonSettings;
    protected readonly ILogger<BaseEndpointService> _logger;

    private string _accessToken;
    private DateTime _refreshTokenUtcTime;
    private bool disposedValue;
    private readonly SemaphoreSlim _semaphore = new(1);

    public OAuth2AuthenticationService(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClient, OAuth2AuthenticationServiceSettings oAuth2AuthenticationServiceSettings, JsonSerializerSettings jsonSettings)
    {
        _httpClient = httpClient;
        _authSettings = oAuth2AuthenticationServiceSettings;
        _jsonSettings = jsonSettings;
        _logger = logger;

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
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
        finally
        {
            if (acquiredLock)
            {
                _semaphore.Release();
            }
            Dispose();
        }
    }
    private async Task AddAuthHeaderCore(HttpRequestMessage request, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(_authSettings.BearerToken))
        {
            request.Headers.Add("Authorization", $"Bearer {_authSettings.BearerToken}");
        }
        else
        {
            await AuthenticateAsync(ct);
            request.Headers.Add("Authorization", $"Bearer {_accessToken}");
        }

    }
    private async Task GetAccessTokenAsync(CancellationToken ct)
    {
        try
        {
            var client = _httpClient.CreateClient();
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username", _authSettings.UserName },
                    { "password", _authSettings.Password },
                    { "client_id", _authSettings.ClientId }
        });

            var postResponse = await client.PostAsync(_authSettings.TokenUrl, content);
            postResponse.EnsureSuccessStatusCode();

            var responseBody = await postResponse.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody, _jsonSettings);
            _accessToken = GetJsonKeyValue("access_token", json);
            _refreshTokenUtcTime = DateTime.UtcNow + TimeSpan.FromSeconds(int.Parse(GetJsonKeyValue("expires_in", json))) - TimeSpan.FromSeconds(10);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
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
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}