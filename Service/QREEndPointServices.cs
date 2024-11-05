using EIR_9209_2.DataStore;
using Microsoft.AspNetCore.SignalR;

namespace EIR_9209_2.Service
{
    public class QREEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryGeoZonesRepository _zones;
        private readonly IInMemoryTagsRepository _tags;
        private readonly IInMemoryEmployeesRepository _emp;
        private readonly IInMemorySiteInfoRepository _siteInfo;
        private readonly IInMemoryEmployeesSchedule _schedules;

        public QREEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, ILoggerService loggerService, IInMemoryGeoZonesRepository zones, IInMemoryTagsRepository tags, IInMemoryEmployeesRepository emp, IInMemoryEmployeesSchedule schedule , IInMemorySiteInfoRepository siteInfo)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection, loggerService)
        {
            _zones = zones;
            _tags = tags;
            _emp = emp;
            _schedules = schedule;
            _siteInfo = siteInfo;
        }

        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                if (!string.IsNullOrEmpty(_endpointConfig.OAuthUrl))
                {
                    string server = string.IsNullOrEmpty(_endpointConfig.IpAddress) ? _endpointConfig.Hostname : _endpointConfig.IpAddress;
                    IOAuth2AuthenticationService authService;
                    authService = new OAuth2AuthenticationService(_logger, _httpClientFactory, new OAuth2AuthenticationServiceSettings(server, _endpointConfig.OAuthUrl, _endpointConfig.OAuthUserName, _endpointConfig.OAuthPassword, _endpointConfig.OAuthClientId, _endpointConfig.OutgoingApikey,_endpointConfig.AuthType), jsonSettings);

                    IQueryService queryService;
                    string FormatUrl = "";
                    //process tag data
                    if (_endpointConfig.MessageType.Equals("AREA_AGGREGATION", StringComparison.CurrentCultureIgnoreCase))
                    {
                        FormatUrl = string.Format(_endpointConfig.Url, server);
                        queryService = new QueryService(_logger, _httpClientFactory, authService, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));

                        var now = _siteInfo.GetCurrentTimeInTimeZone(DateTime.Now);
                        var endingHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Local);
                        var startingHour = endingHour.AddHours(-1 * _endpointConfig.HoursBack);
                        var currentHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Local);
                        var pastHour = currentHour.AddHours(-1);

                        var allAreaIds = await queryService.GetAreasAsync(stoppingToken);

                        int areasBatchCount = 20;
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
                                    if (_endpointConfig.LogData)
                                    {
                                        Task.Run(() => _loggerService.LogData(newValue.ToString(),
                                        _endpointConfig.MessageType,
                                        _endpointConfig.Name,
                                        FormatUrl), stoppingToken);
                                    }
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

                                if (_endpointConfig.LogData)
                                {
                                    Task.Run(() => _loggerService.LogData(newValue.ToString(),
                                    _endpointConfig.MessageType,
                                    _endpointConfig.Name,
                                    FormatUrl), stoppingToken);
                                }
                                //add to the list
                                _zones.AddAreaDwell(hour, newValue);

                            }
                    
                        }
                        _endpointConfig.Status = EWorkerServiceState.Idel;
                        var updateCon = _connection.Update(_endpointConfig).Result;
                        if (updateCon != null)
                        {
                            await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                        }

                    }
                    if (_endpointConfig.MessageType.Equals("TAG_TIMELINE", StringComparison.CurrentCultureIgnoreCase))
                    {
                        FormatUrl = string.Format(_endpointConfig.Url,server);
                        queryService = new QueryService(_logger, _httpClientFactory, authService, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));

                        var now = _siteInfo.GetCurrentTimeInTimeZone(DateTime.Now);
                        var endingHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Local);
                        var hourBack = _endpointConfig.HoursBack;
                        var startingHour = endingHour.AddHours(-1 * _endpointConfig.HoursBack);
                        var currentHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Local);
                        var pastHour = currentHour.AddHours(-1);

                        var allAreaIds = await queryService.GetAreasAsync(stoppingToken);
                        int areasBatchCount = 20;
                        Int32.TryParse(_configuration[key: "ApplicationConfiguration:QREMinTimeOnArea"], out int MinTimeOnArea); //get the value from appsettings.json
                        Int32.TryParse(_configuration[key: "ApplicationConfiguration:QRETimeStep"], out int TimeStep); //get the value from appsettings.json
                        Int32.TryParse(_configuration[key: "ApplicationConfiguration:QREActivationTime"], out int ActivationTime); //get the value from appsettings.json
                        Int32.TryParse(_configuration[key: "ApplicationConfiguration:QREDeactivationTime"], out int DeactivationTime); //get the value from appsettings.json
                        Int32.TryParse(_configuration[key: "ApplicationConfiguration:QREDisappearTime"], out int DisappearTime); //get the value from appsettings.json

                        for (var hour = endingHour; startingHour <= hour; hour = hour.AddHours(-1))
                        {
                            if (_tags.ExistingTagTimeline(hour))
                            {
                                if (currentHour == hour || pastHour == hour)
                                {
                                    var currentvalue = _tags.GetCurrentTagTimeline(hour);

                                    var newValue = await queryService.GetTotalTagTimeLine(hour, hour.AddHours(1), TimeSpan.FromSeconds(MinTimeOnArea),
                                        TimeSpan.FromSeconds(TimeStep), TimeSpan.FromSeconds(ActivationTime),
                                        TimeSpan.FromSeconds(DeactivationTime), TimeSpan.FromSeconds(DisappearTime),
                                        allAreaIds, areasBatchCount, stoppingToken).ConfigureAwait(false);
                                    if (_endpointConfig.LogData)
                                    {
                                        Task.Run(() => _loggerService.LogData(newValue.ToString(),
                                        _endpointConfig.MessageType,
                                        _endpointConfig.Name,
                                        FormatUrl), stoppingToken);
                                    }
                                    //add to the list
                                    _tags.UpdateTagTimeline(hour, newValue, currentvalue);

                                }
                            }
                            else
                            {

                                var newValue = await queryService.GetTotalTagTimeLine(hour, hour.AddHours(1), TimeSpan.FromSeconds(MinTimeOnArea),
                                TimeSpan.FromSeconds(TimeStep), TimeSpan.FromSeconds(ActivationTime),
                                TimeSpan.FromSeconds(DeactivationTime), TimeSpan.FromSeconds(DisappearTime),
                                allAreaIds, areasBatchCount, stoppingToken).ConfigureAwait(false);

                                if (_endpointConfig.LogData)
                                {
                                    Task.Run(() => _loggerService.LogData(newValue.ToString(),
                                    _endpointConfig.MessageType,
                                    _endpointConfig.Name,
                                    FormatUrl), stoppingToken);
                                }
                                //add to the list
                                _tags.AddTagTimeline(hour, newValue);

                            }
                            _ = Task.Run(() => _schedules.RunEmpScheduleReport());
                        }
                        _endpointConfig.Status = EWorkerServiceState.Idel;
                        var updateCon = _connection.Update(_endpointConfig).Result;
                        if (updateCon != null)
                        {
                            await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                        }
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
            finally
            {
                //run summary report
                if (_endpointConfig.MessageType == "AREA_AGGREGATION")
                {
                   _ = Task.Run(() => _zones.RunMPESummaryReport());
                }
                //run employee schedule report
                if (_endpointConfig.MessageType == "TAG_TIMELINE")
                {
                    _ = Task.Run(() => _schedules.RunEmpScheduleReport());
                }
            }
        }
    }
}
