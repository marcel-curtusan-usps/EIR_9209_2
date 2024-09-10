
using EIR_9209_2.DataStore;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    public class HCESEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryEmployeesRepository _empSchedules;
        private readonly IInMemorySiteInfoRepository _siteInfo;

        public HCESEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, IInMemorySiteInfoRepository siteInfo, IInMemoryEmployeesRepository empSchedules)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection)
        {
            _siteInfo = siteInfo;
            _empSchedules = empSchedules;
        }

        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                SiteInformation siteinfo = _siteInfo.GetSiteInfo();
                if (siteinfo != null)
                {
                    IQueryService queryService;
                    var now = _siteInfo.GetCurrentTimeInTimeZone(DateTime.Now);
                    string FormatUrl = string.Format(_endpointConfig.Url);
                    string Finnum = siteinfo.FinanceNumber;
                    queryService = new QueryService(_logger, _httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
                    var result = await queryService.GetIVESData(stoppingToken);

                    if (_endpointConfig.MessageType == "getByFacilityID")
                    {
                        _ = Task.Run(async () => await ProcessEmployeeInfoData(result), stoppingToken);
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
                    await Task.Run(() => _empSchedules.RunEmpScheduleReport(), stoppingToken).ConfigureAwait(false);
                }
            }
        }
        private async Task ProcessEmployeeInfoData(JToken result)
        {
            try
            {
                if (result is not null && ((JObject)result).ContainsKey("DATA"))
                {
                    await Task.Run(() => _empSchedules.LoadEmployees(result)).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
        private async Task ProcessEmpScheduleData(JToken result)
        {
            try
            {
                if (result is not null && ((JObject)result).ContainsKey("DATA"))
                {
                    await Task.Run(() => _empSchedules.LoadEmpSchedule(result)).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

    }
}
