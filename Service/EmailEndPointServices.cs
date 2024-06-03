using Microsoft.AspNetCore.SignalR;

namespace EIR_9209_2.Service
{
    public class EmailEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryGeoZonesRepository _geoZones;

        public EmailEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IHubContext<HubServices> hubServices, IConfiguration configuration, IInMemoryGeoZonesRepository geoZones)
              : base(logger, httpClientFactory, endpointConfig, hubServices, configuration)
        {
            _geoZones = geoZones;
        }
        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                IQueryService queryService;
                _endpointConfig.Status = EWorkerServiceState.Running;
                _endpointConfig.LasttimeApiConnected = DateTime.Now;
                _endpointConfig.ApiConnected = true;

                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(_endpointConfig.Url, stoppingToken);
                //loop thought geozone and check if the email is in the geozone
                foreach (var email in _geoZones.GetAll().Where(r => !string.IsNullOrEmpty(r.Properties.Emails)).Select(y => y.Properties).ToList())
                {

                    string FormatUrl = "";
                    //send email

                    FormatUrl = string.Format(_endpointConfig.Url, email.MpeType);
                    queryService = new QueryService(_httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
                    var result = (await queryService.SendEmail(stoppingToken));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
            }
        }
    }
}