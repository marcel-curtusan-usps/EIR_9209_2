
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.SignalR;
using static EIR_9209_2.Models.GeoMarker;
using MongoDB.Driver;

namespace EIR_9209_2.Service
{

    public class BackgroundWorkerService : BackgroundService
    {
        private readonly ILogger<BackgroundWorkerService>? _logger;
        private readonly Dictionary<string, CancellationTokenSource>? _endPointCancellations;
        private readonly Dictionary<string, PeriodicTimer>? _endPointTimers;
        private readonly IHubContext<HubServices> _hubServices;
        private readonly List<Connection>? _endPointList; // List of endpoints
        private readonly IInMemoryConnectionRepository _connections;
        private readonly IInMemoryTagsRepository _tags;

        public BackgroundWorkerService(ILogger<BackgroundWorkerService> logger, IInMemoryConnectionRepository connectionList, IInMemoryTagsRepository tags, IHubContext<HubServices> hubServices)
        {
            _logger = logger;
            _connections = connectionList;
            _tags = tags;
            _endPointList = [];
            _endPointCancellations = [];
            _endPointTimers = [];
            _hubServices = hubServices;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                //allow time to load from file before starting the service
                await Task.Delay(1000, stoppingToken);
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (_endPointList != null && _endPointList.Any())
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
                                                await ProcessTagMovementData(result);
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
                        }
                    }
                    else
                    {
                        (_connections.GetAll())?.ToList().ForEach(endPoint => AddAndStartEndPoint(endPoint));
                        await Task.Delay(1000, stoppingToken);
                    }
                }
            }
            catch (Exception e)
            {

                _logger.LogInformation(e.Message);
            }
        }



        public async Task Start()
        {
            // Start the service
            ExecuteAsync(CancellationToken.None);
            Task.FromResult(true);
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
                    //find tag in the list
                    GeoMarker currentitem = _tags.Get(qtitem.TagId);
                    if (currentitem != null)
                    {
                        var update = false;
                        if (currentitem.Geometry.Coordinates != qtitem.Location)
                        {
                            currentitem.Geometry.Coordinates = qtitem.Location;
                            update = true;
                        }
                        //check if the tag is visable
                        if (currentitem.Properties.Visible != visable)
                        {
                            currentitem.Properties.Visible = visable;
                            update = true;
                        }
                        //check if the tag is on the same floor
                        if (currentitem.Properties.FloorId != qtitem.LocationCoordSysId)
                        {
                            currentitem.Properties.FloorId = qtitem.LocationCoordSysId;
                            update = true;
                        }
                        //check if tag posAge is different
                        if (currentitem.Properties.posAge != posAge)
                        {
                            currentitem.Properties.posAge = posAge;
                            update = true;
                        }
                        //check if the server timestamp is different
                        if (currentitem.Properties.ServerTS != qtitem.ServerTS)
                        {
                            currentitem.Properties.ServerTS = qtitem.ServerTS;
                            update = true;
                        }
                        if (update)
                        {
                            _tags.Update(currentitem);
                        }

                    }
                    else
                    {
                        GeoMarker NewMarker = new GeoMarker
                        {
                            _id = qtitem.TagId,
                            Geometry = new MarkerGeometry { Coordinates = qtitem.Location },
                            Properties = new Marker
                            {
                                Id = qtitem.TagId,
                                FloorId = qtitem.LocationCoordSysId,
                                ServerTS = qtitem.ServerTS,
                                posAge = posAge,
                                Visible = posAge > 1 && posAge < 150000 ? true : false
                            }
                        };

                        _tags.Add(NewMarker);

                    }

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
        public void Stop() { }

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
