using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NuGet.Protocol.Core.Types;
using System.Threading;

public class QPEEndpointService
{
    private readonly ILogger<QPEEndpointService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IInMemoryConnectionRepository _connections;
    private readonly IInMemoryTagsRepository _tags;
    private readonly IHubContext<HubServices> _hubServices;
    private readonly Connection _endpointConfig;
    private CancellationTokenSource _cancellationTokenSource;
    private Task _task;

    public QPEEndpointService(ILogger<QPEEndpointService> logger,
        IHttpClientFactory httpClientFactory,
        Connection endpointConfig,
        IInMemoryConnectionRepository connections,
        IInMemoryTagsRepository tags,
        IHubContext<HubServices> hubServices)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _endpointConfig = endpointConfig;
        _connections = connections;
        _hubServices = hubServices;
        _tags = tags;
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
            if (_endpointConfig.MessageType == "getTagData")
            {
                _endpointConfig.Status = EWorkerServiceState.Running;
                _endpointConfig.LasttimeApiConnected = DateTime.Now;
                _endpointConfig.ApiConnected = true;
                _connections.Update(_endpointConfig);
                FormatUrl = string.Format(_endpointConfig.Url, _endpointConfig.MessageType);
                queryService = new QueryService(_httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
                var result = (await queryService.GetQuuppaTagData(stoppingToken));
                // Process tag data in a separate thread
                _ = Task.Run(async () => await ProcessTagMovementData(result), stoppingToken);
                //_logger.LogInformation("Data from {Url}: {Data}", _endpointConfig.Url, result);
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
        }
    }
    private async Task ProcessTagMovementData(QuuppaTag result)
    {
        try
        {
            IInMemoryTagsRepository _tags;
            foreach (Tags qtitem in result.Tags.Where(r => r.LocationTS > 5))
            {
                long posAge = -1;
                qtitem.ServerTS = result.ResponseTS;
                if (qtitem.LocationTS == 0)
                {
                    posAge = -1;
                }
                else
                {
                    posAge = qtitem.ServerTS - qtitem.LocationTS;
                }
                bool visable = posAge > 1 && posAge < 150000 ? true : false;
                ////find tag in the list
                //GeoMarker currentitem = _tags.Get(qtitem.TagId);
                //if (currentitem != null)
                //{
                //    var update = false;
                //    if (currentitem.Geometry.Coordinates != qtitem.Location)
                //    {
                //        currentitem.Geometry.Coordinates = qtitem.Location;
                //        update = true;
                //    }
                //    //check if the tag is visable
                //    if (currentitem.Properties.Visible != visable)
                //    {
                //        currentitem.Properties.Visible = visable;
                //        update = true;
                //    }
                //    //check if the tag is on the same floor
                //    if (currentitem.Properties.FloorId != qtitem.LocationCoordSysId)
                //    {
                //        currentitem.Properties.FloorId = qtitem.LocationCoordSysId;
                //        update = true;
                //    }
                //    //check if tag posAge is different
                //    if (currentitem.Properties.posAge != posAge)
                //    {
                //        currentitem.Properties.posAge = posAge;
                //        update = true;
                //    }
                //    //check if the server timestamp is different
                //    if (currentitem.Properties.ServerTS != qtitem.ServerTS)
                //    {
                //        currentitem.Properties.ServerTS = qtitem.ServerTS;
                //        update = true;
                //    }
                //    if (update)
                //    {
                //        _tags.Update(currentitem);
                //    }

                //}
                //else
                //{
                //    GeoMarker NewMarker = new GeoMarker
                //    {
                //        _id = qtitem.TagId,
                //        Geometry = new MarkerGeometry { Coordinates = qtitem.Location },
                //        Properties = new Marker
                //        {
                //            Id = qtitem.TagId,
                //            FloorId = qtitem.LocationCoordSysId,
                //            ServerTS = qtitem.ServerTS,
                //            posAge = posAge,
                //            Visible = posAge > 1 && posAge < 150000 ? true : false
                //        }
                //    };

                //    _tags.Add(NewMarker);

                //}

                if (qtitem.Location.Any())
                {
                    JObject PositionGeoJson = new JObject
                    {
                        ["type"] = "Feature",
                        ["geometry"] = new JObject
                        {
                            ["type"] = "Point",
                            ["coordinates"] = qtitem.Location.Any() ? new JArray(qtitem.Location[0], qtitem.Location[1]) : new JArray(0, 0)
                        },
                        ["properties"] = new JObject
                        {
                            ["id"] = qtitem.TagId,
                            ["floorId"] = qtitem.LocationCoordSysId,
                            ["posAge"] = posAge,
                            ["visible"] = visable
                        }
                    };

                    await _hubServices.Clients.Group("Tags").SendAsync("tags", PositionGeoJson.ToString());
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