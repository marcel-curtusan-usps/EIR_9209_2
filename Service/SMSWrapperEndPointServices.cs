using EIR_9209_2.DataStore;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    public class SMSWrapperEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryTagsRepository _tags;
        private readonly IInMemorySiteInfoRepository _siteInfo;

        public SMSWrapperEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, IInMemoryTagsRepository tags, IInMemorySiteInfoRepository siteInfo)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection)
        {
            _tags = tags;
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
                        FormatUrl = string.Format(_endpointConfig.Url, server, _endpointConfig.MessageType, siteinfo.SiteId);
                    }
                    if (_endpointConfig.MessageType.Equals("NASSCodeEmployeeList", StringComparison.CurrentCultureIgnoreCase))
                    {
                        FormatUrl = string.Format(_endpointConfig.Url, server, _endpointConfig.MessageType, siteinfo.SiteId);
                    }
                    if (!string.IsNullOrEmpty(FormatUrl))
                    {
                        queryService = new QueryService(_logger, _httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));
                        var result = await queryService.GetSMSWrapperData(stoppingToken);
                        // Process tag data in a separate thread
                        await ProcessFDBIDEmployeeListData(result, stoppingToken);
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

        private async Task ProcessFDBIDEmployeeListData(JToken result, CancellationToken stoppingToken)
        {
            try
            {
                if (result is not null)
                {
                    await Task.Run(() => _tags.UpdateEmployeeInfo(result),stoppingToken).ConfigureAwait(false);

                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}

