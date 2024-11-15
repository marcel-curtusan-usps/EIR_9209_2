
using EIR_9209_2.DataStore;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    public class IVESEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryEmployeesRepository _emp;
        private readonly IInMemoryEmployeesSchedule _schedules;
        private readonly IInMemorySiteInfoRepository _siteInfo;

        public IVESEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, ILoggerService loggerService, IInMemorySiteInfoRepository siteInfo, IInMemoryEmployeesRepository emp, IInMemoryEmployeesSchedule empSchedules)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection, loggerService)
        {
            _siteInfo = siteInfo;
            _emp = emp;
            _schedules = empSchedules;

        }

        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                SiteInformation siteinfo = await _siteInfo.GetSiteInfo();
                if (siteinfo != null)
                {
                    IQueryService queryService;
                    var now = _siteInfo.GetCurrentTimeInTimeZone(DateTime.Now);
                    string server = string.IsNullOrEmpty(_endpointConfig.IpAddress) ? _endpointConfig.Hostname : _endpointConfig.IpAddress;
                    string FormatUrl = string.Format(_endpointConfig.Url, server, siteinfo.FinanceNumber, now.ToString("yyyyMMdd"));
                    queryService = new QueryService(_logger, _httpClientFactory, jsonSettings, new QueryServiceSettings(
                        new Uri(FormatUrl),
                        new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)
                    ));
                    var result = await queryService.GetIVESData(stoppingToken);

                    if (_endpointConfig.MessageType.Equals("getEmpInfo", StringComparison.CurrentCultureIgnoreCase))
                    {
                        _endpointConfig.Status = EWorkerServiceState.Idel;
                        var updateCon = _connection.Update(_endpointConfig).Result;
                        if (updateCon != null)
                        {
                            await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                        }
                        await ProcessEmployeeInfoData(result, stoppingToken);
                    }
                    if (_endpointConfig.MessageType.Equals("getEmpSchedule", StringComparison.CurrentCultureIgnoreCase))
                    {
                        _endpointConfig.Status = EWorkerServiceState.Idel;
                        var updateCon = _connection.Update(_endpointConfig).Result;
                        if (updateCon != null)
                        {
                            await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                        }
                        await ProcessEmpScheduleData(result, stoppingToken);
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
                if (_endpointConfig.MessageType == "getEmpSchedule")
                {
                  _schedules.RunEmpScheduleReport();
                }
            }
        }
        private async Task ProcessEmployeeInfoData(JToken result, CancellationToken stoppingToken)
        {
            try
            {
                if (result is not null && ((JObject)result).ContainsKey("DATA"))
                {
                    await Task.Run(() => _emp.LoadEmployees(result), stoppingToken).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
        private async Task ProcessEmpScheduleData(JToken result, CancellationToken stoppingToken)
        {
            try
            {
                if (result is not null && ((JObject)result).ContainsKey("DATA"))
                {
                    await Task.Run(() => _schedules.LoadEmpSchedule(result), stoppingToken).ConfigureAwait(false); 
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

    }
}
