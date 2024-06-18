
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using PuppeteerSharp.Input;
using System.Configuration;

namespace EIR_9209_2.Service
{
    public class QREEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryGeoZonesRepository _zones;
        public QREEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IHubContext<HubServices> hubServices, IConfiguration configuration, IInMemoryGeoZonesRepository zones)
            : base(logger, httpClientFactory, endpointConfig, hubServices, configuration)
        {
            _zones = zones;
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

                if (!string.IsNullOrEmpty(_endpointConfig.OAuthUrl))
                {
                    IOAuth2AuthenticationService authService;
                    authService = new OAuth2AuthenticationService(_httpClientFactory, new OAuth2AuthenticationServiceSettings(_endpointConfig.OAuthUrl, _endpointConfig.OAuthUserName, _endpointConfig.OAuthPassword, _endpointConfig.OAuthClientId), jsonSettings);

                    IQueryService queryService;
                    string FormatUrl = "";
                    //process tag data
                    if (_endpointConfig.MessageType == "AREA_AGGREGATION")
                    {
                        FormatUrl = string.Format(_endpointConfig.Url);
                        queryService = new QueryService(_httpClientFactory, authService, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));

                        var now = DateTime.Now;
                        var endingHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Local);
                        var startingHour = endingHour.AddHours(-1 * _endpointConfig.HoursBack);
                        var currentHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Local);
                        var pastHour = currentHour.AddHours(-1);

                        var allAreaIds = await queryService.GetAreasAsync(stoppingToken);

                        int areasBatchCount = 10;
                        Int32.TryParse(_configuration[key: "ApplicationConfiguration:QREMinTimeOnArea"], out int MinTimeOnArea); //get the value from appsettings.json
                        Int32.TryParse(_configuration[key: "ApplicationConfiguration:QRETimeStep"], out int TimeStep); //get the value from appsettings.json
                        Int32.TryParse(_configuration[key: "ApplicationConfiguration:QREActivationTime"], out int ActivationTime); //get the value from appsettings.json
                        Int32.TryParse(_configuration[key: "ApplicationConfiguration:QREDeactivationTime"], out int DeactivationTime); //get the value from appsettings.json
                        Int32.TryParse(_configuration[key: "ApplicationConfiguration:QREDisappearTime"], out int DisappearTime); //get the value from appsettings.json

                        for (var hour = endingHour; startingHour <= hour; hour = hour.AddHours(-1))
                        {
                            if (_zones.ExistingAreaDwell(hour))
                            {
                                if (currentHour == hour || pastHour == hour)
                                {
                                    var currentvalue = _zones.GetAreaDwell(hour);
                                    var newValue = await queryService.GetTotalDwellTime(hour, hour.AddHours(1), TimeSpan.FromSeconds(MinTimeOnArea),
                                        TimeSpan.FromSeconds(TimeStep), TimeSpan.FromSeconds(ActivationTime),
                                         TimeSpan.FromSeconds(DeactivationTime), TimeSpan.FromSeconds(DisappearTime),
                                        allAreaIds, areasBatchCount, stoppingToken).ConfigureAwait(false);
                                    //add to the list
                                    _zones.UpdateAreaDwell(hour, newValue, currentvalue);
                                }
                            }
                            else
                            {
                                var newValue = await queryService.GetTotalDwellTime(hour, hour.AddHours(1), TimeSpan.FromSeconds(MinTimeOnArea),
                                        TimeSpan.FromSeconds(TimeStep), TimeSpan.FromSeconds(ActivationTime),
                                         TimeSpan.FromSeconds(DeactivationTime), TimeSpan.FromSeconds(DisappearTime),
                                         allAreaIds, areasBatchCount, stoppingToken).ConfigureAwait(false);
                                //add to the list
                                _zones.AddAreaDwell(hour, newValue);
                            }
                        }
                        _ = Task.Run(() => _zones.RunMPESummaryReport());
                        //var result = (await queryService.GetQPETagData(stoppingToken));
                        //await _hubServices.Clients.Group("Connections").SendAsync("UpdateConnection", _endpointConfig);
                        //// Process tag data in a separate thread
                        //_ = Task.Run(async () => await ProcessTagMovementData(result), stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
            }
        }
    }
}
