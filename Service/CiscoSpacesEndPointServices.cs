using Microsoft.AspNetCore.SignalR;

namespace EIR_9209_2.Service
{
    public class CiscoSpacesEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryTagsRepository _tags;

        public CiscoSpacesEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, IInMemoryTagsRepository tags)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection)
        {
            _tags = tags;
        }

        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                IOAuth2AuthenticationService authService;
                authService = new OAuth2AuthenticationService(_logger, _httpClientFactory, new OAuth2AuthenticationServiceSettings("", "", "", "", _endpointConfig.OutgoingApikey), jsonSettings);

                IQueryService queryService;
                //process tag data
                string FormatUrl = "";
                if (_endpointConfig.MessageType == "CLIENT")
                {
                    FormatUrl = string.Format(_endpointConfig.Url, _endpointConfig.MessageType, _endpointConfig.MapId, _endpointConfig.TenantId);
                    queryService = new QueryService(_logger, _httpClientFactory, authService, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));
                    var result = await queryService.GetCiscoSpacesData(stoppingToken);

                    // Process CLIENT data in a separate thread
                    Action processData = () => _tags.UpdateTagCiscoSpacesClientInfo(result);
                    await Task.Run(processData).ConfigureAwait(false);
                }
                if (_endpointConfig.MessageType == "BLE_TAG")
                {
                    FormatUrl = string.Format(_endpointConfig.Url, _endpointConfig.MessageType, _endpointConfig.MapId, _endpointConfig.TenantId);
                    queryService = new QueryService(_logger, _httpClientFactory, authService, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));
                    var result = await queryService.GetCiscoSpacesData(stoppingToken);

                    // Process tag data in a separate thread
                    Action processData = () => _tags.UpdateTagCiscoSpacesBLEInfo(result);
                    await Task.Run(processData).ConfigureAwait(false);
                }
                if (_endpointConfig.MessageType == "FLOOR")
                {
                    FormatUrl = string.Format(_endpointConfig.Url, _endpointConfig.MessageType, _endpointConfig.MapId, _endpointConfig.TenantId);
                    queryService = new QueryService(_logger, _httpClientFactory, authService, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));
                    var result = await queryService.GetCiscoSpacesData(stoppingToken);

                    // Process tag data in a separate thread
                    Action processData = () => _tags.UpdateTagCiscoSpacesAPInfo(result["accessPoints"]);
                    await Task.Run(processData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
                _endpointConfig.ApiConnected = false;
                _endpointConfig.Status = EWorkerServiceState.ErrorPullingData;
                var updateCon = _connection.Update(_endpointConfig).Result;
                if (updateCon != null)
                {
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                }
            }
        }
    }
}
