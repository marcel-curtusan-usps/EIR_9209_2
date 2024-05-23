using EIR_9209_2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NuGet.Protocol.Core.Types;
using System.Threading;

public class IDSEndpointService
{
    private readonly ILogger<IDSEndpointService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IInMemoryConnectionRepository _connections;
    private readonly IInMemoryGeoZonesRepository _geoZones;
    private readonly IHubContext<HubServices> _hubServices;
    private readonly Connection _endpointConfig;
    private CancellationTokenSource _cancellationTokenSource;
    private Task _task;

    public IDSEndpointService(ILogger<IDSEndpointService> logger,
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
        _hubServices = hubServices;
        _geoZones = geoZones;
        _cancellationTokenSource = new CancellationTokenSource();
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
            IQueryService queryService;
            string FormatUrl = "";
            //process tag data

            _endpointConfig.Status = EWorkerServiceState.Running;
            _endpointConfig.LasttimeApiConnected = DateTime.Now;
            _endpointConfig.ApiConnected = true;
            _connections.Update(_endpointConfig);
            FormatUrl = string.Format(_endpointConfig.Url, _endpointConfig.MessageType);
            queryService = new QueryService(_httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
            var result = (await queryService.GetIDSData(_endpointConfig.MessageType, _endpointConfig.HoursBack, _endpointConfig.HoursForward, stoppingToken));
            // Process tag data in a separate thread
            _ = Task.Run(() => ProcessIDSData(result), stoppingToken);
            //_logger.LogInformation("Data from {Url}: {Data}", _endpointConfig.Url, result);


        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
        }
    }
    private void ProcessIDSData(JToken result)
    {
        try
        {
            try
            {
                List<string> mpeNames = result.Select(item => item["MPE_NAME"]?.ToString()).Distinct().OrderBy(name => name).ToList();
                foreach (string mpeName in mpeNames)
                {
                    bool pushDBUpdate = false;
                    var geoZone = _geoZones.GetMPEName(mpeName);
                    if (geoZone != null && geoZone.Properties.MPERunPerformance != null)
                    {

                        if (geoZone.Properties.DataSource != "IDS")
                        {
                            geoZone.Properties.DataSource = "IDS";
                            pushDBUpdate = true;
                        }
                        List<string> hourslist = GetListofHours(24);
                        foreach (string hr in hourslist)
                        {
                            var mpeData = result.Where(item => item["MPE_NAME"]?.ToString() == mpeName && item["HOUR"]?.ToString() == hr).FirstOrDefault();
                            if (mpeData != null)
                            {
                                if (geoZone.Properties.MPERunPerformance.HourlyData.Where(h => h.Hour == hr).Any())
                                {
                                    geoZone.Properties.MPERunPerformance.HourlyData.Where(h => h.Hour == hr).ToList().ForEach(h =>
                                    {
                                        if (h.Sorted != (int)mpeData["SORTED"])
                                        {
                                            h.Sorted = (int)mpeData["SORTED"];
                                            pushDBUpdate = true;
                                        }

                                        if (h.Rejected != (int)mpeData["REJECTED"])
                                        {
                                            h.Rejected = (int)mpeData["REJECTED"];
                                            pushDBUpdate = true;
                                        }
                                        if (h.Count != (int)mpeData["INDUCTED"])
                                        {
                                            h.Count = (int)mpeData["INDUCTED"];
                                            pushDBUpdate = true;
                                        }

                                    });
                                }
                                else
                                {
                                    geoZone.Properties.MPERunPerformance.HourlyData.Add(new HourlyData
                                    {
                                        Hour = hr,
                                        Sorted = (int)mpeData["SORTED"],
                                        Rejected = (int)mpeData["REJECTED"],
                                        Count = (int)mpeData["INDUCTED"]
                                    });
                                    pushDBUpdate = true;
                                }
                            }
                            else
                            {
                                if (geoZone.Properties.MPERunPerformance.HourlyData.Where(h => h.Hour == hr).Any())
                                {
                                    geoZone.Properties.MPERunPerformance.HourlyData.Where(h => h.Hour == hr).ToList().ForEach(h =>
                                    {
                                        h.Sorted = 0;
                                        h.Rejected = 0;
                                        h.Count = 0;
                                    });
                                }
                                else
                                {
                                    geoZone.Properties.MPERunPerformance.HourlyData.Add(new HourlyData
                                    {
                                        Hour = hr,
                                        Sorted = 0,
                                        Rejected = 0,
                                        Count = 0
                                    });
                                }
                            }
                        }
                        if (pushDBUpdate)
                        {
                            _geoZones.Update(geoZone);
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error Processing data from");
            }

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
    }
    private List<string> GetListofHours(int hours)
    {
        var localTime = DateTime.Now;
        return Enumerable.Range(0, hours).Select(i => localTime.AddHours(-23).AddHours(i).ToString("yyyy-MM-dd HH:00")).ToList();
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