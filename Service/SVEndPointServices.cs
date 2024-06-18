using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    public class SVEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryGeoZonesRepository _geoZones;
        public SVEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IHubContext<HubServices> hubServices, IConfiguration configuration, IInMemoryGeoZonesRepository geozone)
            : base(logger, httpClientFactory, endpointConfig, hubServices, configuration)
        {
            _geoZones = geozone;
        }
        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                _endpointConfig.Status = EWorkerServiceState.Running;
                _endpointConfig.LasttimeApiConnected = DateTime.Now;
                if (_endpointConfig.ActiveConnection)
                {
                    _endpointConfig.ApiConnected = true;
                }
                else
                {
                    _endpointConfig.ApiConnected = false;
                    _endpointConfig.Status = EWorkerServiceState.Idel;
                }
                await _hubServices.Clients.Group("Connections").SendAsync("UpdateConnection", _endpointConfig);

                IQueryService queryService;
                string FormatUrl = "";




                FormatUrl = string.Format(_endpointConfig.Url, _configuration[key: "ApplicationConfiguration:NassCode"]);
                queryService = new QueryService(_httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
                var result = (await queryService.GetSVDoorData(stoppingToken));
                //process zone data
                if (_endpointConfig.MessageType.ToLower() == "doors")
                {
                    // Process MPE data in a separate thread
                    _ = Task.Run(async () => await ProcessDoorsData(result), stoppingToken);
                }
                if (_endpointConfig.MessageType.ToLower() == "getdoor_associated_trips")
                {
                    // Process MPE data in a separate thread
                    _ = Task.Run(async () => await ProcessGetdoorAssociatedTripsData(result), stoppingToken);
                }
                if (_endpointConfig.MessageType.ToLower() == "trip_itinerary")
                {
                    // Process MPE data in a separate thread
                    _ = Task.Run(async () => await ProcessTripItineraryData(result), stoppingToken);
                }
                if (_endpointConfig.MessageType.ToLower() == "trips")
                {
                    // Process MPE data in a separate thread
                    _ = Task.Run(async () => await ProcessTripsData(result), stoppingToken);
                }
                if (_endpointConfig.MessageType.ToLower() == "container")
                {
                    // Process MPE data in a separate thread
                    _ = Task.Run(async () => await ProcessContainerData(result), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
            }
        }

        private async Task ProcessGetdoorAssociatedTripsData(JToken result)
        {
            throw new NotImplementedException();
        }

        private async Task ProcessTripItineraryData(JToken result)
        {
            throw new NotImplementedException();
        }

        private async Task ProcessTripsData(JToken result)
        {
            throw new NotImplementedException();
        }

        private async Task ProcessContainerData(JToken result)
        {
            throw new NotImplementedException();
        }

        private async Task ProcessDoorsData(JToken result)
        {
            try
            {
                foreach (var item in result.OfType<JObject>())
                {
                    string name = "";


                    var geoZone = _geoZones.GetMPEName(name);
                    if (geoZone != null)
                    {
                        bool pushUIUpdate = false;
                        if (pushUIUpdate)
                        {
                            await _hubServices.Clients.Group("DockDoorZones").SendAsync("DockDoorUpdateGeoZone", geoZone.Properties.MPERunPerformance);
                        }
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
