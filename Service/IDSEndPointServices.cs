using EIR_9209_2.DatabaseCalls.IDS;
using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EIR_9209_2.Service
{
    internal class IDSEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryGeoZonesRepository _geoZones; 
        private readonly IIDS _ids;
        public IDSEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, ILoggerService loggerService, IInMemoryGeoZonesRepository geozone, IIDS ids)
                : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection, loggerService)
        {
            _geoZones = geozone;
            _ids = ids;
        }
        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                JObject data = new JObject
                {
                    ["startHour"] = _endpointConfig.HoursBack,
                    ["endHour"] = _endpointConfig.HoursForward,
                    ["queryName"] = _endpointConfig.MessageType
                };

                JToken result = await _ids.GetOracleIDSData(data);
                if (_endpointConfig.LogData)
                {
                    // Start a new thread to handle the logging
                    _ = Task.Run(() => _loggerService.LogData(result.ToJson(),
                        _endpointConfig.MessageType,
                        _endpointConfig.Name,
                        data.ToString()), stoppingToken);
                }
                if (result.HasValues)
                {
                    if (result is JObject resultObject && resultObject.ContainsKey("Error"))
                    {
                        _logger.LogError($"Error fetching data from IDS{result}");

                        _endpointConfig.Status = EWorkerServiceState.ErrorPullingData;
                        var updateCon = _connection.Update(_endpointConfig).Result;
                        if (updateCon != null)
                        {
                            await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"Fetched data from IDS");

                        _endpointConfig.Status = EWorkerServiceState.Idel;
                        var updateCon = _connection.Update(_endpointConfig).Result;
                        if (updateCon != null)
                        {
                            await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                        }
                        await ProcessIDSdata(result, stoppingToken);
                    }
               
                }
                else
                {
                    _logger.LogError($"Error fetching data from IDS{result}");                   
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

        private async Task ProcessIDSdata(JToken result, CancellationToken stoppingToken)
        {
           await _geoZones.ProcessIDSData(result, stoppingToken);
        }
    }
}