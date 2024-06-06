using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    public class QPEEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryTagsRepository _tags;
        public QPEEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IHubContext<HubServices> hubServices, IConfiguration configuration, IInMemoryTagsRepository tags)
            : base(logger, httpClientFactory, endpointConfig, hubServices, configuration)
        {
            _tags = tags;
        }

        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                IQueryService queryService;
                string FormatUrl = "";
                //process tag data
                if (_endpointConfig.MessageType == "getTagData")
                {
                    if (_endpointConfig.Status != EWorkerServiceState.Running)
                    {
                        _endpointConfig.Status = EWorkerServiceState.Running;
                        _endpointConfig.LasttimeApiConnected = DateTime.Now;
                        _endpointConfig.ApiConnected = true;
                        await _hubServices.Clients.Group("Connections").SendAsync("UpdateConnection", _endpointConfig);
                    }
                    FormatUrl = string.Format(_endpointConfig.Url, _endpointConfig.MessageType);
                    queryService = new QueryService(_httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
                    var result = (await queryService.GetQPETagData(stoppingToken));

                    // Process tag data in a separate thread
                    _ = Task.Run(async () => await ProcessTagMovementData(result), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
            }
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
                                ["visible"] = visable,
                                ["locationMovementStatus"] = qtitem.LocationMovementStatus,
                                ["positionTS_txt"] = qtitem.LocationTS,
                                ["craftName"] = _tags.GetCraftType(qtitem.TagId)
                            }
                        };

                        await _hubServices.Clients.Group("Tags").SendAsync("tags", PositionGeoJson.ToString());
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
