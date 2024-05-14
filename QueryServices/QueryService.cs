using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

internal class QueryService : IQueryService
{
    private readonly HttpClient _httpClient;
    private readonly IOAuth2AuthenticationService _authService;
    private readonly JsonSerializerSettings _jsonSettings;
    private readonly Uri _fullUrl;

    public QueryService(HttpClient httpClient, JsonSerializerSettings jsonSettings, QueryServiceSettings settings)
    {
        this._httpClient = httpClient;
        this._jsonSettings = jsonSettings;
        this._fullUrl = new Uri(settings.FullUrl);
    }

    public QueryService(HttpClient httpClient, IOAuth2AuthenticationService authService, JsonSerializerSettings jsonSettings, QueryServiceSettings settings)
    {
        this._httpClient = httpClient;
        this._authService = authService;
        this._jsonSettings = jsonSettings;
        this._fullUrl = new Uri(settings.FullUrl);
    }

    public async Task<JObject> GetData(CancellationToken ct)
    {
        var query = "";
        var result = new JObject();
        if (!string.IsNullOrEmpty(query))
        {
            result = (await GetPostQueryResults<JObject>(_fullUrl.AbsoluteUri, query, ct).ConfigureAwait(false));
        }
        else
        {
            result = (await GetQueryResults<JObject>(_fullUrl.AbsoluteUri, ct).ConfigureAwait(false));
        }

        return result;
    }

    private async Task<T> GetQueryResults<T>(string queryUrl, CancellationToken ct)
    {
        using (var request = new HttpRequestMessage(HttpMethod.Get, queryUrl))
        {
            if (_authService != null)
            {
                await _authService.AddAuthHeader(request, ct);
            }
            var response = await _httpClient.SendAsync(request, ct);

            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseBody, _jsonSettings);
        }
    }

    private async Task<T> GetPostQueryResults<T>(string queryUrl, string query, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, queryUrl);
        if (_authService != null)
        {
            await _authService.AddAuthHeader(request, ct);
        }
        if (!string.IsNullOrEmpty(query))
        {
            request.Content = new StringContent(JsonConvert.SerializeObject(query, _jsonSettings), Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request, ct);

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseBody, _jsonSettings);
    }
}