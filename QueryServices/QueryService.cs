using EIR_9209_2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
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

    public async Task<QuuppaTag> GetQuuppaTagData(CancellationToken ct)
    {
        var query = "";

        if (!string.IsNullOrEmpty(query))
        {
            return (await GetPostQueryResults<QuuppaTag>(_fullUrl.AbsoluteUri, query, ct).ConfigureAwait(false));
        }
        else
        {
            return (await GetQueryResults<QuuppaTag>(_fullUrl.AbsoluteUri, ct).ConfigureAwait(false));
        }
    }

    private async Task<T> GetQueryResults<T>(string queryUrl, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true };
            if (_authService != null)
            {
                await _authService.AddAuthHeader(request, ct);
            }
            var response = await _httpClient.SendAsync(request, ct);

            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseBody, _jsonSettings);
        }
        catch (HttpRequestException ex)
        {
            // Log the exception or handle it in some other way
            // For example, you might want to rethrow the exception to let the caller handle it
            throw new Exception("An error occurred while sending the HTTP request.", ex);
        }
        catch (JsonException ex)
        {
            // Log the exception or handle it in some other way
            // For example, you might want to rethrow the exception to let the caller handle it
            throw new Exception("An error occurred while deserializing the response body.", ex);
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