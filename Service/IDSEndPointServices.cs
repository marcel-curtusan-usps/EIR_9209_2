using EIR_9209_2.DatabaseCalls.IDS;
using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
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

                // string server = string.IsNullOrEmpty(_endpointConfig.IpAddress) ? _endpointConfig.Hostname : _endpointConfig.IpAddress;
                // IOAuth2AuthenticationService authService;
                // authService = new OAuth2AuthenticationService(_logger, _httpClientFactory, new OAuth2AuthenticationServiceSettings(server, "", _endpointConfig.OAuthUserName, _endpointConfig.OAuthPassword, _endpointConfig.OAuthClientId, "", _endpointConfig.AuthType), jsonSettings);
                // IQueryService queryService;
                // string FormatUrl = string.Format(_endpointConfig.Url, server, _endpointConfig.MessageType, _endpointConfig.HoursBack, _endpointConfig.HoursForward);
                // queryService = new QueryService(_logger, _httpClientFactory, authService, jsonSettings,
                //     new QueryServiceSettings(
                //         new Uri(FormatUrl),
                //         new TimeSpan(0,0,0,0,_endpointConfig.MillisecondsTimeout)
                //     ));
                //var result = await queryService.GetIDSData(stoppingToken);
                JObject data = new JObject
                {
                    ["startHour"] = _endpointConfig.HoursBack,
                    ["endHour"] = _endpointConfig.HoursForward,
                    ["queryName"] = _endpointConfig.MessageType
                };

                JToken result = await _ids.GetOracleIDSData(data);
                if (result.HasValues)
                {
                    await ProcessIDSdata(result, stoppingToken);
                }
                else
                {
                    if (((JObject)result).ContainsKey("Error"))
                    {
                        _logger.LogError($"Error fetching data from IDS{result}");

                    }
                    else
                    {
                        _logger.LogInformation($"Fetched data from IDS");
                    }
                }


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
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                }
            }
        }

        private async Task ProcessIDSdata(JToken result, CancellationToken stoppingToken)
        {
            await Task.Run(() => _geoZones.ProcessIDSData(result), stoppingToken).ConfigureAwait(false);
        }
    }
}