
using EIR_9209_2.DataStore;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    public class IVESEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryEmpSchedulesRepository _empSchedules;
        private readonly IInMemorySiteInfoRepository _siteInfo;

        public IVESEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, IInMemorySiteInfoRepository siteInfo, IInMemoryEmpSchedulesRepository empSchedules)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection)
        {
            _siteInfo = siteInfo;
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


                IQueryService queryService;
                string FormatUrl = "";
                SiteInformation siteinfo = _siteInfo.GetSiteInfo();
                string Finnum = siteinfo.FinanceNumber;
                string TodayDate = DateTime.Now.ToString("yyyyMMdd");
                FormatUrl = string.Format(_endpointConfig.Url, Finnum, TodayDate);
                queryService = new QueryService(_logger, _httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
                var result = await queryService.GetIVESData(stoppingToken);

                if (_endpointConfig.MessageType == "getEmpInfo")
                {
                    _ = Task.Run(async () => await ProcessEmployeeInfoData(result), stoppingToken);
                }
                if (_endpointConfig.MessageType == "getEmpSchedule")
                {
                    _ = Task.Run(async () => await ProcessEmpScheduleData(result), stoppingToken);
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
                    await Task.Run(() => _empSchedules.LoadEmpInfo(result)).ConfigureAwait(false);
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
