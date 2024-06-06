using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Ocsp;
using System.Net.Http;

namespace EIR_9209_2.Service
{
    public class MPEWatchEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryGeoZonesRepository _geoZones;
        public MPEWatchEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IHubContext<HubServices> hubServices, IConfiguration configuration, IInMemoryGeoZonesRepository geozone)
            : base(logger, httpClientFactory, endpointConfig, hubServices, configuration)
        {
            _geoZones = geozone;
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
                string MpeWatch_id = "1";
                string start_time = string.Concat(DateTime.Now.AddHours(-_endpointConfig.HoursBack).ToString("MM/dd/yyyy_"), "00:00:00");
                string end_time = string.Concat(DateTime.Now.AddHours(_endpointConfig.HoursForward).ToString("MM/dd/yyyy_"), "23:59:59");
                FormatUrl = string.Format(_endpointConfig.Url, MpeWatch_id, _endpointConfig.MessageType, start_time, end_time);
                queryService = new QueryService(_httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
                var result = (await queryService.GetMPEWatchData(stoppingToken));
                //process zone data
                if (_endpointConfig.MessageType.ToLower() == "rpg_run_perf")
                {
                    // Process MPE data in a separate thread
                    _ = Task.Run(async () => await ProcessMPEWatchRunPerfData(result), stoppingToken);
                    //_logger.LogInformation("Data from {Url}: {Data}", _endpointConfig.Url, result);
                }
                if (_endpointConfig.MessageType.ToLower() == "rpg_plan")
                {
                    // Process MPE data in a separate thread
                    _ = Task.Run(async () => await ProcessMPEWatchRpgPlanData(result), stoppingToken);
                    //_logger.LogInformation("Data from {Url}: {Data}", _endpointConfig.Url, result);
                }
                if (_endpointConfig.MessageType.ToLower() == "dps_run_estm")
                {
                    // Process MPE data in a separate thread
                    _ = Task.Run(async () => await ProcessMPEWatchDPSRunData(result), stoppingToken);
                    //_logger.LogInformation("Data from {Url}: {Data}", _endpointConfig.Url, result);
                }
                _ = Task.Run(() => _geoZones.RunMPESummaryReport());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
            }
        }
        private async Task ProcessMPEWatchRpgPlanData(JToken result)
        {
            try
            {
                if (result is not null && ((JObject)result).ContainsKey("data"))
                {
                    var data = result.SelectToken("data");
                    if (data != null)
                    {

                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
        }
        private async Task ProcessMPEWatchDPSRunData(JToken result)
        {
            try
            {
                if (result is not null && ((JObject)result).ContainsKey("data"))
                {
                    var data = result.SelectToken("data");
                    if (data != null)
                    {

                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
        }

        private async Task ProcessMPEWatchRunPerfData(JToken result)
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
                            foreach (MPERunPerformance mpe in mpeList)
                            {
                                mpe.MpeId = string.Concat(mpe.MpeType, "-", mpe.MpeNumber.ToString().PadLeft(3, '0'));
                                await Task.Run(() => _geoZones.UpdateMPERunInfo(mpe));
                                await Task.Run(() => _geoZones.UpdateMPERunActivity(mpe));

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
        }
    }
}
