using EIR_9209_2.Models;
using EIR_9209_2.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;


internal class QueryService : IQueryService
{
    private readonly IHttpClientFactory _httpClient;
    private readonly IOAuth2AuthenticationService _authService;
    private readonly JsonSerializerSettings _jsonSettings;
    private readonly Uri _baseQueryUrlWithPort;
    private readonly Uri _fullUrl;
    private readonly TimeSpan _timeout; // Add Timeout field
    protected readonly ILogger<BaseEndpointService> _logger;

    public QueryService(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClient, JsonSerializerSettings jsonSettings, QueryServiceSettings settings)
    {
        _logger = logger;
        _httpClient = httpClient;
        _jsonSettings = jsonSettings;
        _fullUrl = new Uri(settings.FullUrl);
        _timeout = settings.Timeout; // Initialize Timeout field
    }

    public QueryService(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClient, IOAuth2AuthenticationService authService, JsonSerializerSettings jsonSettings, QueryServiceSettings settings)
    {
        _logger = logger;
        _httpClient = httpClient;
        _authService = authService;
        _jsonSettings = jsonSettings;
        _baseQueryUrlWithPort = new Uri(settings.BaseQueryUrlWithPort);
        _fullUrl = new Uri(settings.FullUrl);
        _timeout = settings.Timeout; // Initialize Timeout field
    }

    public async Task<QuuppaTag> GetQPETagData(CancellationToken ct)
    {
        return await GetQueryResults<QuuppaTag>(_fullUrl.AbsoluteUri, ct).ConfigureAwait(false);
    }
    public async Task<QPEProjectInfo> GetQPEProjectInfo(CancellationToken ct)
    {
        return await GetQueryResults<QPEProjectInfo>(_fullUrl.AbsoluteUri, ct).ConfigureAwait(false);
    }
    public async Task<JToken> GetIDSData(CancellationToken ct)
    {
        return await GetQueryResults<dynamic>(_fullUrl.AbsoluteUri, ct).ConfigureAwait(false);
    }
    public async Task<string> SendEmail(CancellationToken ct)
    {
        return await GetPostQueryResults<dynamic>(_fullUrl.AbsoluteUri, new object(), ct).ConfigureAwait(false);
    }
    public async Task<JToken> GetSMSWrapperData(CancellationToken ct)
    {
        return await GetQueryResults<dynamic>(_fullUrl.AbsoluteUri, ct).ConfigureAwait(false);
    }
    public async Task<JToken> GetIVESData(CancellationToken ct)
    {
        return await GetQueryResults<dynamic>(_fullUrl.AbsoluteUri, ct).ConfigureAwait(false);
    }
    public async Task<JToken> GetMPEWatchData(CancellationToken ct)
    {
        return await GetQueryResults<JToken>(_fullUrl.AbsoluteUri, ct).ConfigureAwait(false);
    }
    public async Task<JToken> GetSVDoorData(CancellationToken ct)
    {
        return await GetQueryResults<JToken>(_fullUrl.AbsoluteUri, ct).ConfigureAwait(false);
    }
    public async Task<JToken> GetCiscoSpacesData(CancellationToken ct)
    {
        return await GetQueryResults<JToken>(_fullUrl.AbsoluteUri, ct).ConfigureAwait(false);
    }
    public async Task<JToken> GetCameraData(CancellationToken ct)
    {
        return await GetQueryResults<JToken>(_fullUrl.AbsoluteUri, ct).ConfigureAwait(false);
    }
    public async Task<byte[]> GetPictureData(CancellationToken ct)
    {
        return await GetQueryResults<byte[]>(_fullUrl.AbsoluteUri, ct).ConfigureAwait(false);
    }
    public async Task<List<(string areaId, string areaName)>> GetAreasAsync(CancellationToken ct)
    {
        string GetAreasUrlPath = "api/usps/area/list";
        var areas = await GetQueryResults<IEnumerable<QREArea>>(new Uri(_baseQueryUrlWithPort, GetAreasUrlPath).AbsoluteUri, ct);
        return areas.Select(a => (a.Id, a.Name)).ToList();
    }

    public async Task<List<AreaDwell>> GetTotalDwellTime(DateTime startTime, DateTime endTime, TimeSpan minEmployeeTimeInArea,
    TimeSpan TimeStep, TimeSpan ActivationTime, TimeSpan DeactivationTime, TimeSpan DisappearTime, List<(string areaId, string areaName)> allAreaIds, int areaBatchCount, CancellationToken ct)
    {
        var queries = BreakUpAreasIntoBatches()
              .Select(areasBatch => new ReportQueryBuilder()
              .WithQueryType(ESelsReportQueryType.TotalDwellTimeByPersonByOperationalArea)
              .WithStartLocalTime(startTime)
              .WithEndLocalTime(endTime)
              .WithMinTimeOnArea(minEmployeeTimeInArea)
              .WithTimeStep(TimeStep)
              .WithActivationTime(ActivationTime)
              .WithDeactivationTime(DeactivationTime)
              .WithDisappearTime(DisappearTime)
              .WithAreaIds(areasBatch.Select(a => a.areaId).ToList())
              .Build()
              );

        var queryTasks = queries.Select(query => GetPostQueryResults<List<TagDwellTimeInAreaQueryResult>>(_fullUrl.AbsoluteUri, query, ct));
        var queryResults = (await Task.WhenAll(queryTasks).ConfigureAwait(false))
            .SelectMany(x => x)
            .Where(r => !r.User.Equals("Empty Time"))
            .ToList();

        var result = TransformQueryResults(queryResults);

        return result;

        IEnumerable<List<(string areaId, string areaName)>> BreakUpAreasIntoBatches()
        {
            return Enumerable.Range(0, (allAreaIds.Count + areaBatchCount - 1) / areaBatchCount).Select(i => allAreaIds.Skip(i * areaBatchCount).Take(areaBatchCount).ToList());
        }
    }

    public async Task<List<TagTimeline>> GetTotalTagTimeLine(DateTime startTime, DateTime endTime, TimeSpan minEmployeeTimeInArea,
    TimeSpan TimeStep, TimeSpan ActivationTime, TimeSpan DeactivationTime, TimeSpan DisappearTime, List<(string areaId, string areaName)> allAreaIds, int areaBatchCount, CancellationToken ct)
    {
        var queries = BreakUpTimelinIntoBatches()
              .Select(areasBatch => new ReportQueryBuilder()
              .WithQueryType(ESelsReportQueryType.TimelineByPerson)
              .WithStartLocalTime(startTime)
              .WithEndLocalTime(endTime)
              .WithMinTimeOnArea(minEmployeeTimeInArea)
              .WithTimeStep(TimeStep)
              .WithActivationTime(ActivationTime)
              .WithDeactivationTime(DeactivationTime)
              .WithDisappearTime(DisappearTime)
              .WithAreaIds(areasBatch.Select(a => a.areaId).ToList())
              .Build()
              );

        var queryTasks = queries.Select(query => GetPostQueryResults<List<TagTimelineQueryResult>>(_fullUrl.AbsoluteUri, query, ct));
        var queryResults = (await Task.WhenAll(queryTasks).ConfigureAwait(false))
            .SelectMany(x => x)
            .Where(r => !r.User.Equals("Empty Time"))
            .ToList();

        var result = TransformTimeLineQueryResults(queryResults, startTime);

        return result;

        IEnumerable<List<(string areaId, string areaName)>> BreakUpTimelinIntoBatches()
        {
            return Enumerable.Range(0, (allAreaIds.Count + areaBatchCount - 1) / areaBatchCount).Select(i => allAreaIds.Skip(i * areaBatchCount).Take(areaBatchCount).ToList());
        }
    }

    private async Task<T> GetQueryResults<T>(string queryUrl, CancellationToken ct)
    {
        try
        {
            var client = _httpClient.CreateClient();
            client.Timeout = _timeout; // Set the timeout
            using var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true };
            if (_authService != null)
            {
                await _authService.AddAuthHeader(request, ct);
            }
            using var response = await client.SendAsync(request, ct);

            response.EnsureSuccessStatusCode();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseBody, _jsonSettings);
            }
            else
            {
                throw new Exception($"The response code is {response.StatusCode}.");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogInformation(queryUrl);
            _logger.LogError(ex.Message);
            throw new Exception("An error occurred while sending the HTTP request.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogInformation(queryUrl);
            _logger.LogError(ex.Message);
            throw new Exception("An error occurred while deserializing the response body.", ex);
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken == ct)
        {
            _logger.LogInformation(queryUrl);
            _logger.LogError("The request was canceled due to a timeout.");
            throw new Exception("The request was canceled due to a timeout.", ex);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex.Message);
            throw new Exception("An error occurred while connection.", ex);
        }
        catch (Exception e)
        {
            _logger.LogInformation(queryUrl);
            _logger.LogError(e.Message);
            throw new Exception("An error occurred while connection.", e);
        }
    }

    private async Task<T> GetPostQueryResults<T>(string queryUrl, object query, CancellationToken ct)
    {
        try
        {
            var client = _httpClient.CreateClient();
            client.Timeout = _timeout; // Set the timeout
            using var request = new HttpRequestMessage(HttpMethod.Post, queryUrl);
            if (_authService != null)
            {
                await _authService.AddAuthHeader(request, ct);
            }
            request.Content = new StringContent(JsonConvert.SerializeObject(query, _jsonSettings), Encoding.UTF8, "application/json");
            using var response = await client.SendAsync(request, ct);

            response.EnsureSuccessStatusCode();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseBody, _jsonSettings);
            }
            else
            {
                throw new Exception($"The response code is {response.StatusCode}.");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogInformation(queryUrl);
            _logger.LogError(ex.Message);
            throw new Exception("An error occurred while sending the HTTP request.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogInformation(queryUrl);
            _logger.LogError(ex.Message);
            throw new Exception("An error occurred while deserializing the response body.", ex);
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken == ct)
        {
            _logger.LogInformation(queryUrl);
            _logger.LogError("The request was canceled due to a timeout.");
            throw new Exception("The request was canceled due to a timeout.", ex);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex.Message);
            throw new Exception("An error occurred while connection.", ex);
        }
        catch (Exception e)
        {
            _logger.LogInformation(queryUrl);
            _logger.LogError(e.Message);
            throw new Exception("An error occurred while connection.", e);
        }
    }


    private List<AreaDwell> TransformQueryResults(List<TagDwellTimeInAreaQueryResult> results)
    {
        const string userRegexPattern = @"^(.+?)\s(.+?)\s\((\d+)\)$"; //expected pattern FIRSTNAME LASTNAME (EIN)
        return results
            .Where(r => Regex.Match(r.User, userRegexPattern).Success)
            .Select(r => new AreaDwell
            {
                FirstName = Regex.Match(r.User, userRegexPattern).Groups[1].Value,
                LastName = Regex.Match(r.User, userRegexPattern).Groups[2].Value,
                EmployeeName = string.Concat(Regex.Match(r.User, userRegexPattern).Groups[1].Value, @" ", Regex.Match(r.User, userRegexPattern).Groups[2].Value),
                Ein = Regex.Match(r.User, userRegexPattern).Groups[3].Value.PadLeft(8, '0'),
                AreaName = r.Area,
                DwellTimeDurationInArea = r.Duration,
                Type = r.Type
            }).ToList();
    }

    private List<TagTimeline> TransformTimeLineQueryResults(List<TagTimelineQueryResult> results, DateTime hour)
    {
        const string userRegexPattern = @"^(.+?)\s(.+?)\s\((\d+)\)$"; //expected pattern FIRSTNAME LASTNAME (EIN)
        return results
            .Where(r => Regex.Match(r.User, userRegexPattern,).Success)
            .Select(r => new TagTimeline
            {
                Hour = hour,
                FirstName = Regex.Match(r.User, userRegexPattern).Groups[1].Value,
                LastName = Regex.Match(r.User, userRegexPattern).Groups[2].Value,
                EmployeeName = string.Concat(Regex.Match(r.User, userRegexPattern).Groups[1].Value, @" ", Regex.Match(r.User, userRegexPattern).Groups[2].Value),
                Ein = Regex.Match(r.User, userRegexPattern).Groups[3].Value.PadLeft(8, '0'),
                AreaName = r.Area,
                Start = r.Start,
                End = r.End,
                Duration = r.Duration,
                Type = r.Type
            }).ToList();

    }
}