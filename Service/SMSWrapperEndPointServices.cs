using EIR_9209_2.DataStore;
using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    public class SMSWrapperEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryEmployeesRepository _emp;
        private readonly IInMemorySiteInfoRepository _siteInfo;

        public SMSWrapperEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, ILoggerService loggerService, IInMemoryEmployeesRepository emp, IInMemorySiteInfoRepository siteInfo)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection, loggerService)
        {
            _emp = emp;
            _siteInfo = siteInfo;
        }

        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                SiteInformation siteinfo = await _siteInfo.GetSiteInfo();
                if (siteinfo != null)
                {
                    IQueryService queryService;
                    string server = string.IsNullOrEmpty(_endpointConfig.IpAddress) ? _endpointConfig.Hostname : _endpointConfig.IpAddress;
                    string FormatUrl = "";

                    if (_endpointConfig.MessageType.Equals("FDBIDEmployeeList", StringComparison.CurrentCultureIgnoreCase))
                    {
                        FormatUrl = string.Format(_endpointConfig.Url, server, _endpointConfig.MessageType, siteinfo.FacilityId);
                    }
                    if (_endpointConfig.MessageType.Equals("NASSCodeEmployeeList", StringComparison.CurrentCultureIgnoreCase))
                    {
                        FormatUrl = string.Format(_endpointConfig.Url, server, _endpointConfig.MessageType, siteinfo.SiteId);
                    }
                    if (!string.IsNullOrEmpty(FormatUrl))
                    {
                        queryService = new QueryService(_logger, _httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));
                        var result = await queryService.GetSMSWrapperData(stoppingToken);
                        
                        _endpointConfig.Status = EWorkerServiceState.Idel;
                        var updateCon = _connection.Update(_endpointConfig).Result;
                        if (updateCon != null)
                        {
                            await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                        }
                        // Process tag data in a separate thread
                        await ProcessEmployeeListData(result, stoppingToken);
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

        private async Task ProcessEmployeeListData(List<SMSWrapperEmployeeInfo> result, CancellationToken stoppingToken)
        {
            try
            {
                if (result is not null)
                {
                    await _emp.LoadSMSEmployeeInfo(result, stoppingToken);

                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}

