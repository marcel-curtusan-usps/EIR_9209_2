using EIR_9209_2.DataStore;
using Microsoft.AspNetCore.SignalR;
using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    /// <summary>
    /// Service for handling QRE endpoint operations.
    /// </summary>
    public class QREEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryGeoZonesRepository _zones;
        private readonly IInMemoryTagsRepository _tags;
        private readonly IInMemoryEmployeesRepository _emp;
        private readonly IInMemorySiteInfoRepository _siteInfo;
        private readonly IInMemoryEmployeesSchedule _schedules;
        /// <summary>
        /// Initializes a new instance of the <see cref="QREEndPointServices"/> class.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="endpointConfig"></param>
        /// <param name="configuration"></param>
        /// <param name="hubContext"></param>
        /// <param name="connection"></param>
        /// <param name="loggerService"></param>
        /// <param name="zones"></param>
        /// <param name="tags"></param>
        /// <param name="emp"></param>
        /// <param name="schedule"></param>
        /// <param name="siteInfo"></param>
        public QREEndPointServices(ILogger<QREEndPointServices> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, ILoggerService loggerService, IInMemoryGeoZonesRepository zones, IInMemoryTagsRepository tags, IInMemoryEmployeesRepository emp, IInMemoryEmployeesSchedule schedule, IInMemorySiteInfoRepository siteInfo)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection, loggerService)
        {
            _zones = zones;
            _tags = tags;
            _emp = emp;
            _schedules = schedule;
            _siteInfo = siteInfo;
        }
        /// <summary>
        /// Fetches data from the QRE endpoint. 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                if (!string.IsNullOrEmpty(_endpointConfig.OAuthUrl))
                {
                    string server = string.IsNullOrEmpty(_endpointConfig.IpAddress) ? _endpointConfig.Hostname : _endpointConfig.IpAddress;
                    IOAuth2AuthenticationService authService;
                    authService = new OAuth2AuthenticationService(_loggerService, _httpClientFactory, new OAuth2AuthenticationServiceSettings(server, _endpointConfig.OAuthUrl, _endpointConfig.OAuthUserName, _endpointConfig.OAuthPassword, _endpointConfig.OAuthClientId, _endpointConfig.OutgoingApikey, _endpointConfig.AuthType), jsonSettings);

                    IQueryService queryService;
                    string FormatUrl = "";
                    //process tag data
                    if (_endpointConfig.MessageType.Equals("TAG_AGGREGATION", StringComparison.CurrentCultureIgnoreCase))
                    {
                        FormatUrl = string.Format(_endpointConfig.Url, server);
                        queryService = new QueryService(_loggerService, _httpClientFactory, authService, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));

                        var now = await _siteInfo.GetCurrentTimeInTimeZone(DateTime.Now);
                        var endingHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Local);
                        var startingHour = endingHour.AddHours(-1 * _endpointConfig.HoursBack);
                        var currentHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Local);
                        var pastHour = currentHour.AddHours(-1);

                        var allAreaIds = await queryService.GetAreasAsync(stoppingToken);

                        int areasBatchCount = 20;
                        _ = int.TryParse(_configuration[key: "ApplicationConfiguration:QREMinTimeOnArea"], out int MinTimeOnArea); //get the value from appsettings.json
                        _ = int.TryParse(_configuration[key: "ApplicationConfiguration:QRETimeStep"], out int TimeStep); //get the value from appsettings.json
                        _ = int.TryParse(_configuration[key: "ApplicationConfiguration:QREActivationTime"], out int ActivationTime); //get the value from appsettings.json
                        _ = int.TryParse(_configuration[key: "ApplicationConfiguration:QREDeactivationTime"], out int DeactivationTime); //get the value from appsettings.json
                        _ = int.TryParse(_configuration[key: "ApplicationConfiguration:QREDisappearTime"], out int DisappearTime); //get the value from appsettings.json

                        for (var hour = endingHour; startingHour <= hour; hour = hour.AddHours(-1))
                        {
                            List<AreaDwell> newValue = new List<AreaDwell>();
                            if (_zones.ExistingAreaDwell(hour))
                            {
                                if (currentHour == hour || pastHour == hour)
                                {
                                    var currentvalue = _zones.GetAreaDwell(hour);

                                    // _logger.LogInformation("Calling GetTotalDwellTime with parameters: hour={Hour}, nextHour={NextHour}, MinTimeOnArea={MinTimeOnArea}, TimeStep={TimeStep}, ActivationTime={ActivationTime}, DeactivationTime={DeactivationTime}, DisappearTime={DisappearTime}, allAreaIds.Count={AllAreaIdsCount}, areasBatchCount={AreasBatchCount}",
                                    //     hour, hour.AddHours(1), MinTimeOnArea, TimeStep, ActivationTime, DeactivationTime, DisappearTime, allAreaIds == null ? -1 : allAreaIds.Count, areasBatchCount);

                                    //     _loggerService.LogData(JToken.FromObject($"Calling GetTotalDwellTime with parameters: hour={hour}, nextHour={hour.AddHours(1)}, MinTimeOnArea={MinTimeOnArea}, TimeStep={TimeStep}, ActivationTime={ActivationTime}, DeactivationTime={DeactivationTime}, DisappearTime={DisappearTime}, allAreaIds.Count={allAreaIds == null ? -1 : allAreaIds.Count}, areasBatchCount={areasBatchCount}"), "Info", "FetchDataFromEndpoint", FormatUrl);
                                    // if (allAreaIds == null)
                                    // {
                                    //     _logger.LogWarning("allAreaIds is null before calling GetTotalDwellTime for hour {Hour}", hour);
                                    // }
                                    // else if (allAreaIds.Count == 0)
                                    // {
                                    //     _logger.LogWarning("allAreaIds is empty before calling GetTotalDwellTime for hour {Hour}", hour);
                                    // }

                                    newValue = await queryService.GetTotalDwellTime(hour, hour.AddHours(1), TimeSpan.FromSeconds(MinTimeOnArea),
                                             TimeSpan.FromSeconds(TimeStep), TimeSpan.FromSeconds(ActivationTime),
                                              TimeSpan.FromSeconds(DeactivationTime), TimeSpan.FromSeconds(DisappearTime),
                                             allAreaIds, areasBatchCount, stoppingToken).ConfigureAwait(false);

                                    if (newValue == null)
                                    {
                                       // _logger.LogWarning("GetTotalDwellTime returned null for hour {Hour}. Skipping logging and UpdateAreaDwell.", hour);
                                        continue;
                                    }

                                    if (_endpointConfig.LogData)
                                    {
                                        _ = Task.Run(() => _loggerService.LogData(newValue.ToString(),
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
                                // _logger.LogInformation("Calling GetTotalDwellTime with parameters: hour={Hour}, nextHour={NextHour}, MinTimeOnArea={MinTimeOnArea}, TimeStep={TimeStep}, ActivationTime={ActivationTime}, DeactivationTime={DeactivationTime}, DisappearTime={DisappearTime}, allAreaIds.Count={AllAreaIdsCount}, areasBatchCount={AreasBatchCount}",
                                //     hour, hour.AddHours(1), MinTimeOnArea, TimeStep, ActivationTime, DeactivationTime, DisappearTime, allAreaIds == null ? -1 : allAreaIds.Count, areasBatchCount);
                                // if (allAreaIds == null)
                                // {
                                //     _logger.LogWarning("allAreaIds is null before calling GetTotalDwellTime for hour {Hour}", hour);
                                // }
                                // else if (allAreaIds.Count == 0)
                                // {
                                //     _logger.LogWarning("allAreaIds is empty before calling GetTotalDwellTime for hour {Hour}", hour);
                                // }

                                newValue = await queryService.GetTotalDwellTime(hour, hour.AddHours(1), TimeSpan.FromSeconds(MinTimeOnArea),
                                          TimeSpan.FromSeconds(TimeStep), TimeSpan.FromSeconds(ActivationTime),
                                           TimeSpan.FromSeconds(DeactivationTime), TimeSpan.FromSeconds(DisappearTime),
                                           allAreaIds, areasBatchCount, stoppingToken).ConfigureAwait(false);

                                if (newValue == null)
                                {
                                    //_logger.LogWarning("GetTotalDwellTime returned null for hour {Hour}. Skipping logging and AddAreaDwell.", hour);
                                    continue;
                                }

                                if (_endpointConfig.LogData)
                                {
                                    _ = Task.Run(() => _loggerService.LogData(newValue.ToString(),
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
                        FormatUrl = string.Format(_endpointConfig.Url, server);
                        queryService = new QueryService(_loggerService, _httpClientFactory, authService, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));

                        var now = await _siteInfo.GetCurrentTimeInTimeZone(DateTime.Now);
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
                                        _ = Task.Run(() => _loggerService.LogData(newValue.ToString(),
                                        _endpointConfig.MessageType,
                                        _endpointConfig.Name,
                                        FormatUrl), stoppingToken).ConfigureAwait(false);
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
                                    _ = Task.Run(() => _loggerService.LogData(newValue.ToString(),
                                      _endpointConfig.MessageType,
                                      _endpointConfig.Name,
                                      FormatUrl), stoppingToken).ConfigureAwait(false);
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
                    if (_endpointConfig.MessageType.Equals("Report_Generation", StringComparison.CurrentCultureIgnoreCase))
                    {
                        FormatUrl = string.Format(_endpointConfig.Url, server);
                        queryService = new QueryService(_loggerService, _httpClientFactory, authService, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));

                        var now = await _siteInfo.GetCurrentTimeInTimeZone(DateTime.Now);
                        var endingHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Local);
                        var startingHour = endingHour.AddHours(-1 * _endpointConfig.HoursBack);
                        var currentHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Local);
                        var pastHour = currentHour.AddHours(-1);

                        var allAreaOriginIds = await queryService.GetAreasOriginIdAsync(stoppingToken);
                        var allBadgeIds = await queryService.GetBadgeListAsync(stoppingToken);
                        int areasBatchCount = 20;
                        _ = int.TryParse(_configuration[key: "ApplicationConfiguration:QREMinTimeOnArea"], out int MinTimeOnArea); //get the value from appsettings.json
                        _ = int.TryParse(_configuration[key: "ApplicationConfiguration:QRETimeStep"], out int TimeStep); //get the value from appsettings.json
                        _ = int.TryParse(_configuration[key: "ApplicationConfiguration:QREActivationTime"], out int ActivationTime); //get the value from appsettings.json
                        _ = int.TryParse(_configuration[key: "ApplicationConfiguration:QREDeactivationTime"], out int DeactivationTime); //get the value from appsettings.json
                        _ = int.TryParse(_configuration[key: "ApplicationConfiguration:QREDisappearTime"], out int DisappearTime); //get the value from appsettings.json
                        List<string> eventTypes = new List<string> { "AREA" };
                        for (var hour = endingHour; startingHour <= hour; hour = hour.AddHours(-1))
                        {
                            ReportResponse reportResponse = null!;
                            if (await _zones.ExistingReportInList(hour))
                            {
                                if (currentHour == hour || pastHour == hour)
                                {
                                    reportResponse = await queryService.CreateReportDwellTime(hour, hour.AddHours(1), TimeSpan.FromSeconds(MinTimeOnArea),
                                                                      TimeSpan.FromSeconds(TimeStep), TimeSpan.FromSeconds(ActivationTime),
                                                                       TimeSpan.FromSeconds(DeactivationTime), TimeSpan.FromSeconds(DisappearTime), allBadgeIds,
                                                                       allAreaOriginIds, areasBatchCount, eventTypes, _endpointConfig.WebhookUrl, _endpointConfig.WebhookUserName, _endpointConfig.WebhookPassword, stoppingToken).ConfigureAwait(false);


                                    if (_endpointConfig.LogData)
                                    {
                                        _ = Task.Run(() => _loggerService.LogData(reportResponse.ToString(),
                                         _endpointConfig.MessageType,
                                         _endpointConfig.Name,
                                         FormatUrl), stoppingToken);
                                    }
                                    if (reportResponse == null)
                                    {
                                       await _loggerService.LogData(JToken.FromObject($"GetTotalDwellTime returned null for hour {hour}. Skipping logging and UpdateAreaDwell."), "Error", "FetchDataFromEndpoint", _endpointConfig.Url);
                                        continue;
                                    }
                                    reportResponse.DateTimeRequestFor = hour;
                                    reportResponse.DateTimeRange = pastHour.ToString("yyyyMMdd_HH:mm") + "_" + currentHour.ToString("yyyyMMdd_HH:mm");
                                    //add to the list
                                    await _zones.AddReportResponse(reportResponse);
                                }

                            }
                            else
                            {
                                reportResponse = await queryService.CreateReportDwellTime(hour, hour.AddHours(1), TimeSpan.FromSeconds(MinTimeOnArea),
                                                                       TimeSpan.FromSeconds(TimeStep), TimeSpan.FromSeconds(ActivationTime),
                                                                        TimeSpan.FromSeconds(DeactivationTime), TimeSpan.FromSeconds(DisappearTime), allBadgeIds,
                                                                        allAreaOriginIds, areasBatchCount, eventTypes, _endpointConfig.WebhookUrl, _endpointConfig.WebhookUserName, _endpointConfig.WebhookPassword, stoppingToken).ConfigureAwait(false);


                                if (_endpointConfig.LogData)
                                {
                                    _ = Task.Run(() => _loggerService.LogData(reportResponse.ToString(),
                                     _endpointConfig.MessageType,
                                     _endpointConfig.Name,
                                     FormatUrl), stoppingToken);
                                }
                                if (reportResponse == null)
                                {
                                    await _loggerService.LogData(JToken.FromObject($"GetTotalDwellTime returned null for hour {hour}. Skipping logging and UpdateAreaDwell."), "Error", "FetchDataFromEndpoint", _endpointConfig.Url);
                                    continue;
                                }
                                reportResponse.DateTimeRequestFor = hour;
                                reportResponse.DateTimeRange = pastHour.ToString("yyyyMMdd_HH:mm") + "_" + currentHour.ToString("yyyyMMdd_HH:mm");
                                //add to the list
                                await _zones.AddReportResponse(reportResponse);
                            }
                        }
                    }

                    //download reports data
                    if (_endpointConfig.MessageType.Equals("Report_Content", StringComparison.CurrentCultureIgnoreCase))
                    {
                        FormatUrl = BuildUrl(_endpointConfig.Url, new Dictionary<string, string> { { "ServerIpOrHostName", server } });
                        queryService = new QueryService(_loggerService, _httpClientFactory, authService, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));


                        var allReports = await _zones.GetReportList(stoppingToken).ConfigureAwait(false);
                        List<ReportResponse> reportList = allReports.Where(r => r.State != "Downloaded").ToList();
                        if (reportList != null && reportList.Count > 0)
                        {
                            for (int i = 0; i < reportList.Count; i++)
                            {
                                if (stoppingToken.IsCancellationRequested)
                                {
                                    return;
                                }
                                try
                                {

                                    var report = reportList[i];
                                    List<ReportContentItems> reportContentItems = await queryService.DownloadReportDwellTime(report.ResourceId, stoppingToken).ConfigureAwait(false);


                                    if (_endpointConfig.LogData)
                                    {
                                        _ = Task.Run(() => _loggerService.LogData(reportContentItems.ToString(),
                                         _endpointConfig.MessageType,
                                         _endpointConfig.Name,
                                         FormatUrl), stoppingToken);
                                    }
                                    if (reportContentItems == null)
                                    {
                                        await _loggerService.LogData(JToken.FromObject($"No data found for TotalDwellTime report with ResourceId {report.ResourceId}. Skipping logging and UpdateAreaDwell."), "Error", "FetchDataFromEndpoint", _endpointConfig.Url);
                                        continue;
                                    }

                                    await _zones.UpdateReportResponse(report.ResourceId, "Downloaded");
                                    await _zones.AddReportContentItem(report.DateTimeRequestFor, reportContentItems);
                                }
                                catch (System.Exception)
                                {

                                    continue;
                                }
                            }
                        }

                    }
                }
                else
                {
                    _loggerService.LogData(JToken.FromObject($"OAuthUrl is not set for endpoint {_endpointConfig.Id}. Skipping data fetch."), "Error", "FetchDataFromEndpoint", _endpointConfig.Url);
                    _endpointConfig.ApiConnected = false;
                    _endpointConfig.Status = EWorkerServiceState.ErrorPullingData;
                }
            }
            catch (Exception ex)
            {
                await _loggerService.LogData(JToken.FromObject(ex.Message), "Error", "FetchDataFromEndpoint", _endpointConfig.Url);
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
                if (_endpointConfig.MessageType == "TAG_AGGREGATION")
                {
                    _ = Task.Run(() => _zones.RunMPESummaryReport());
                }
                //run summary report
                if (_endpointConfig.MessageType == "Report_Content")
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
