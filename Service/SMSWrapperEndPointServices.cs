using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    public class SMSWrapperEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryTagsRepository _tags;

        public SMSWrapperEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, IInMemoryTagsRepository tags)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection)
        {
            _tags = tags;
        }

        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                IQueryService queryService;
                string FormatUrl = string.Format(_endpointConfig.Url, _endpointConfig.MessageType, _configuration[key: "ApplicationConfiguration:NassCode"]);
                queryService = new QueryService(_logger, _httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));
                var result = await queryService.GetSMSWrapperData(stoppingToken);

                if (_endpointConfig.MessageType.ToLower() == "FDBIDEmployeeList".ToLower())
                {
                    // Process tag data in a separate thread
                    _ = Task.Run(async () => await ProcessFDBIDEmployeeListData(result), stoppingToken).ConfigureAwait(false);
                }
                if (_endpointConfig.MessageType.ToLower() == "NASSCodeEmployeeList".ToLower())
                {
                    // Process tag data in a separate thread
                    _ = Task.Run(async () => await ProcessFDBIDEmployeeListData(result), stoppingToken).ConfigureAwait(false);
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

        private async Task ProcessFDBIDEmployeeListData(JToken result)
        {
            try
            {
                if (result is not null)
                {
                    await Task.Run(() => _tags.UpdateEmployeeInfo(result)).ConfigureAwait(false);

                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}

