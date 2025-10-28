using EIR_9209_2.DataStore;
using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace EIR_9209_2.Service
{
    internal class CameraEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryCamerasRepository _camera;
        private readonly IInMemorySiteInfoRepository _siteInfo;
        public CameraEndPointServices(ILogger<CameraEndPointServices> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, ILoggerService loggerService, IInMemorySiteInfoRepository siteInfo, IInMemoryCamerasRepository camera)
                : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection, loggerService)
        {
            _siteInfo = siteInfo;
            _camera = camera;

        }
        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                string server = string.IsNullOrEmpty(_endpointConfig.IpAddress) ? _endpointConfig.Hostname : _endpointConfig.IpAddress;

                if (_endpointConfig.MessageType.Equals("Cameras", StringComparison.CurrentCultureIgnoreCase))
                {
                    await HandleCameraListAsync(server, stoppingToken);
                }

                if (_endpointConfig.MessageType.Equals("getCameraStills", StringComparison.CurrentCultureIgnoreCase))
                {
                    await HandleGetCameraStillsAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                await _loggerService.LogData(JToken.FromObject(ex.Message), "Error", _endpointConfig.MessageType, _endpointConfig.Url);
                _endpointConfig.ApiConnected = false;
                _endpointConfig.Status = EWorkerServiceState.ErrorPullingData;
                var updateCon = _connection.Update(_endpointConfig).Result;
                if (updateCon != null)
                {
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                }
            }
        }

        private async Task HandleCameraListAsync(string server, CancellationToken stoppingToken)
        {
            // The Cameras URL expected server and site FdbId (see previous implementation)
            SiteInformation siteinfo = await _siteInfo.GetSiteInfo();
            if (siteinfo == null)
            {
                await _loggerService.LogData(JToken.FromObject("Site information not available for camera list fetch"), "Warning", _endpointConfig.MessageType, _endpointConfig.Url);
                return;
            }

            string FormatUrl = string.Format(_endpointConfig.Url, server, siteinfo.FdbId);
            var queryService = new QueryService(_loggerService, _httpClientFactory, jsonSettings,
            new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));

            var cresult = await queryService.GetCameraData(stoppingToken);
            _endpointConfig.Status = EWorkerServiceState.Idel;
            var updateCon = _connection.Update(_endpointConfig).Result;
            if (updateCon != null)
            {
                await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
            }
            // Process the data as needed
            await ProcessCameraData(cresult);
        }

        private async Task HandleGetCameraStillsAsync(CancellationToken stoppingToken)
        {
            // process camera list and get pictures
            foreach (CameraGeoMarker camera in _camera.GetAll())
            {
                // stop if cancellation requested
                if (stoppingToken.IsCancellationRequested)
                {
                    await _loggerService.LogData(JToken.FromObject("Cancellation requested - stopping camera stills fetch loop."), "Information", _endpointConfig.MessageType, _endpointConfig.Url);
                    break;
                }

                byte[] result = Array.Empty<byte>();
                // capture camera id safely for logging and downstream calls
                var cameraId = camera?.Properties?.Id ?? string.Empty;
                string FormatUrl = string.Empty;
                try
                {
                    if (string.IsNullOrEmpty(camera?.Properties?.CameraName))
                    {
                        continue;
                    }
                    FormatUrl = string.Format(_endpointConfig.Url, camera.Properties.IP);

                    // Use IHttpClientFactory to obtain an HttpClient (do not create per-iteration new instances)
                    var httpClient = _httpClientFactory.CreateClient();
                    // Use GetAsync with the cancellation token so the operation can be cancelled
                    var response = await httpClient.GetAsync(FormatUrl, stoppingToken);
                    response.EnsureSuccessStatusCode();
                    result = await response.Content.ReadAsByteArrayAsync();
                    await _camera.LoadCameraStills(result, cameraId);
                }
                catch (OperationCanceledException oce)
                {
                    await _loggerService.LogData(JToken.FromObject("Camera stills fetch cancelled for camera " + cameraId), "Information", _endpointConfig.MessageType, FormatUrl);
                    break; // stop processing further cameras when cancellation requested
                }
                catch (HttpRequestException hre)
                {
                    // network / connection issues - log and continue to next camera
                    await _loggerService.LogData(JToken.FromObject(hre.Message), "Warning", _endpointConfig.MessageType, FormatUrl);
                    await _camera.Delete(cameraId);
                }
                catch (System.Net.Sockets.SocketException se)
                {
                    // lower-level socket errors - log and continue
                    await _loggerService.LogData(JToken.FromObject(se.Message), "Warning", _endpointConfig.MessageType, FormatUrl);
                }
                catch (Exception e)
                {
                    // unexpected errors - log and continue
                    await _loggerService.LogData(JToken.FromObject(e.Message), "Error", _endpointConfig.MessageType, FormatUrl);
                }
            }
        }
        private async Task ProcessCameraData(JToken result)
        {
            if (result is null)
            {
                await _loggerService.LogData(JToken.FromObject("ProcessCameraData called with null result"), "Warning", _endpointConfig.MessageType, _endpointConfig.Url);
                return;
            }

            try
            {
                var bicamList = result.ToObject<List<BICAMCameras>>() ?? new List<BICAMCameras>();

                var cameraList = bicamList.Select(bicam => new Cameras
                {
                    Id = bicam.CameraName,
                    DisplayName = bicam.FacilityDisplayName,
                    CameraDirection = bicam.CameraName,
                    LocaleKey = bicam.LocaleKey,
                    ModelNum = bicam.ModelNum,
                    FacilityPhysAddrTxt = bicam.FacilityPhysAddrTxt,
                    GeoProcRegionNm = bicam.GeoProcRegionNm,
                    FacilitySubtypeDesc = bicam.FacilitySubtypeDesc,
                    GeoProcDivisionNm = bicam.GeoProcDivisionNm,
                    AuthKey = bicam.AuthKey,
                    FacilityLatitudeNum = bicam.FacilitiyLatitudeNum,
                    FacilityLongitudeNum = bicam.FacilitiyLongitudeNum,
                    CameraName = bicam.CameraName,
                    IP = bicam.CameraName,
                    Description = bicam.Description,
                    Reachable = bicam.Reachable,
                    Type = "Cameras",
                }).ToList();

                if (cameraList.Count > 0)
                {
                    await _camera.LoadCameraData(cameraList);
                }
                else
                {
                    await _loggerService.LogData(JToken.FromObject("No camera data to load after processing result"), "Information", _endpointConfig.MessageType, _endpointConfig.Url);
                }
            }
            catch (Exception e)
            {
                await _loggerService.LogData(JToken.FromObject(e.Message), "Error", _endpointConfig.MessageType, _endpointConfig.Url);
            }
        }

        public class BICAMCameras
        {
            [JsonProperty("LOCALE_KEY")]
            public string LocaleKey { get; set; } = "";

            [JsonProperty("MODEL_NUM")]
            public string ModelNum { get; set; } = "";

            [JsonProperty("FACILITY_PHYS_ADDR_TXT")]
            public string FacilityPhysAddrTxt { get; set; } = "";

            [JsonProperty("GEO_PROC_REGION_NM")]
            public string GeoProcRegionNm { get; set; } = "";

            [JsonProperty("FACILITY_SUBTYPE_DESC")]
            public string FacilitySubtypeDesc { get; set; } = "";

            [JsonProperty("GEO_PROC_DIVISION_NM")]
            public string GeoProcDivisionNm { get; set; } = "";

            [JsonProperty("AUTH_KEY")]
            public string AuthKey { get; set; } = "";

            [JsonProperty("FACILITY_LATITUDE_NUM")]
            public double FacilitiyLatitudeNum { get; set; } = 0.0;

            [JsonProperty("FACILITY_LONGITUDE_NUM")]
            public double FacilitiyLongitudeNum { get; set; } = 0.0;

            [JsonProperty("DESCRIPTION")]
            public string Description { get; set; } = "";

            [JsonProperty("CAMERA_NAME")]
            public string CameraName { get; set; } = "";

            [JsonProperty("REACHABLE")]
            public string Reachable { get; set; } = "";

            [JsonProperty("FACILITY_DISPLAY_NME")]
            public string FacilityDisplayName { get; set; } = "";
        }
    }
}
