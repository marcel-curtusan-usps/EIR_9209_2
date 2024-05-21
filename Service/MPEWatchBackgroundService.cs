using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    public class MPEWatchBackgroundService : BackgroundService, IMPEWatchBackgroundService
    {
        private readonly ILogger<MPEWatchBackgroundService>? _logger;
        private readonly Dictionary<string, CancellationTokenSource>? _endPointCancellations;
        private readonly Dictionary<string, PeriodicTimer>? _endPointTimers;
        private readonly IHubContext<HubServices> _hubServices;
        private readonly List<Connection>? _endPointList; // List of endpoints
        private readonly IInMemoryConnectionRepository _connections;
        private readonly IInMemoryGeoZonesRepository _geoZones;

        public MPEWatchBackgroundService(ILogger<MPEWatchBackgroundService> logger, IInMemoryConnectionRepository connectionList, IInMemoryGeoZonesRepository geoZoneList, IHubContext<HubServices> hubServices)
        {
            _logger = logger;
            _connections = connectionList;
            _geoZones = geoZoneList;
            _endPointList = [];
            _endPointCancellations = [];
            _endPointTimers = [];
            _hubServices = hubServices;
            (_connections.GetbyType("MPEWatch"))?.ToList().ForEach(endPoint => AddAndStartEndPoint(endPoint));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (_endPointList != null && _endPointList.Any())
                    {
                        foreach (var endPoint in _endPointList)
                        {
                            EWorkerServiceState CurrentStatus = endPoint.Status;
                            if (await _endPointTimers[endPoint.Id].WaitForNextTickAsync(_endPointCancellations[endPoint.Id].Token) && !_endPointCancellations[endPoint.Id].IsCancellationRequested)
                            {
                                try
                                {
                                    endPoint.Status = EWorkerServiceState.Running;
                                    string FormatUrl = "";
                                    string MpeWatch_id = "1";
                                    string start_time = string.Concat(DateTime.Now.AddHours(-endPoint.HoursBack).ToString("MM/dd/yyyy_"), "00:00:00");
                                    string end_time = string.Concat(DateTime.Now.AddHours(endPoint.HoursForward).ToString("MM/dd/yyyy_"), "23:59:59");
                                    using HttpClient _httpClient = new();


                                    IQueryService queryService;
                                    FormatUrl = string.Format(endPoint.Url, MpeWatch_id, endPoint.MessageType, start_time, end_time);
                                    queryService = new QueryService(_httpClient, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
                                    var result = (await queryService.GetMPEWatchData(_endPointCancellations[endPoint.Id].Token));

                                    if (endPoint.MessageType == "rpg_run_perf")
                                    {
                                        //process MPEWatch data
                                        _ = Task.Run(() => ProcessMPEWatchData(result), stoppingToken);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    endPoint.Status = EWorkerServiceState.ErrorPullingData;
                                    _logger.LogError(ex, "Error while starting Pulling from URL Retrying...");
                                }
                                if (_endPointTimers[endPoint.Id].Period.TotalMilliseconds != endPoint.MillisecondsInterval)
                                {
                                    _endPointTimers[endPoint.Id] = new PeriodicTimer(TimeSpan.FromMilliseconds(endPoint.MillisecondsInterval));
                                }
                            }
                            else
                            {
                                endPoint.Status = EWorkerServiceState.Running;
                            }
                        }
                    }
                    else
                    {
                        (_connections.GetbyType("MPEWatch"))?.ToList().ForEach(endPoint => AddAndStartEndPoint(endPoint));
                        await Task.Delay(1000, stoppingToken);
                    }
                }
            }
            catch (Exception e)
            {

                _logger.LogError(e.Message);
            }
        }

        private void ProcessMPEWatchData(JToken result)
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

        public bool IsEndPointRunning(string Id)
        {
            // If the endpoint has a CancellationTokenSource and its token has not been cancelled, the endpoint is running
            return _endPointCancellations.ContainsKey(Id) && !_endPointCancellations[Id].Token.IsCancellationRequested;
        }
        public void RemoveEndPoint(Connection endPoint)
        {
            // If the endpoint is running, stop it
            if (IsEndPointRunning(endPoint.Id))
            {
                StopEndPoint(endPoint.Id);
            }

            // Remove the endpoint from the list
            _endPointList.Remove(endPoint);

            // Remove the endpoint's CancellationTokenSource from the dictionary
            _endPointCancellations.Remove(endPoint.Id);

            // Remove the endpoint's PeriodicTimer for the endpoint
            _endPointTimers.Remove(endPoint.Id);
        }
        public void StartEndPoint(Connection endPoint)
        {
            // If the endpoint is already running, do nothing
            if (!_endPointCancellations[endPoint.Id].IsCancellationRequested)
            {
                return;
            }

            // Create a new CancellationTokenSource for the endpoint
            _endPointCancellations[endPoint.Id] = new CancellationTokenSource();
        }
        public void StopEndPoint(string Id)
        {
            _endPointCancellations[Id].Cancel();
        }
        public void AddAndStartEndPoint(Connection endPoint)
        {
            // Add the endpoint to the list
            _endPointList.Add(endPoint);
            if (endPoint.ActiveConnection)
            {
                // Create a new CancellationTokenSource for the endpoint
                _endPointCancellations[endPoint.Id] = new CancellationTokenSource();

                // Create a new PeriodicTimer for the endpoint
                endPoint.LasttimeApiConnected = DateTime.Now;
                //this will run the endpoint every 100 milliseconds, we want to start the data pull right away then change the interval to the endpoint's interval after it the first pull
                _endPointTimers[endPoint.Id] = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
            }
        }

        public void Start()
        {
            ExecuteAsync(CancellationToken.None);
            Task.FromResult(true);
        }

        public void Stop()
        {
            throw new NotImplementedException();
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

}
