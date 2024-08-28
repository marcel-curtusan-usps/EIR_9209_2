using Microsoft.AspNetCore.SignalR;

namespace EIR_9209_2.Service
{
    public class QREEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryGeoZonesRepository _zones;
        private readonly IInMemoryTagsRepository _tags;
        private readonly IInMemoryEmpSchedulesRepository _empSchedules;

        public QREEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, IInMemoryGeoZonesRepository zones, IInMemoryTagsRepository tags, IInMemoryEmpSchedulesRepository empSchedules)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection)
        {
            _zones = zones;
            _tags = tags;
            _empSchedules = empSchedules;
        }

        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {

                _endpointConfig.Status = EWorkerServiceState.Running;
                _endpointConfig.LasttimeApiConnected = DateTime.Now;
                _endpointConfig.ApiConnected = true;
                await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", _endpointConfig, cancellationToken: stoppingToken);


                if (!string.IsNullOrEmpty(_endpointConfig.OAuthUrl))
                {
                    IOAuth2AuthenticationService authService;
                    authService = new OAuth2AuthenticationService(_logger, _httpClientFactory, new OAuth2AuthenticationServiceSettings(_endpointConfig.OAuthUrl, _endpointConfig.OAuthUserName, _endpointConfig.OAuthPassword, _endpointConfig.OAuthClientId, _endpointConfig.OutgoingApikey), jsonSettings);

                    IQueryService queryService;
                    string FormatUrl = "";
                    //process tag data
                    if (_endpointConfig.MessageType == "AREA_AGGREGATION")
                    {
                        FormatUrl = string.Format(_endpointConfig.Url);
                        queryService = new QueryService(_logger, _httpClientFactory, authService, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));

                        var now = DateTime.Now;
                        var endingHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Local);
                        var startingHour = endingHour.AddHours(-1 * _endpointConfig.HoursBack);
                        var currentHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Local);
                        var pastHour = currentHour.AddHours(-1);

                        var allAreaIds = await queryService.GetAreasAsync(stoppingToken);

                        int areasBatchCount = 24;
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
                            //// Process tag data in a separate thread
                            await Task.Run(() => _zones.RunMPESummaryReport(), stoppingToken).ConfigureAwait(false);
                        }

                    }
                    if (_endpointConfig.MessageType == "TAG_TIMELINE")
                    {
                        FormatUrl = string.Format(_endpointConfig.Url);
                        queryService = new QueryService(_logger, _httpClientFactory, authService, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));

                        var now = DateTime.Now;
                        var endingHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Local);
                        var hourBack = _endpointConfig.HoursBack;

                        //first day of the week
                        //DayOfWeek weekStart = DayOfWeek.Saturday;
                        //DateTime startingDate = DateTime.Today;
                        //while (startingDate.DayOfWeek != weekStart)
                        //    startingDate = startingDate.AddDays(-1);
                        //var weekFirstHour = startingDate.AddHours(-3);

                        DateTime startingDate = DateTime.Today;
                        startingDate = startingDate.AddDays(-7);
                        var weekFirstHour = startingDate.AddHours(-3);

                        var currentHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Local);
                        var pastHour = currentHour.AddHours(-1);
                        var hourCnt = 0;

                        var allAreaIds = await queryService.GetAreasAsync(stoppingToken);

                        int areasBatchCount = 24;
                        Int32.TryParse(_configuration[key: "ApplicationConfiguration:QREMinTimeOnArea"], out int MinTimeOnArea); //get the value from appsettings.json
                        Int32.TryParse(_configuration[key: "ApplicationConfiguration:QRETimeStep"], out int TimeStep); //get the value from appsettings.json
                        Int32.TryParse(_configuration[key: "ApplicationConfiguration:QREActivationTime"], out int ActivationTime); //get the value from appsettings.json
                        Int32.TryParse(_configuration[key: "ApplicationConfiguration:QREDeactivationTime"], out int DeactivationTime); //get the value from appsettings.json
                        Int32.TryParse(_configuration[key: "ApplicationConfiguration:QREDisappearTime"], out int DisappearTime); //get the value from appsettings.json

                        for (var hour = endingHour; weekFirstHour <= hour; hour = hour.AddHours(-1))
                        {
                            if (_tags.ExistingTagTimeline(hour))
                            {
                                if (currentHour == hour || pastHour == hour)
                                {
                                    var currentvalue = _tags.GetTagTimeline(hour);

                                    var newValue = await queryService.GetTotalTagTimeline(hour, hour.AddHours(1), TimeSpan.FromSeconds(MinTimeOnArea),
                                        TimeSpan.FromSeconds(TimeStep), TimeSpan.FromSeconds(ActivationTime),
                                        TimeSpan.FromSeconds(DeactivationTime), TimeSpan.FromSeconds(DisappearTime),
                                        allAreaIds, areasBatchCount, stoppingToken).ConfigureAwait(false);

                                    //add to the list
                                    _tags.UpdateTagTimeline(hour, newValue, currentvalue);

                                }
                            }
                            else
                            {
                                hourCnt++;
                                if (hourCnt <= hourBack)
                                {
                                    var newValue = await queryService.GetTotalTagTimeline(hour, hour.AddHours(1), TimeSpan.FromSeconds(MinTimeOnArea),
                                    TimeSpan.FromSeconds(TimeStep), TimeSpan.FromSeconds(ActivationTime),
                                    TimeSpan.FromSeconds(DeactivationTime), TimeSpan.FromSeconds(DisappearTime),
                                    allAreaIds, areasBatchCount, stoppingToken).ConfigureAwait(false);
                                    //add to the list
                                    _tags.AddTagTimeline(hour, newValue);
                                }
                            }
                            await Task.Run(() => _empSchedules.RunEmpScheduleReport(), stoppingToken).ConfigureAwait(false);
                        }
                        //remove too old tagtimeline data
                        _tags.RemoveTagTimeline(weekFirstHour);
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
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, cancellationToken: stoppingToken);
                }
            }
            finally
            {
                //run summary report
                if (_endpointConfig.MessageType == "AREA_AGGREGATION")
                {
                    await Task.Run(() => _zones.RunMPESummaryReport(), stoppingToken).ConfigureAwait(false);
                }
                //run employee schedule report
                if (_endpointConfig.MessageType == "TAG_TIMELINE")
                {
                    await Task.Run(() => _empSchedules.RunEmpScheduleReport(), stoppingToken).ConfigureAwait(false);
                }
            }
        }
    }
}
