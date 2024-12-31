using EIR_9209_2.DatabaseCalls.IDS;
using EIR_9209_2.DataStore;
using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using NuGet.Packaging;
using NuGet.Protocol;

namespace EIR_9209_2.Service
{
    internal class IDSEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryGeoZonesRepository _geoZones; 
        private readonly IIDS _ids;
        private readonly IInMemorySiteInfoRepository _siteInfo;
        public IDSEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, ILoggerService loggerService, IInMemoryGeoZonesRepository geozone, IIDS ids, IInMemorySiteInfoRepository siteInfo)
                : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection, loggerService)
        {
            _geoZones = geozone;
            _ids = ids;
            _siteInfo = siteInfo;
        }
        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                List<int> datadayList = [];
                List<int> rejectBinList = [];
                DateTime currentTime = await _siteInfo.GetCurrentTimeInTimeZone(DateTime.Now);
                int datadayStart = 0;
                if (_endpointConfig.LasttimeApiConnected.Year == 1 || (DateTime.Now - _endpointConfig.LasttimeApiConnected).TotalHours > 24)
                {
                    datadayStart = GetJulianDateDay(currentTime.AddDays(-7));
                }
                else
                {
                    datadayStart = GetJulianDateDay(currentTime.AddDays(-1));
                }
                int datadayEnd = GetJulianDateDay(currentTime);
           
                if (datadayStart < datadayEnd)
                {
                    datadayList = Enumerable.Range(datadayStart, datadayEnd-datadayStart+1).ToList();
                }
                else
                {
                    datadayList = Enumerable.Range(datadayStart, 63 - datadayStart + 1).ToList();
                }
                JObject data = new JObject
                {
                    ["datadayList"] = new JArray(datadayList),
                    ["queryName"] = _endpointConfig.MessageType
                };
                if (_endpointConfig.MessageType.Equals("SIPSPscCount", StringComparison.CurrentCultureIgnoreCase))
                {
                    var geoZones = await _geoZones.GetGeoZonebyName("MPE", "SIPS|ADUS|SDUS");
                    var geoZonesArray = geoZones as JArray;
                    var rejectBins = geoZonesArray?.Where(gz => gz["properties"]["rejectBins"].ToString() != "").Select(gz => gz["properties"]["rejectBins"]).FirstOrDefault()?.ToString();
                    if (rejectBins != null && rejectBins != "")
                    {
                        var rejectBinNumbers = rejectBins.Split(',').Select(int.Parse);
                        HashSet<int> uniqueRejectBins = new HashSet<int>(rejectBinList);

                        foreach (var bin in rejectBinNumbers)
                        {
                            if (uniqueRejectBins.Add(bin))
                            {
                                rejectBinList.Add(bin);
                            }
                        }
                    }
                    else
                    {
                        rejectBinList.Add(49); // Default value if no rejectBins found
                    }
                    data["rejectBins"] = new JArray(rejectBinList);
                }
                JToken result = await _ids.GetOracleIDSData(data);
                if (_endpointConfig.LogData)
                {
                    // Start a new thread to handle the logging
                    _ = Task.Run(() => _loggerService.LogData(result.ToJson(),
                        _endpointConfig.MessageType,
                        _endpointConfig.Name,
                        data.ToString()), stoppingToken);
                }
                if (result.HasValues)
                {
                    if (result is JObject resultObject && resultObject.ContainsKey("Error"))
                    {
                        _logger.LogError($"Error fetching data from IDS{result}");

                        _endpointConfig.Status = EWorkerServiceState.ErrorPullingData;
                        var updateCon = _connection.Update(_endpointConfig).Result;
                        if (updateCon != null)
                        {
                            await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"Fetched data from IDS");

                        _endpointConfig.Status = EWorkerServiceState.Idel;
                        var updateCon = _connection.Update(_endpointConfig).Result;
                        if (updateCon != null)
                        {
                            await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                        }
                        await ProcessIDSdata(result, stoppingToken);
                    }
               
                }
                else
                {
                    _logger.LogError($"Error fetching data from IDS{result}");                   
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

        private async Task ProcessIDSdata(JToken result, CancellationToken stoppingToken)
        {
           await _geoZones.ProcessIDSData(result, stoppingToken);
        }
        private int GetJulianDateDay(DateTime datadayTime)
        {
            //Julian Date from DCS_CNVRT_DATE function in IDS
            return Convert.ToInt32((datadayTime.AddDays(22).AddDays(-7 / 24).ToOADate() + 2415018.5) % 63) == 0 ? 63 : Convert.ToInt32((datadayTime.AddDays(22).AddDays(-7 / 24).ToOADate() + 2415018.5) % 63);
        }
    }
}