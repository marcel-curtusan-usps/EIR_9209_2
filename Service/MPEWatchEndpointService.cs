using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using NuGet.Protocol.Core.Types;
using System.Threading;

internal class MPEWatchEndpointService
{
    private readonly ILogger<MPEWatchEndpointService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Connection _endpointConfig;
    private readonly IInMemoryConnectionRepository _connections;
    private readonly IInMemoryGeoZonesRepository _geoZones;
    private readonly IHubContext<HubServices> _hubServices;
    private CancellationTokenSource _cancellationTokenSource;
    private Task _task;

    public MPEWatchEndpointService(ILogger<MPEWatchEndpointService> logger,
        IHttpClientFactory httpClientFactory,
        Connection endpointConfig,
        IInMemoryConnectionRepository connections,
        IInMemoryGeoZonesRepository geoZones,
        IHubContext<HubServices> hubServices)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _endpointConfig = endpointConfig;
        _connections = connections;
        _geoZones = geoZones;
        _hubServices = hubServices;
    }
    public void Start()
    {
        if (_task == null || _task.IsCompleted)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Run(async () => await RunAsync(_cancellationTokenSource.Token));
        }
    }

    public void Stop()
    {
        if (_task != null && !_task.IsCompleted)
        {
            _cancellationTokenSource.Cancel();
        }
    }
    public void UpdateInterval(long newIntervalSeconds)
    {
        Stop();
        _endpointConfig.MillisecondsInterval = newIntervalSeconds;
        Start();
    }
    public void UpdateActive(bool Active)
    {
        if (_endpointConfig.ActiveConnection != Active && Active)
        {
            Start();
            _endpointConfig.ActiveConnection = Active;
        }
        if (_endpointConfig.ActiveConnection != Active && !Active)
        {
            Stop();
            _endpointConfig.ActiveConnection = Active;
        }
    }
    private async Task RunAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await FetchDataFromEndpoint(stoppingToken);
                if (timer.Period.TotalMilliseconds != _endpointConfig.MillisecondsInterval)
                {
                    timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_endpointConfig.MillisecondsInterval));
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Stopping data collection for {Url}", _endpointConfig.Url);
        }
        finally
        {
            timer.Dispose();
        }
    }

    private async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
    {
        try
        {
            //var client = _httpClientFactory.CreateClient();
            //string FormatUrl = string.Format(_endpointConfig.Url, _endpointConfig.MessageType);
            //var response = await client.GetAsync(FormatUrl, stoppingToken);

            //response.EnsureSuccessStatusCode();
            //var content = await response.Content.ReadAsStringAsync(stoppingToken);

            IQueryService queryService;
            string FormatUrl = "";
            _endpointConfig.Status = EWorkerServiceState.Running;
            _endpointConfig.LasttimeApiConnected = DateTime.Now;
            _connections.Update(_endpointConfig);
            string MpeWatch_id = "1";
            string start_time = string.Concat(DateTime.Now.AddHours(-_endpointConfig.HoursBack).ToString("MM/dd/yyyy_"), "00:00:00");
            string end_time = string.Concat(DateTime.Now.AddHours(_endpointConfig.HoursForward).ToString("MM/dd/yyyy_"), "23:59:59");
            FormatUrl = string.Format(_endpointConfig.Url, MpeWatch_id, _endpointConfig.MessageType, start_time, end_time);
            queryService = new QueryService(_httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
            var result = (await queryService.GetMPEWatchData(stoppingToken));
            //process zone data
            if (_endpointConfig.MessageType.ToLower() == "rpg_run_perf")
            {
                // Process zone data in a separate thread
                _ = Task.Run(async () => await ProcessMPEWatchRunPerfData(result), stoppingToken);
                //_logger.LogInformation("Data from {Url}: {Data}", _endpointConfig.Url, result);
            }
            if (_endpointConfig.MessageType.ToLower() == "rpg_plan")
            {
                // Process zone data in a separate thread
                _ = Task.Run(async () => await ProcessMPEWatchRpgPlanData(result), stoppingToken);
                //_logger.LogInformation("Data from {Url}: {Data}", _endpointConfig.Url, result);
            }
            if (_endpointConfig.MessageType.ToLower() == "dps_run_estm")
            {
                // Process zone data in a separate thread
                _ = Task.Run(async () => await ProcessMPEWatchDPSRunData(result), stoppingToken);
                //_logger.LogInformation("Data from {Url}: {Data}", _endpointConfig.Url, result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
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
            _logger.LogError(e.Message);
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
            _logger.LogError(e.Message);
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
                                _hubServices.Clients.Group("MPEZones").SendAsync("UpdateGeoZone", geoZone);
                            }
                            _geoZones.Update(geoZone);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
    }

    internal static JsonSerializerSettings jsonSettings = new()
    {
        //keep this
        Error = (sender, args) =>
        {
            //in fotf this should log to an actual log file for diagnostics
            Console.WriteLine($"Json Error: {args.ErrorContext.Error.Message} at path [{args.ErrorContext.Path}] " +
                $"on original object:{Environment.NewLine}{JsonConvert.SerializeObject(args.CurrentObject)}");
            //keep this
            args.ErrorContext.Handled = true;
        },
        //keep this
        ContractResolver = new DefaultContractResolver
        {
            //keep this
            NamingStrategy = new CamelCaseNamingStrategy()
        }
    };
}