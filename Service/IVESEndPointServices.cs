
using EIR_9209_2.DataStore;
using EIR_9209_2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using PuppeteerSharp.Input;
using System;
using System.Configuration;

namespace EIR_9209_2.Service
{
    public class IVESEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryEmpSchedulesRepository _empSchedules;
        private readonly IInMemorySiteInfoRepository _siteInfo;

        public IVESEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IInMemoryConnectionRepository connection, IInMemorySiteInfoRepository siteInfo, IInMemoryEmpSchedulesRepository empSchedules)
            : base(logger, httpClientFactory, endpointConfig, configuration, connection)
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
                if (_endpointConfig.ActiveConnection)
                {
                    _endpointConfig.ApiConnected = true;
                }
                else
                {
                    _endpointConfig.ApiConnected = false;
                    _endpointConfig.Status = EWorkerServiceState.Idel;
                }

                await _connection.Update(_endpointConfig);

                IQueryService queryService;
                string FormatUrl = "";
                SiteInformation siteinfo = _siteInfo.GetByNASSCode(_configuration["ApplicationConfiguration:NassCode"]!.ToString());
                string Finnum = siteinfo.FinanceNumber;
                string TodayDate = DateTime.Now.ToString("yyyyMMdd");
                FormatUrl = string.Format(_endpointConfig.Url, Finnum, TodayDate);
                queryService = new QueryService(_httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
                var result = (await queryService.GetIVESData(stoppingToken));

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
                Logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
            }
        }
        private async Task ProcessEmployeeInfoData(JToken result)
        {
            try
            {
                if (result is not null && ((JObject)result).ContainsKey("DATA"))
                {
                    _ = Task.Run(() => _empSchedules.LoadEmpInfo(result));
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
        }
        private async Task ProcessEmpScheduleData(JToken result)
        {
            try
            {
                if (result is not null && ((JObject)result).ContainsKey("DATA"))
                {
                    _ = Task.Run(() => _empSchedules.LoadEmpSchedule(result));
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
        }

    }
}
