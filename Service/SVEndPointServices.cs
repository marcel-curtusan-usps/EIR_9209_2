using EIR_9209_2.DataStore;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    public class SVEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryGeoZonesRepository _geoZones; 
        private readonly IInMemorySiteInfoRepository _siteInfo;

        public SVEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, IInMemoryGeoZonesRepository geozone, IInMemorySiteInfoRepository siteInfo)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection)
        {
            _geoZones = geozone;
            _siteInfo = siteInfo;
        }
        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                IQueryService queryService;
                SiteInformation siteinfo = _siteInfo.GetSiteInfo();
                if (siteinfo != null)
                {
                    string FormatUrl = string.Format(_endpointConfig.Url, siteinfo.SiteId);
                    queryService = new QueryService(_logger, _httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));
                    var result = await queryService.GetSVDoorData(stoppingToken);
                    //process zone data
                    if (_endpointConfig.MessageType.ToLower() == "doors")
                    {
                        // Process MPE data in a separate thread
                        await ProcessDoorsData(result);
                    }
                    if (_endpointConfig.MessageType.ToLower() == "getdoor_associated_trips")
                    {
                        // Process MPE data in a separate thread
                        await ProcessGetdoorAssociatedTripsData(result);
                    }
                    if (_endpointConfig.MessageType.ToLower() == "trip_itinerary")
                    {
                        // Process MPE data in a separate thread
                        await ProcessTripItineraryData(result);
                    }
                    if (_endpointConfig.MessageType.ToLower() == "trips")
                    {
                        // Process MPE data in a separate thread
                        await ProcessTripsData(result);
                    }
                    if (_endpointConfig.MessageType.ToLower() == "container")
                    {
                        // Process MPE data in a separate thread
                        await ProcessContainerData(result);
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
            try
            {
                await _geoZones.ProcessSVContainerData(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        private async Task ProcessDoorsData(JToken result)
        {
            try
            {
                await _geoZones.ProcessSVDoorsData(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}
