using Microsoft.AspNetCore.SignalR;
using System.Net.Http;

namespace EIR_9209_2.Service
{
    internal class IDSEndPointServices : BaseEndpointService
    {


        public IDSEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IHubContext<HubServices> hubServices)
                : base(logger, httpClientFactory, endpointConfig, hubServices)
        {

        }
        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                if (_endpointConfig.ActiveConnection)
                {
                    IQueryService queryService;
                    string FormatUrl = "";
                    //process tag data

                    _endpointConfig.Status = EWorkerServiceState.Running;
                    _endpointConfig.LasttimeApiConnected = DateTime.Now;
                    _endpointConfig.ApiConnected = true;

                    FormatUrl = string.Format(_endpointConfig.Url, _endpointConfig.MessageType, _endpointConfig.HoursBack, _endpointConfig.HoursForward);
                    queryService = new QueryService(_httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
                    var result = (await queryService.GetIDSData(stoppingToken));
                    await _hubServices.Clients.Group("Connections").SendAsync("UpdateConnection", _endpointConfig);
                }  // Process the data as needed
            }

            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
            }
        }
    }
}