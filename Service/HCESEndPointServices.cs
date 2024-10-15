
using EIR_9209_2.DataStore;
using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    public class HCESEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryEmployeesRepository _emp;
        private readonly IInMemorySiteInfoRepository _siteInfo;

        public HCESEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, ILoggerService loggerService, IInMemorySiteInfoRepository siteInfo, IInMemoryEmployeesRepository emp)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection, loggerService)
        {
            _siteInfo = siteInfo;
            _emp = emp;
        }

        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                SiteInformation siteinfo = await _siteInfo.GetSiteInfo();
                if (siteinfo != null)
                {
                    string server = string.IsNullOrEmpty(_endpointConfig.IpAddress) ? _endpointConfig.Hostname : _endpointConfig.IpAddress;
                    IOAuth2AuthenticationService authService;
                    authService = new OAuth2AuthenticationService(_logger, _httpClientFactory, new OAuth2AuthenticationServiceSettings(server, "", _endpointConfig.OAuthUserName, _endpointConfig.OAuthPassword, _endpointConfig.OAuthClientId,"", _endpointConfig.AuthType), jsonSettings);

                    IQueryService queryService;
                    string FormatUrl = string.Format(_endpointConfig.Url, server);
                    queryService = new QueryService(_logger, _httpClientFactory, authService, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));
                    if (_endpointConfig.MessageType.Equals("getByFacilityID", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var result = await queryService.GetHCESData(stoppingToken, "facilityID", siteinfo.FacilityId, _endpointConfig.OAuthClientId);
                        await ProcessEmployeeInfoData(result, stoppingToken);
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
        private async Task ProcessEmployeeInfoData(Hces result, CancellationToken stoppingToken)
        {
            try
            {
                if (result is not null)
                {
                   await _emp.LoadHECSEmployees(result, stoppingToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}
