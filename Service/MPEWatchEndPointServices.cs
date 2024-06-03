using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
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
                IQueryService queryService;
                string FormatUrl = "";
                _endpointConfig.Status = EWorkerServiceState.Running;
                _endpointConfig.LasttimeApiConnected = DateTime.Now;
                _endpointConfig.ApiConnected = true;
                string MpeWatch_id = "1";
                string start_time = string.Concat(DateTime.Now.AddHours(-_endpointConfig.HoursBack).ToString("MM/dd/yyyy_"), "00:00:00");
                string end_time = string.Concat(DateTime.Now.AddHours(_endpointConfig.HoursForward).ToString("MM/dd/yyyy_"), "23:59:59");
                FormatUrl = string.Format(_endpointConfig.Url, MpeWatch_id, _endpointConfig.MessageType, start_time, end_time);
                queryService = new QueryService(_httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
                var result = (await queryService.GetMPEWatchData(stoppingToken));
                await _hubServices.Clients.Group("Connections").SendAsync("UpdateConnection", _endpointConfig);
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
                        var mpeList = data.ToObject<List<MPERunPerformance>>();

                        foreach (var mpe in mpeList)
                        {
                            var mpeName = string.Concat(mpe.MpeType, "-", mpe.MpeNumber.ToString().PadLeft(3, '0'));
                            // get geozone that matched the mpe watch id
                            var geoZone = _geoZones.GetMPEName(mpeName);
                            if (geoZone != null)
                            {
                                bool pushUIUpdate = false;
                                //update the geozone with the new data
                                //geoZone.Properties.MPERunPerformance = mpe;
                                //check  mpe run performance data and update the geozone
                                if (geoZone.Properties.MPERunPerformance.ZoneId != geoZone.Properties.Id)
                                {
                                    geoZone.Properties.MPERunPerformance.ZoneId = geoZone.Properties.Id;
                                }
                                if (geoZone.Properties.DataSource != "IDS")
                                {
                                    if (geoZone.Properties.MPERunPerformance.HourlyData != mpe.HourlyData)
                                    {
                                        geoZone.Properties.MPERunPerformance.HourlyData = mpe.HourlyData;
                                        pushUIUpdate = true;
                                    }
                                }
                                if (geoZone.Properties.MPERunPerformance.CurSortplan != mpe.CurSortplan)
                                {
                                    geoZone.Properties.MPERunPerformance.CurSortplan = mpe.CurSortplan;
                                    pushUIUpdate = true;

                                }
                                if (geoZone.Properties.MPERunPerformance.CurThruputOphr != mpe.CurThruputOphr)
                                {
                                    geoZone.Properties.MPERunPerformance.CurThruputOphr = mpe.CurThruputOphr;
                                    pushUIUpdate = true;
                                }
                                if (geoZone.Properties.MPERunPerformance.TotSortplanVol != mpe.TotSortplanVol)
                                {
                                    geoZone.Properties.MPERunPerformance.TotSortplanVol = mpe.TotSortplanVol;
                                    pushUIUpdate = true;
                                }

                                if (geoZone.Properties.MPERunPerformance.CurrentRunStart != mpe.CurrentRunStart)
                                {
                                    geoZone.Properties.MPERunPerformance.CurrentRunStart = mpe.CurrentRunStart;
                                    pushUIUpdate = true;
                                }
                                if (geoZone.Properties.MPERunPerformance.CurrentRunEnd != mpe.CurrentRunEnd)
                                {
                                    geoZone.Properties.MPERunPerformance.CurrentRunEnd = mpe.CurrentRunEnd;
                                    pushUIUpdate = true;
                                }
                                if (geoZone.Properties.MPERunPerformance.CurOperationId != mpe.CurOperationId)
                                {
                                    geoZone.Properties.MPERunPerformance.CurOperationId = mpe.CurOperationId;
                                    pushUIUpdate = true;
                                }

                                if (geoZone.Properties.MPERunPerformance.UnplanMaintSpStatus != mpe.UnplanMaintSpStatus)
                                {
                                    geoZone.Properties.MPERunPerformance.UnplanMaintSpStatus = mpe.UnplanMaintSpStatus;
                                    pushUIUpdate = true;
                                }
                                if (geoZone.Properties.MPERunPerformance.OpRunningLateStatus != mpe.OpRunningLateStatus)
                                {
                                    geoZone.Properties.MPERunPerformance.OpRunningLateStatus = mpe.OpRunningLateStatus;
                                    pushUIUpdate = true;
                                }
                                if (geoZone.Properties.MPERunPerformance.OpRunningLateTimer != mpe.OpRunningLateTimer)
                                {
                                    geoZone.Properties.MPERunPerformance.OpRunningLateTimer = mpe.OpRunningLateTimer;
                                    pushUIUpdate = true;
                                }
                                if (geoZone.Properties.MPERunPerformance.SortplanWrongStatus != mpe.SortplanWrongStatus)
                                {
                                    geoZone.Properties.MPERunPerformance.SortplanWrongStatus = mpe.SortplanWrongStatus;
                                    pushUIUpdate = true;
                                }
                                if (geoZone.Properties.MPERunPerformance.OpStartedLateStatus != mpe.OpStartedLateStatus)
                                {
                                    geoZone.Properties.MPERunPerformance.OpStartedLateStatus = mpe.OpStartedLateStatus;
                                    pushUIUpdate = true;
                                }
                                if (geoZone.Properties.MPERunPerformance.ThroughputStatus != mpe.ThroughputStatus)
                                {
                                    geoZone.Properties.MPERunPerformance.ThroughputStatus = mpe.ThroughputStatus;
                                    pushUIUpdate = true;
                                }
                                if (geoZone.Properties.MPERunPerformance.UnplanMaintSpTimer != mpe.UnplanMaintSpTimer)
                                {
                                    geoZone.Properties.MPERunPerformance.UnplanMaintSpTimer = mpe.UnplanMaintSpTimer;
                                    pushUIUpdate = true;
                                }
                                if (geoZone.Properties.MPERunPerformance.BinFullStatus != mpe.BinFullStatus)
                                {
                                    geoZone.Properties.MPERunPerformance.BinFullStatus = mpe.BinFullStatus;
                                    pushUIUpdate = true;
                                }
                                if (geoZone.Properties.MPERunPerformance.BinFullBins != mpe.BinFullBins)
                                {
                                    geoZone.Properties.MPERunPerformance.BinFullBins = mpe.BinFullBins;
                                    pushUIUpdate = true;
                                }

                                if (pushUIUpdate)
                                {
                                    await _hubServices.Clients.Group("MPEZones").SendAsync("MPEPerformanceUpdateGeoZone", geoZone.Properties.MPERunPerformance);
                                    _geoZones.Update(geoZone);
                                }

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
