using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    internal class IDSEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryGeoZonesRepository _geoZones;
        public IDSEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, IInMemoryGeoZonesRepository geozone)
                : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection)
        {
            _geoZones = geozone;
        }
        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                IQueryService queryService;
                string FormatUrl = string.Format(_endpointConfig.Url, _endpointConfig.MessageType, _endpointConfig.HoursBack, _endpointConfig.HoursForward);
                queryService = new QueryService(_logger, _httpClientFactory, jsonSettings,
                    new QueryServiceSettings(
                        new Uri(FormatUrl),
                        new TimeSpan(0,0,0,0,_endpointConfig.MillisecondsTimeout)
                    ));
               var result = await queryService.GetIDSData(stoppingToken);
                // Process the data as needed
                await ProcessIDSdata(result);
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

        private async Task ProcessIDSdata(JToken result)
        {
            await _geoZones.ProcessIDSData(result);
        }
    }
}