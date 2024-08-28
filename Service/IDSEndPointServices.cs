using Microsoft.AspNetCore.SignalR;

namespace EIR_9209_2.Service
{
    internal class IDSEndPointServices : BaseEndpointService
    {

        public IDSEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection)
                : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection)
        {

        }
        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                _endpointConfig.Status = EWorkerServiceState.Running;
                _endpointConfig.LasttimeApiConnected = DateTime.Now;
                _endpointConfig.ApiConnected = true;
                await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", _endpointConfig, cancellationToken: stoppingToken);

                IQueryService queryService;
                string FormatUrl = "";
                //process tag data
                FormatUrl = string.Format(_endpointConfig.Url, _endpointConfig.MessageType, _endpointConfig.HoursBack, _endpointConfig.HoursForward);
                queryService = new QueryService(_logger, _httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
                await queryService.GetIDSData(stoppingToken);
                // Process the data as needed
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
                _endpointConfig.ApiConnected = false;
                _endpointConfig.Status = EWorkerServiceState.ErrorPullingData;
                var updateCon = _connection.Update(_endpointConfig).Result;
                if (updateCon != null)
                {
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, cancellationToken: stoppingToken);
                }
            }
        }
    }
}