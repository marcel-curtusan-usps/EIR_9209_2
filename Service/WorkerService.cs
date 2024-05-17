using EIR_9209_2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using static EIR_9209_2.Models.GeoMarker;

public class WorkerService : IHostedService, IWorkerService, IDisposable
{
    private readonly ILogger<WorkerService> _logger;
    private readonly HubServices _hubServices;
    private readonly List<Connection> _endPointList;
    private readonly Dictionary<string, CancellationTokenSource> _endPointCancellations;
    private readonly Dictionary<string, PeriodicTimer> _endPointTimers;
    private readonly JsonSerializerSettings _serializerSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Formatting = Formatting.Indented
    };
    private CancellationTokenSource _cts = new();
    private readonly IInMemoryConnectionRepository _connections;
    private readonly IInMemoryTagsRepository _tags;
    private EWorkerServiceState _state;
    public EWorkerServiceState State
    {
        get => _state;
        private set
        {
            if (_state == value) return;
            _state = value;
            _logger.LogInformation("Worker Service changed to state [{NewState}]", value);
        }
    }
    public WorkerService(ILogger<WorkerService> logger, IInMemoryConnectionRepository connectionList, IInMemoryTagsRepository tagList, HubServices hubServices)
    {
        _logger = logger;
        _connections = connectionList;
        _tags = tagList;
        _hubServices = hubServices;
        _endPointList = [];
        _endPointCancellations = [];
        _endPointTimers = [];

    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        StartWorkerService();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {

        // remove endpoint from list 
        _endPointList.ForEach(endPoint => StopEndPoint(endPoint.Id));
        _endPointList.ForEach(endPoint => RemoveEndPoint(endPoint));
        _endPointList.Clear();
        _endPointCancellations.Clear();
        _endPointTimers.Clear();
        _cts.Cancel();
    }
    private void StartWorkerService()
    {
        State = EWorkerServiceState.Starting;
        _cts = new CancellationTokenSource();
        //_connections.GetAll() .ForEach(endPoint => AddAndStartEndPoint(endPoint));
        RunWorkerServiceAsync();
    }

    private void RunWorkerServiceAsync()
    {
        Task.Run(async () =>
        {
            try
            {
                State = EWorkerServiceState.Starting;
                if (_endPointList != null && _endPointList.Any())
                {
                    foreach (var endPoint in _endPointList)
                    {
                        endPoint.Status = EWorkerServiceState.Starting;
                        _endPointTimers[endPoint.Id] = new PeriodicTimer(TimeSpan.FromMilliseconds(endPoint.MillisecondsInterval));
                        _endPointCancellations[endPoint.Id] = new CancellationTokenSource();

                    }
                    while (!_cts.IsCancellationRequested)
                    {
                        try
                        {
                            if (_endPointList.Any())
                            {
                                foreach (var endPoint in _endPointList)
                                {
                                    EWorkerServiceState CurrentStatus = endPoint.Status;
                                    if (!_endPointCancellations[endPoint.Id].IsCancellationRequested && await _endPointTimers[endPoint.Id].WaitForNextTickAsync(_endPointCancellations[endPoint.Id].Token))
                                    {
                                        try
                                        {
                                            endPoint.Status = EWorkerServiceState.Running;

                                            using HttpClient _httpClient = new();

                                            try
                                            {
                                                if (!string.IsNullOrEmpty(endPoint.OAuthUrl))
                                                {
                                                    //oauth2 token
                                                    IOAuth2AuthenticationService authService;
                                                    authService = new OAuth2AuthenticationService(_httpClient, new OAuth2AuthenticationServiceSettings(endPoint.OAuthUrl, endPoint.UserName, endPoint.Password, endPoint.ClientId), jsonSettings);
                                                    IQueryService queryService;
                                                    queryService = new QueryService(_httpClient, authService, jsonSettings, new QueryServiceSettings(new Uri(endPoint.Url)));
                                                    var result = await queryService.GetData(_endPointCancellations[endPoint.Id].Token);
                                                    //process tag data
                                                    if (endPoint.MessageType == "getTagData")
                                                    {
                                                        // Process tag data in a separate thread
                                                        _ = Task.Run(() => ProcessTagMovementData(result));
                                                        _ = Task.Run(() => ProcessTagStorageData(result));
                                                    }
                                                }
                                                else
                                                {
                                                    IQueryService queryService;
                                                    queryService = new QueryService(_httpClient, jsonSettings, new QueryServiceSettings(new Uri(endPoint.Url)));
                                                    var result = (await queryService.GetData(_endPointCancellations[endPoint.Id].Token));
                                                    //process tag data
                                                    if (endPoint.MessageType == "getTagData")
                                                    {
                                                        // Process tag data in a separate thread
                                                        _ = Task.Run(() => ProcessTagMovementData(result));
                                                        // _ = Task.Run(() => ProcessTagStorageData(result));
                                                        // await ProcessTagData(result);
                                                    }
                                                }

                                            }
                                            catch (Exception e)
                                            {
                                                endPoint.Status = EWorkerServiceState.ErrorPullingData;
                                                _logger.LogError(e, "Error Pulling data from URL Retrying...");
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            endPoint.Status = EWorkerServiceState.ErrorPullingData;
                                            _logger.LogError(ex, "Error while starting Pulling from URL Retrying...");
                                        }
                                    }
                                    else
                                    {
                                        endPoint.Status = EWorkerServiceState.Stopped;
                                    }
                                    if (CurrentStatus != endPoint.Status)
                                    {
                                        await _hubServices.SendMessageToGroup("Connections", JsonConvert.SerializeObject(endPoint, _serializerSettings), "connection");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error while looping...");
                        }
                    }
                }
                else
                {
                    (_connections.GetAll())?.ToList().ForEach(endPoint => AddAndStartEndPoint(endPoint));
                    RunWorkerServiceAsync();
                }
            }
            catch (Exception ex)
            {
                State = EWorkerServiceState.Stopping;
                _logger.LogError(ex, "Error while connecting to the Main Application. Retrying in 60 seconds...");

            }
        });

    }
    private async Task ProcessTagMovementData(QuuppaTag result)
    {
        try
        {

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
                try
                {
                    if (visable)
                    {
                        await _hubServices.SendMessageToGroup("Tags", PositionGeoJson.ToString(), "tags");
                    }

                }
                catch (Exception ep)
                {
                    _logger.LogError(ep.Message);
                }
            }

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
    }
    private async Task ProcessTagStorageData(QuuppaTag result)
    {
        try
        {

            foreach (Tags qtitem in result.Tags)
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

                var marker = _tags.Get(qtitem.TagId);
                if (marker != null)
                {
                    marker.Properties.LocationMovementStatus = qtitem.LocationMovementStatus;
                    marker.Properties.PositionTS = qtitem.LocationTS;
                    marker.Properties.ServerTS = qtitem.ServerTS;
                    marker.Properties.posAge = posAge;
                    marker.Properties.Visible = visable;
                    marker.Properties.Color = qtitem.Color;
                    marker.Properties.Zones = qtitem.LocationZoneIds;
                    marker.Properties.ZonesNames = qtitem.LocationZoneNames.ToString();
                    marker.Properties.LocationType = qtitem.LocationType;
                    marker.Geometry.Coordinates = qtitem.Location;
                    _tags.Update(marker);

                }
                else
                {
                    marker = new GeoMarker
                    {
                        _id = qtitem.TagId,
                        Geometry = new MarkerGeometry
                        {
                            Coordinates = qtitem.Location
                        },
                        Properties = new Marker
                        {
                            Id = qtitem.TagId,
                            Name = !string.IsNullOrEmpty(qtitem.TagName) ? qtitem.TagName : "",
                            PositionTS = qtitem.LocationTS,
                            ServerTS = qtitem.ServerTS,
                            LastSeenTS = qtitem.LastSeenTS,
                            FloorId = qtitem.LocationCoordSysId,
                            Zones = qtitem.LocationZoneIds,
                            Color = qtitem.Color,
                            posAge = posAge,
                            Visible = visable

                        }
                    };
                    _tags.Add(marker);
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
            _endPointTimers[endPoint.Id] = new PeriodicTimer(TimeSpan.FromMilliseconds(endPoint.MillisecondsInterval));
        }
    }

    public void Dispose()
    {
        ((IDisposable)_cts).Dispose();
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
