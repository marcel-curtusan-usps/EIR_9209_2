using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{

    /// <summary>
    /// This class is responsible for fetching data from the MPEWatch endpoint and processing it.
    /// It inherits from the BaseEndpointService class and implements the FetchDataFromEndpoint method.
    /// </summary>
    public class MPEWatchEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryGeoZonesRepository _geoZones;
        /// <summary>
        /// Initializes a new instance of the <see cref="MPEWatchEndPointServices"/> class.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="endpointConfig"></param>
        /// <param name="configuration"></param>
        /// <param name="hubContext"></param>
        /// <param name="connection"></param>
        /// <param name="loggerService"></param>
        /// <param name="geozone"></param>
        public MPEWatchEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, ILoggerService loggerService, IInMemoryGeoZonesRepository geozone)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection, loggerService)
        {
            _geoZones = geozone;
        }
        /// <summary>
        /// Fetches data from the endpoint and processes it.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                IQueryService queryService;

                var MpeWatchSetting = _configuration.GetSection("MpeWatch");
                var MpeWatchSettingRequestId = MpeWatchSetting.GetSection("RequestId");
                _ = int.TryParse(MpeWatchSettingRequestId.Value, out int MpeWatchId);

                string server = string.IsNullOrEmpty(_endpointConfig.IpAddress) ? _endpointConfig.Hostname : _endpointConfig.IpAddress;
                string start_time = string.Concat(DateTime.Now.AddHours(-_endpointConfig.HoursBack).ToString("MM/dd/yyyy_"), "00:00:00");
                string end_time = string.Concat(DateTime.Now.AddHours(_endpointConfig.HoursForward).ToString("MM/dd/yyyy_"), "23:59:59");
                if (MpeWatchId == 0)
                {
                    string url = string.Format(_endpointConfig.OAuthUrl, server, "", "", start_time, end_time);
                    queryService = new QueryService(_loggerService, _httpClientFactory, jsonSettings, new QueryServiceSettings(
                        new Uri(url),
                        new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)
                    ));
                    var reqiestId = await queryService.GetMPEWatchRequestId(stoppingToken);
                    if (reqiestId != null)
                    {
                        int RequestId = 0;
                        int.TryParse(reqiestId.id, out RequestId);
                        MpeWatchId = RequestId;
                        _configuration["MpeWatch:RequestId"] = RequestId.ToString();
                    }
                }

                string FormatUrl = string.Format(_endpointConfig.Url, server, MpeWatchId, _endpointConfig.MessageType, start_time, end_time);

                queryService = new QueryService(_loggerService, _httpClientFactory, jsonSettings, new QueryServiceSettings(
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
                    _endpointConfig.Status = EWorkerServiceState.Idle;
                    var updateCon = _connection.Update(_endpointConfig).Result;
                    if (updateCon != null)
                    {
                        await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
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
                    _endpointConfig.Status = EWorkerServiceState.Idle;
                    var updateCon = _connection.Update(_endpointConfig).Result;
                    if (updateCon != null)
                    {
                        await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
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
                    _endpointConfig.Status = EWorkerServiceState.Idle;
                    var updateCon = _connection.Update(_endpointConfig).Result;
                    if (updateCon != null)
                    {
                        await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                    }
                    // Process MPE data in a separate thread
                    await ProcessMPEWatchDPSRunData(result, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                await _loggerService.LogData(JToken.FromObject(ex.Message), "Error", "FetchDataFromEndpoint", _endpointConfig.Url);
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
                await _loggerService.LogData(JToken.FromObject(e.Message), "Error", "ProcessMPEWatchRpgPlanData", _endpointConfig.Url);
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
                await _loggerService.LogData(JToken.FromObject(e.Message), "Error", "ProcessMPEWatchRpgPlanData", _endpointConfig.Url);
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
                        var jsonSettings = new JsonSerializerSettings();
                        jsonSettings.Converters.Add(new MPEWatchRunPerformanceConverter());
                        List<MPERunPerformance>? mPERunPerformances = JsonConvert.DeserializeObject<List<MPERunPerformance>>(data.ToString(), jsonSettings);

                        if (mPERunPerformances != null && mPERunPerformances.Any())
                        {
                            await _geoZones.UpdateMPERunInfo(mPERunPerformances, stoppingToken);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                await _loggerService.LogData(JToken.FromObject(e.Message), "Error", "ProcessMPEWatchRunPerfData", _endpointConfig.Url);
            }
        }
        public class MPEWatchRunPerformance
        {
            [JsonProperty("id")]
            public string ZoneId { get; set; } = "";

            [JsonProperty("mpe_type")]
            public string MpeType { get; set; } = "";

            [JsonProperty("mpe_number")]
            public string MpeNumber { get; set; } = "";

            [JsonProperty("bins")]
            public string Bins { get; set; } = "";

            [JsonProperty("cur_sortplan")]
            public string CurSortplan { get; set; } = "";

            [JsonProperty("cur_thruput_ophr")]
            public string CurThruputOphr { get; set; } = "";

            [JsonProperty("tot_sortplan_vol")]
            public string TotSortplanVol { get; set; } = "";

            [JsonProperty("rpg_est_vol")]
            public string RpgEstVol { get; set; } = "";

            [JsonProperty("act_vol_plan_vol_nbr")]
            public string ActVolPlanVolNbr { get; set; } = "";

            [JsonProperty("current_run_start")]
            public string CurrentRunStart { get; set; } = "";

            [JsonProperty("current_run_end")]
            public string CurrentRunEnd { get; set; } = "";

            [JsonProperty("cur_operation_id")]
            public string CurOperationId { get; set; } = "";

            [JsonProperty("bin_full_status")]
            public string BinFullStatus { get; set; } = "";

            [JsonProperty("bin_full_bins")]
            public string BinFullBins { get; set; } = "";

            [JsonProperty("throughput_status")]
            public string ThroughputStatus { get; set; } = "";

            [JsonProperty("unplan_maint_sp_status")]
            public string UnplanMaintSpStatus { get; set; } = "";

            [JsonProperty("op_started_late_status")]
            public string OpStartedLateStatus { get; set; } = "";

            [JsonProperty("op_running_late_status")]
            public string OpRunningLateStatus { get; set; } = "";

            [JsonProperty("sortplan_wrong_status")]
            public string SortplanWrongStatus { get; set; } = "";

            [JsonProperty("unplan_maint_sp_timer")]
            public string UnplanMaintSpTimer { get; set; } = "";

            [JsonProperty("op_started_late_timer")]
            public string OpStartedLateTimer { get; set; } = "";

            [JsonProperty("op_running_late_timer")]
            public string OpRunningLateTimer { get; set; } = "";

            [JsonProperty("rpg_start_dtm")]
            public string RPGStartDtm { get; set; } = "";

            [JsonProperty("rpg_end_dtm")]
            public string RPGEndDtm { get; set; } = "";

            [JsonProperty("expected_throughput")]
            public string ExpectedThroughput { get; set; } = "";

            [JsonProperty("sortplan_wrong_timer")]
            public string SortplanWrongTimer { get; set; } = "";

            [JsonProperty("rpg_est_comp_time")]
            public string RpgEstCompTime { get; set; } = "";
            public DateTime RpgEstimatedCompletion { get; set; } = DateTime.MinValue;

            [JsonProperty("hourly_data")]
            public List<HourlyData> HourlyData { get; set; } = new List<HourlyData>();

            [JsonProperty("rpg_expected_thruput")]
            public string RpgExpectedThruput { get; set; } = "";

            [JsonProperty("ars_recrej3")]
            public string ArsRecrej3 { get; set; } = "";

            [JsonProperty("sweep_recrej3")]
            public string SweepRecrej3 { get; set; } = "";
        }
        public class mpeWatchHourlyData
        {
            [JsonProperty("hour")]
            public DateTime Hour { get; set; } = DateTime.MinValue;

            [JsonProperty("count")]
            public int Count { get; set; } = 0;
            public int Sorted { get; set; } = 0;
            public int Rejected { get; set; } = 0;
        }
    }

    internal class MPEWatchRunPerformanceConverter : JsonConverter<MPERunPerformance>
    {
        public override MPERunPerformance ReadJson(JsonReader reader, Type objectType, MPERunPerformance existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            return new MPERunPerformance
            {
                ZoneId = (string)obj["id"],
                MpeType = (string)obj["mpe_type"],
                MpeNumber = (string)obj["mpe_number"],
                Bins = (string)obj["bins"],
                CurSortplan = (string)obj["cur_sortplan"],
                CurThruputOphr = (string)obj["cur_thruput_ophr"],
                TotSortplanVol = (string)obj["tot_sortplan_vol"],
                RpgEstVol = (string)obj["rpg_est_vol"],
                ActVolPlanVolNbr = (string)obj["act_vol_plan_vol_nbr"],
                CurrentRunStart = (string)obj["current_run_start"],
                CurrentRunEnd = (string)obj["current_run_end"],
                CurOperationId = (string)obj["cur_operation_id"],
                BinFullStatus = (string)obj["bin_full_status"],
                BinFullBins = (string)obj["bin_full_bins"],
                ThroughputStatus = (string)obj["throughput_status"],
                UnplanMaintSpStatus = (string)obj["unplan_maint_sp_status"],
                OpStartedLateStatus = (string)obj["op_started_late_status"],
                OpRunningLateStatus = (string)obj["op_running_late_status"],
                SortplanWrongStatus = (string)obj["sortplan_wrong_status"],
                UnplanMaintSpTimer = (string)obj["unplan_maint_sp_timer"],
                OpStartedLateTimer = (string)obj["op_started_late_timer"],
                OpRunningLateTimer = (string)obj["op_running_late_timer"],
                RPGStartDtm = (string)obj["rpg_start_dtm"],
                RPGEndDtm = (string)obj["rpg_end_dtm"],
                HourlyData = obj["hourly_data"].ToObject<List<HourlyData>>(),
                SortplanWrongTimer = (string)obj["sortplan_wrong_timer"],
                RpgEstCompTime = (string)obj["rpg_est_comp_time"],
                RpgExpectedThruput = (string)obj["rpg_expected_thruput"],
                ArsRecrej3 = (string)obj["ars_recrej3"],
                SweepRecrej3 = (string)obj["sweep_recrej3"]
            };
        }

        public override void WriteJson(JsonWriter writer, MPERunPerformance value, JsonSerializer serializer)
        {
            JObject obj = new JObject
        {
            { "id", value.ZoneId },
            { "mpe_type", value.MpeType },
            { "mpe_number", value.MpeNumber },
            { "bins", value.Bins },
            { "cur_sortplan", value.CurSortplan },
            { "cur_thruput_ophr", value.CurThruputOphr },
            { "tot_sortplan_vol", value.TotSortplanVol },
            { "rpg_est_vol", value.RpgEstVol },
            { "act_vol_plan_vol_nbr", value.ActVolPlanVolNbr },
            { "current_run_start", value.CurrentRunStart },
            { "current_run_end", value.CurrentRunEnd },
            { "cur_operation_id", value.CurOperationId },
            { "bin_full_status", value.BinFullStatus },
            { "bin_full_bins", value.BinFullBins },
            { "throughput_status", value.ThroughputStatus },
            { "unplan_maint_sp_status", value.UnplanMaintSpStatus },
            { "op_started_late_status", value.OpStartedLateStatus },
            { "op_running_late_status", value.OpRunningLateStatus },
            { "sortplan_wrong_status", value.SortplanWrongStatus },
            { "unplan_maint_sp_timer", value.UnplanMaintSpTimer },
            { "op_started_late_timer", value.OpStartedLateTimer },
            { "op_running_late_timer", value.OpRunningLateTimer },
            { "rpg_start_dtm", value.RPGStartDtm },
            { "rpg_end_dtm", value.RPGEndDtm },
            { "sortplan_wrong_timer", value.SortplanWrongTimer },
            { "rpg_est_comp_time", value.RpgEstCompTime },
            { "rpg_estimated_completion", value.RpgEstimatedCompletion },
            { "rpg_expected_thruput", value.RpgExpectedThruput },
            { "ars_recrej3", value.ArsRecrej3 },
            { "sweep_recrej3", value.SweepRecrej3 }
        };
            obj.WriteTo(writer);
        }
    }
}
