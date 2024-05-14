using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.GeoJsonObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Text;

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
    private readonly IConnectionRepository _connections;
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
    public WorkerService(ILogger<WorkerService> logger, IConnectionRepository connectionList, HubServices hubServices)
    {
        _logger = logger;
        _connections = connectionList;
        _hubServices = hubServices;
        _endPointList = [];
        _endPointCancellations = [];
        _endPointTimers = [];
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        StartWorkerService();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {

        // remove endpoint from list 
        _endPointList.ForEach(endPoint => StopEndPoint(endPoint.Id));
        _endPointList.ForEach(endPoint => RemoveEndPoint(endPoint));
        _endPointList.Clear();
        _endPointCancellations.Clear();
        _endPointTimers.Clear();
        _cts.Cancel();
        return Task.CompletedTask;
    }
    private async void StartWorkerService()
    {
        State = EWorkerServiceState.Starting;
        _cts = new CancellationTokenSource();
        (await _connections.GetAll())?.ToList().ForEach(endPoint => AddAndStartEndPoint(endPoint));
        _ = RunWorkerServiceAsync();
    }

    private async Task RunWorkerServiceAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            State = EWorkerServiceState.Running;
            try
            {
                if (_endPointList != null && _endPointList.Any())
                {
                    foreach (var endPoint in _endPointList)
                    {
                        endPoint.Status = EWorkerServiceState.Starting;
                        _endPointTimers[endPoint.Id] = new PeriodicTimer(TimeSpan.FromMilliseconds(endPoint.Interval));
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
                                    if (!_endPointCancellations[endPoint.Id].IsCancellationRequested && await _endPointTimers[endPoint.Id].WaitForNextTickAsync())
                                    {
                                        try
                                        {
                                            endPoint.Status = EWorkerServiceState.Running;

                                            using HttpClient _httpClient = new();
                                            var result = new JObject();
                                            try
                                            {
                                                if (!string.IsNullOrEmpty(endPoint.OAuthUrl))
                                                {
                                                    //oauth2 token
                                                    IOAuth2AuthenticationService authService;
                                                    authService = new OAuth2AuthenticationService(_httpClient, new OAuth2AuthenticationServiceSettings(endPoint.OAuthUrl, endPoint.UserName, endPoint.Password, endPoint.ClientId), jsonSettings);
                                                    IQueryService queryService;
                                                    queryService = new QueryService(_httpClient, authService, jsonSettings, new QueryServiceSettings(new Uri(endPoint.Url)));
                                                    result = await queryService.GetData(_endPointCancellations[endPoint.Id].Token);
                                                    //process tag data
                                                    if (endPoint.MessageType == "getTagData")
                                                    {
                                                        await ProcessTagData(result);
                                                    }
                                                }
                                                else
                                                {
                                                    IQueryService queryService;
                                                    queryService = new QueryService(_httpClient, jsonSettings, new QueryServiceSettings(new Uri(endPoint.Url)));
                                                    result = await queryService.GetData(_endPointCancellations[endPoint.Id].Token);
                                                    //process tag data
                                                    if (endPoint.MessageType == "getTagData")
                                                    {
                                                        await ProcessTagData(result);
                                                    }
                                                }

                                            }
                                            catch (Exception e)
                                            {
                                                endPoint.Status = EWorkerServiceState.ErrorPullingData;
                                                _logger.LogError(e, "Error Pulling data from URL Retrying...");
                                            }
                                            finally
                                            {
                                                result = null;
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
            }
            catch (Exception ex)
            {
                State = EWorkerServiceState.Stopping;
                _logger.LogError(ex, "Error while connecting to the Main Application. Retrying in 60 seconds...");

            }
        }
    }
    private async Task ProcessTagData(JObject result)
    {
        try
        {
            if (result.HasValues && result.ContainsKey("tags"))
            {
                long serverTime = (long)result["responseTS"];

                foreach (JObject item in result["tags"].OfType<JObject>())
                {
                    long posAge = (long)item["locationTS"] > 0 ? (long)result["responseTS"] - (long)item["locationTS"] : 0;
                    bool visable = posAge < 150000 ? true : false;
                    JObject GeoJson = new JObject
                    {
                        ["type"] = "Feature",
                        ["geometry"] = new JObject
                        {
                            ["type"] = "Point",
                            ["coordinates"] = new JArray(item["location"][0], item["location"][1])
                        },
                        ["properties"] = new JObject
                        {
                            ["locationTS"] = item["locationTS"],
                            ["serverTS"] = result["responseTS"],
                            ["id"] = item["tagId"],
                            ["color"] = item["color"],
                            ["floorId"] = item["locationCoordSysId"],
                            ["locationZoneIds"] = item["locationZoneIds"],
                            ["locationZoneNames"] = item["locationZoneNames"],
                            ["locationMovementStatus"] = item["locationMovementStatus"],
                            ["locationType"] = item["locationType"],
                            ["posAge"] = posAge,
                            ["visible"] = visable
                        }
                    };
                    JObject PositionGeoJson = new JObject
                    {
                        ["type"] = "Feature",
                        ["geometry"] = new JObject
                        {
                            ["type"] = "Point",
                            ["coordinates"] = new JArray(item["location"][0], item["location"][1])
                        },
                        ["properties"] = new JObject
                        {
                            ["id"] = item["tagId"],
                            ["floorId"] = item["locationCoordSysId"]?.ToString(),
                            ["posAge"] = posAge,
                            ["visible"] = visable
                        }
                    };
                    //try
                    //{
                    //    byte[] data = Encoding.ASCII.GetBytes(GeoJson.ToString());
                    //    //_ = _connection.InvokeAsync("WorkerData", data);
                    //    //await _hubServices.SendMessageToGroup("Tags", GeoJson.ToString(), "tags");
                    //}
                    //catch (Exception ed)
                    //{
                    //    _logger.LogError(ed.Message);
                    //}
                    try
                    {
                        byte[] positiondata = Encoding.ASCII.GetBytes(PositionGeoJson.ToString());
                        //_ = _connection.InvokeAsync("WorkerPositionData", positiondata);
                        await _hubServices.SendMessageToGroup("Tags", GeoJson.ToString(), "tags");
                    }
                    catch (Exception ep)
                    {
                        _logger.LogError(ep.Message);
                    }


                }
            }
            else
            {
                _logger.LogError("No Tags Found");
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
            _endPointTimers[endPoint.Id] = new PeriodicTimer(TimeSpan.FromMilliseconds(endPoint.Interval));
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
