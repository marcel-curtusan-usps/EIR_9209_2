using Microsoft.AspNetCore.SignalR;

namespace EIR_9209_2.Service
{
    public class QPEEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryTagsRepository _tags;

        public QPEEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, IInMemoryTagsRepository tags)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection)
        {
            _tags = tags;
        }

        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                IQueryService queryService;

                _endpointConfig.Status = EWorkerServiceState.Running;
                _endpointConfig.LasttimeApiConnected = DateTime.Now;
                _endpointConfig.ApiConnected = true;

                await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", _endpointConfig, cancellationToken: stoppingToken);
                //process tag data
                string FormatUrl = "";
                if (_endpointConfig.MessageType == "getTagData")
                {
                    FormatUrl = string.Format(_endpointConfig.Url, _endpointConfig.MessageType);
                    queryService = new QueryService(_logger, _httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
                    var result = (await queryService.GetQPETagData(stoppingToken));

                    // Process tag data in a separate thread
                    //await ProcessTagMovementData(result);
                    Action processData = () => _tags.UpdateTagQPEInfo(result.Tags);
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
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, cancellationToken: stoppingToken);
                }
            }
        }
    }
}
