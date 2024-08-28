using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    public class SVEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryGeoZonesRepository _geoZones;

        public SVEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, IInMemoryGeoZonesRepository geozone)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection)
        {
            _geoZones = geozone;
        }
        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                _endpointConfig.Status = EWorkerServiceState.Running;
                _endpointConfig.LasttimeApiConnected = DateTime.Now;
                _endpointConfig.ApiConnected = true;
                await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", _endpointConfig, cancellationToken: stoppingToken);
                IQueryService queryService;
                string FormatUrl = "";

                FormatUrl = string.Format(_endpointConfig.Url, _configuration[key: "ApplicationConfiguration:NassCode"]);
                queryService = new QueryService(_logger, _httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
                var result = await queryService.GetSVDoorData(stoppingToken);
                //process zone data
                if (_endpointConfig.MessageType.ToLower() == "doors")
                {
                    // Process MPE data in a separate thread
                    _ = Task.Run(() => ProcessDoorsData(result), stoppingToken);
                }
                if (_endpointConfig.MessageType.ToLower() == "getdoor_associated_trips")
                {
                    // Process MPE data in a separate thread
                    _ = Task.Run(() => ProcessGetdoorAssociatedTripsData(result), stoppingToken);
                }
                if (_endpointConfig.MessageType.ToLower() == "trip_itinerary")
                {
                    // Process MPE data in a separate thread
                    _ = Task.Run(() => ProcessTripItineraryData(result), stoppingToken);
                }
                if (_endpointConfig.MessageType.ToLower() == "trips")
                {
                    // Process MPE data in a separate thread
                    _ = Task.Run(() => ProcessTripsData(result), stoppingToken);
                }
                if (_endpointConfig.MessageType.ToLower() == "container")
                {
                    // Process MPE data in a separate thread
                    _ = Task.Run(() => ProcessContainerData(result), stoppingToken);
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
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, cancellationToken: stoppingToken);
                }
            }
        }

        private void ProcessGetdoorAssociatedTripsData(JToken result)
        {
            throw new NotImplementedException();
        }

        private void ProcessTripItineraryData(JToken result)
        {
            throw new NotImplementedException();
        }

        private void ProcessTripsData(JToken result)
        {
            throw new NotImplementedException();
        }

        private void ProcessContainerData(JToken result)
        {
            throw new NotImplementedException();
        }

        private void ProcessDoorsData(JToken result)
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
                            // await _hubServices.Clients.Group("DockDoorZones").SendAsync("DockDoorUpdateGeoZone", geoZone.Properties.MPERunPerformance);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}
