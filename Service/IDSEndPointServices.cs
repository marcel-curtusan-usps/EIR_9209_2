using Microsoft.AspNetCore.SignalR;
using System.Net.Http;

namespace EIR_9209_2.Service
{
    internal class IDSEndPointServices : BaseEndpointService
    {

        public IDSEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IInMemoryConnectionRepository connection)
                : base(logger, httpClientFactory, endpointConfig, configuration, connection)
        {

        }
        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                _endpointConfig.Status = EWorkerServiceState.Running;
                _endpointConfig.LasttimeApiConnected = DateTime.Now;
                if (_endpointConfig.ActiveConnection)
                {
                    _endpointConfig.ApiConnected = true;
                }
                else
                {
                    _endpointConfig.ApiConnected = false;
                    _endpointConfig.Status = EWorkerServiceState.Idel;
                }
                await _connection.Update(_endpointConfig);
                IQueryService queryService;
                string FormatUrl = "";
                //process tag data
                FormatUrl = string.Format(_endpointConfig.Url, _endpointConfig.MessageType, _endpointConfig.HoursBack, _endpointConfig.HoursForward);
                queryService = new QueryService(_httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
                await queryService.GetIDSData(stoppingToken);
                // Process the data as needed
            }

            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
            }
        }
    }
}