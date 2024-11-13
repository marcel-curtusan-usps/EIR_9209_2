using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using NuGet.Common;

namespace EIR_9209_2.Service
{
    public class CiscoSpacesEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryTagsRepository _tags;
        private readonly IInMemoryBackgroundImageRepository _backgroundImage;

        public CiscoSpacesEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, ILoggerService loggerService, IInMemoryTagsRepository tags, IInMemoryBackgroundImageRepository backgroundImage)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection, loggerService)
        {
            _tags = tags;
            _backgroundImage = backgroundImage;
        }

        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                string server = string.IsNullOrEmpty(_endpointConfig.IpAddress) ? _endpointConfig.Hostname : _endpointConfig.IpAddress;
                IOAuth2AuthenticationService authService;
                authService = new OAuth2AuthenticationService(_logger, _httpClientFactory, new OAuth2AuthenticationServiceSettings(server,"", "", "", "", _endpointConfig.OutgoingApikey,_endpointConfig.AuthType), jsonSettings);

                IQueryService queryService;
                //process tag data
                string FormatUrl = "";
                if (_endpointConfig.MessageType.Equals("CLIENT", StringComparison.CurrentCultureIgnoreCase))
                {
                    FormatUrl = string.Format(_endpointConfig.Url, server, _endpointConfig.MessageType, _endpointConfig.MapId, _endpointConfig.TenantId);
                    queryService = new QueryService(_logger, _httpClientFactory, authService, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));
                    var result = await queryService.GetCiscoSpacesData(stoppingToken);

                    // Process CLIENT data in a separate thread
                    await ProcessClients(result, stoppingToken);
                 
                }
                if (_endpointConfig.MessageType.Equals("BLE_TAG", StringComparison.CurrentCultureIgnoreCase))
                {
                    FormatUrl = string.Format(_endpointConfig.Url, server, _endpointConfig.MessageType, _endpointConfig.MapId, _endpointConfig.TenantId);
                    queryService = new QueryService(_logger, _httpClientFactory, authService, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));
                    var result = await queryService.GetCiscoSpacesData(stoppingToken);

                    // Process tag data in a separate thread
                    await ProcessBLE(result, stoppingToken);
                  
                }
                if (_endpointConfig.MessageType.Equals("FLOOR", StringComparison.CurrentCultureIgnoreCase))
                {
                    FormatUrl = string.Format(_endpointConfig.Url, server, _endpointConfig.MapId);
                    queryService = new QueryService(_logger, _httpClientFactory, authService, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));

                    var result = await queryService.GetCiscoSpacesData(stoppingToken);
                    if (((JObject)result).ContainsKey("locationHierarchy"))
                    {
                        //get network id for image
                        var locationHierarchy = result["locationHierarchy"];
                        List<string> networkId = new List<string>();
                        foreach (var item in locationHierarchy)
                        {
                            if (item["type"].ToString() == "floor")
                            {
                                networkId.Add(item["networkId"].ToString());
                            }
                        }

                        //for each network id get the image path
                        List<string> imagePath = new List<string>();
                        foreach (var netIdItem in networkId)
                        {
                            string elemnetsUrl = string.Format("https://{0}/api/location/v1/map/elements/{1}", server, netIdItem);
                            var MapElementsResult = await queryService.GetMapElementsAsync(elemnetsUrl, stoppingToken);
                            imagePath.Add(MapElementsResult["map"]["details"]["image"]["imageName"].ToString());
                        }

                        //for each image path get the image
                        List<string> image = new List<string>();
                        foreach (var imageItem in imagePath)
                        {
                            string imageUrl = string.Format("https://{0}/api/location/v1/map/images/floor/{1}", server, imageItem);
                            var imageResult = await queryService.GetMapImageAsync(imageUrl, stoppingToken);
                            image.Add(imageResult);
                        }
                       
                        if (((JObject)result).ContainsKey("maps"))
                        {
                            var map = result["maps"];
                            map[0]["imagePath"] = image.FirstOrDefault();
                            map[0]["id"] = networkId.FirstOrDefault();
                            await ProcessBackground(map[0], stoppingToken);
                        }
                      
                    }
                    if (((JObject)result).ContainsKey("accessPoints"))
                    {
                        await ProcessAccessPoints(result["accessPoints"], stoppingToken);
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
                _endpointConfig.ApiConnected = false;
                _endpointConfig.Status = EWorkerServiceState.ErrorPullingData;
                var updateCon = _connection.Update(_endpointConfig).Result;
                if (updateCon != null)
                {
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                }
            }
        }

        private async Task ProcessClients(JToken result, CancellationToken stoppingToken)
        {
            try
            {
                await _tags.UpdateTagCiscoSpacesClientInfo(result, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing QPE tag data");
            }
        }

        private async Task ProcessBLE(JToken result, CancellationToken stoppingToken)
        {
            try
            {
                await _tags.UpdateTagCiscoSpacesBLEInfo(result, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing QPE tag data");
            }
        }

        private async Task ProcessBackground(JToken? jToken, CancellationToken stoppingToken)
        {
            try
            {
                await _backgroundImage.ProcessCiscoSpacesBackgroundImage(jToken, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing QPE tag data");
            }
        }

        private async Task ProcessAccessPoints(JToken? jToken, CancellationToken stoppingToken)
        {
            try
            {
               await _tags.UpdateTagCiscoSpacesAPInfo(jToken, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing QPE tag data");
            }
        }
    }
}
