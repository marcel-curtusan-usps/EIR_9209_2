using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    public class MPEWatchEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryGeoZonesRepository _geoZones;

        public MPEWatchEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, ILoggerService loggerService, IInMemoryGeoZonesRepository geozone)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection, loggerService)
        {
            _geoZones = geozone;
        }
        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                IQueryService queryService;
                
                var MpeWatchSetting = _configuration.GetSection("MpeWatch");
                var MpeWatchSettingRequestId = MpeWatchSetting.GetSection("RequestId");
                _ = int.TryParse(MpeWatchSettingRequestId.Value, out int MpeWatchId);

                string server = string.IsNullOrEmpty(_endpointConfig.IpAddress) ? _endpointConfig.Hostname: _endpointConfig.IpAddress;
                string start_time = string.Concat(DateTime.Now.AddHours(-_endpointConfig.HoursBack).ToString("MM/dd/yyyy_"), "00:00:00");
                string end_time = string.Concat(DateTime.Now.AddHours(_endpointConfig.HoursForward).ToString("MM/dd/yyyy_"), "23:59:59");
                if (MpeWatchId == 0)
                {
                    string url = string.Format(_endpointConfig.OAuthUrl, server, "", "", start_time, end_time);
                    queryService = new QueryService(_logger, _httpClientFactory, jsonSettings, new QueryServiceSettings(
                        new Uri(url),
                        new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)
                    ));
                    var reqiestId = await queryService.GetMPEWatchRequestId(stoppingToken);
                    if (reqiestId != null)
                    {
                        int RequestId = 0;
                        int.TryParse(reqiestId.id , out RequestId);
                        MpeWatchId = RequestId;
                        MpeWatchSettingRequestId.Value = reqiestId.id;
                    }
                }

                string FormatUrl = string.Format(_endpointConfig.Url, server, MpeWatchId, _endpointConfig.MessageType, start_time, end_time);

                queryService = new QueryService(_logger, _httpClientFactory, jsonSettings, new QueryServiceSettings(
                        new Uri(FormatUrl),
                        new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)
                    ));
                var result = await queryService.GetMPEWatchData(stoppingToken);
                //process zone data
                if (_endpointConfig.MessageType.Equals("rpg_run_perf", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (_endpointConfig.LogData)
                    {
                        // Start a new thread to handle the logging
                       _ = Task.Run(() => _loggerService.LogData(result,
                            _endpointConfig.MessageType,
                            _endpointConfig.Name,
                            FormatUrl), stoppingToken);
                    }
                    // Process MPE data in a separate thread
                    await ProcessMPEWatchRunPerfData(result, stoppingToken);
                }
                if (_endpointConfig.MessageType.Equals("rpg_plan", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (_endpointConfig.LogData)
                    {
                        // Start a new thread to handle the logging
                        _ = Task.Run(() => _loggerService.LogData(result,
                            _endpointConfig.MessageType,
                            _endpointConfig.Name,
                            FormatUrl), stoppingToken);
                    }
                    // Process MPE data in a separate thread
                    await ProcessMPEWatchRpgPlanData(result, stoppingToken);
                }
                if (_endpointConfig.MessageType.Equals("dps_run_estm", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (_endpointConfig.LogData)
                    {
                        // Start a new thread to handle the logging
                        _ = Task.Run(() => _loggerService.LogData(result,
                            _endpointConfig.MessageType,
                            _endpointConfig.Name,
                            FormatUrl), stoppingToken);
                    }
                    // Process MPE data in a separate thread
                    await ProcessMPEWatchDPSRunData(result, stoppingToken);
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
        private async Task ProcessMPEWatchRpgPlanData(JToken result, CancellationToken stoppingToken)
        {
            try
            {
                if (result is not null && ((JObject)result).ContainsKey("data"))
                {
                    var data = result.SelectToken("data");
                    if (data != null)
                    {
                        await _geoZones.LoadMPEPlan(data, stoppingToken);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
        private async Task ProcessMPEWatchDPSRunData(JToken result, CancellationToken stoppingToken)
        {
            try
            {
                if (result is not null && ((JObject)result).ContainsKey("data"))
                {
                    var data = result.SelectToken("data");
                    if (data != null)
                    {
                       /// await Task.Run(() => _geoZones.LoadMPEPlan(data), stoppingToken).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        private async Task ProcessMPEWatchRunPerfData(JToken result, CancellationToken stoppingToken)
        {
            try
            {
                //loop through the results and process them
                //this is where you would save the data to the database
                //or send it to the front end
                //or do whatever you need to do with the data
                if (result is not null && ((JObject)result).ContainsKey("data"))
                {
                    var data = result.SelectToken("data");
                    if (data != null)
                    {
                        List<MPERunPerformance>? mpeList = data.ToObject<List<MPERunPerformance>>();
                        if (mpeList != null && mpeList.Any())
                        {
                            await _geoZones.UpdateMPERunInfo(mpeList, stoppingToken);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}
