using EIR_9209_2.DataStore;
using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    /// <summary>
    /// This class is responsible for fetching data from the SMS Wrapper endpoint and processing it.
    /// </summary>
    public class SMSWrapperEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryEmployeesRepository _emp;
        private readonly IInMemorySiteInfoRepository _siteInfo;
        /// <summary>
        /// Initializes a new instance of the <see cref="SMSWrapperEndPointServices"/> class.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="endpointConfig"></param>
        /// <param name="configuration"></param>
        /// <param name="hubContext"></param>
        /// <param name="connection"></param>
        /// <param name="loggerService"></param>
        /// <param name="emp"></param>
        /// <param name="siteInfo"></param>
        public SMSWrapperEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, ILoggerService loggerService, IInMemoryEmployeesRepository emp, IInMemorySiteInfoRepository siteInfo)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection, loggerService)
        {
            _emp = emp;
            _siteInfo = siteInfo;
        }
        /// <summary>
        /// Fetches data from the endpoint based on the message type specified in the configuration.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
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
                    if (_endpointConfig.MessageType.Equals("SMSWrapperDBCheck", StringComparison.CurrentCultureIgnoreCase))
                    {
                        FormatUrl = string.Format(_endpointConfig.Url, server, _endpointConfig.MessageType, siteinfo.FacilityId);
                        queryService = new QueryService(_loggerService, _httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));
                        _endpointConfig.Status = EWorkerServiceState.Idel;
                        var updateCon = _connection.Update(_endpointConfig).Result;
                        if (updateCon != null)
                        {
                            await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                        }
                    }

                    else if (_endpointConfig.MessageType.Equals("NASSCodeEmployeeList", StringComparison.CurrentCultureIgnoreCase))
                    {
                        FormatUrl = string.Format(_endpointConfig.Url, server, _endpointConfig.MessageType, siteinfo.SiteId);

                        queryService = new QueryService(_loggerService, _httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));
                        var result = await queryService.GetSMSWrapperData(stoppingToken);

                        _endpointConfig.Status = EWorkerServiceState.Idel;
                        var updateCon = _connection.Update(_endpointConfig).Result;
                        if (updateCon != null)
                        {
                            await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                        }
                        // Start a new thread to handle the logging
                        _ = Task.Run(() => _loggerService.LogData(JToken.Parse(JsonConvert.SerializeObject(result, Formatting.Indented)),
                             _endpointConfig.MessageType,
                             _endpointConfig.Name,
                             FormatUrl), stoppingToken);
                        // Process tag data in a separate thread
                        await ProcessEmployeeListData(result, stoppingToken);
                    }
                    else if (_endpointConfig.MessageType.Equals("FDBIDEmployeeList", StringComparison.CurrentCultureIgnoreCase))
                    {
                        FormatUrl = string.Format(_endpointConfig.Url, server, _endpointConfig.MessageType, siteinfo.FacilityId);

                        queryService = new QueryService(_loggerService, _httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));
                        var result = await queryService.GetSMSWrapperData(stoppingToken);

                        _endpointConfig.Status = EWorkerServiceState.Idel;
                        var updateCon = _connection.Update(_endpointConfig).Result;
                        if (updateCon != null)
                        {
                            await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                        }
                        // Start a new thread to handle the logging
                        _ = Task.Run(() => _loggerService.LogData(JToken.Parse(JsonConvert.SerializeObject(result, Formatting.Indented)),
                             _endpointConfig.MessageType,
                             _endpointConfig.Name,
                             FormatUrl), stoppingToken);
                        // Process tag data in a separate thread
                        await ProcessEmployeeListData(result, stoppingToken);
                    }
                    else
                    {
                        await _loggerService.LogData(JToken.FromObject("Invalid Message Type"), "Error", "FetchDataFromEndpoint", _endpointConfig.Url);
                    }
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
                await _loggerService.LogData(JToken.FromObject(e.Message), "Error", "ProcessEmployeeListData", _endpointConfig.Url);
            }
        }
    }
}

