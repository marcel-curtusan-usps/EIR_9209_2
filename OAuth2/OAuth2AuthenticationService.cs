using EIR_9209_2.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

public class OAuth2AuthenticationService : IOAuth2AuthenticationService, IDisposable
{
    private readonly IHttpClientFactory _httpClient;
    private readonly OAuth2AuthenticationServiceSettings _authSettings;
    private readonly JsonSerializerSettings _jsonSettings;
    /// <summary>
    /// Logger service used for logging within the OAuth2AuthenticationService.
    /// </summary>
    protected readonly ILoggerService _logger;

    private string _accessToken = null!;
    private DateTime _refreshTokenUtcTime;
    private bool disposedValue;
    private readonly SemaphoreSlim _semaphore = new(1);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="httpClient"></param>
    /// <param name="oAuth2AuthenticationServiceSettings"></param>
    /// <param name="jsonSettings"></param> <summary>
    /// 
    /// </summary>
    public OAuth2AuthenticationService(ILoggerService logger, IHttpClientFactory httpClient, OAuth2AuthenticationServiceSettings oAuth2AuthenticationServiceSettings, JsonSerializerSettings jsonSettings)
    {
        _httpClient = httpClient;
        _authSettings = oAuth2AuthenticationServiceSettings;
        _jsonSettings = jsonSettings;
        _logger = logger;

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns> <summary>
    public async Task AddAuthHeader(HttpRequestMessage request, CancellationToken ct)
    {
        bool acquiredLock = false;
        try
        {
            await _semaphore.WaitAsync(ct);
            acquiredLock = true;

            await AddAuthHeaderCore(request, ct);
        }
        catch (Exception e)
        {
            await _logger.LogData(JToken.FromObject(e.Message), "Error", _authSettings.AuthType, _authSettings.TokenUrl);
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
        if (_authSettings.AuthType == "basicAuth")
        {
            var credentials = $"{_authSettings.UserName}:{_authSettings.Password}";
            var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
            request.Headers.Add("Authorization", $"Basic {encodedCredentials}");
        }
        if (_authSettings.AuthType == "bearerToken")
        {
            request.Headers.Add("Authorization", $"Bearer {_authSettings.BearerToken}");
        }
        if (_authSettings.AuthType == "oAuth2")
        {
            await AuthenticateAsync(ct);
            request.Headers.Add("Authorization", $"Bearer {_accessToken}");
        }

    }
    private async Task GetAccessTokenAsync()
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

            var postResponse = await client.PostAsync(string.Format(_authSettings.TokenUrl, _authSettings.Serever), content);
            postResponse.EnsureSuccessStatusCode();

            var responseBody = await postResponse.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody, _jsonSettings) ?? throw new InvalidOperationException("Failed to deserialize token response to JSON dictionary.");
            _accessToken = GetJsonKeyValue("access_token", json);
            _refreshTokenUtcTime = DateTime.UtcNow + TimeSpan.FromSeconds(int.Parse(GetJsonKeyValue("expires_in", json))) - TimeSpan.FromSeconds(10);
        }
        catch (Exception e)
        {
            await _logger.LogData(JToken.FromObject(e.Message), "Error", _authSettings.AuthType, _authSettings.TokenUrl);
        }
    }
    private async Task AuthenticateAsync(CancellationToken ct)
    {
        if (DateTime.UtcNow > _refreshTokenUtcTime)
        {
            await GetAccessTokenAsync();
        }
    }
    private static string GetJsonKeyValue(string keyName, Dictionary<string, string> json)
    {
        if (json == null)
        {
            throw new ArgumentNullException(nameof(json), "JSON dictionary cannot be null");
        }
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
    /// <summary>
    /// Disposes the resources used by the OAuth2AuthenticationService.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // Dispose managed state (managed objects)
                _semaphore.Dispose();
            }

            disposedValue = true;
            _semaphore.Release();
        }
    }
    /// <summary>
    /// Disposes the resources used by the OAuth2AuthenticationService.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}