using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace EIR_9209_2.Service
{
    public class SMSWrapperEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryTagsRepository _tags;

        public SMSWrapperEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IHubContext<HubServices> hubServices, IConfiguration configuration, IInMemoryTagsRepository tags)
            : base(logger, httpClientFactory, endpointConfig, hubServices, configuration)
        {
            _tags = tags;
        }

        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                if (_endpointConfig.Status != EWorkerServiceState.Running)
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

                    await _hubServices.Clients.Group("Connections").SendAsync("UpdateConnection", _endpointConfig);
                }
                IQueryService queryService;
                string FormatUrl = "";
                FormatUrl = string.Format(_endpointConfig.Url, _endpointConfig.MessageType, _configuration[key: "ApplicationConfiguration:NassCode"]);
                queryService = new QueryService(_httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
                var result = (await queryService.GetSMSWrapperData(stoppingToken));

                if (_endpointConfig.MessageType.ToLower() == "FDBIDEmployeeList".ToLower())
                {
                    // Process tag data in a separate thread
                    _ = Task.Run(async () => await ProcessFDBIDEmployeeListData(result), stoppingToken);
                }
                if (_endpointConfig.MessageType.ToLower() == "NASSCodeEmployeeList".ToLower())
                {
                    // Process tag data in a separate thread
                    _ = Task.Run(async () => await ProcessFDBIDEmployeeListData(result), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
            }
        }

        private async Task ProcessFDBIDEmployeeListData(JToken result)
        {
            try
            {
                if (result is not null)
                {
                    await Task.Run(() => _tags.UpdateEmployeeInfo(result));

                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
        }
    }
}

