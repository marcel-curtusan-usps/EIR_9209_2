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
        public CameraEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, IInMemorySiteInfoRepository siteInfo, IInMemoryCamerasRepository camera)
                : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection)
        {
            _siteInfo = siteInfo;
            _camera = camera;
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
                if (_endpointConfig.MessageType == "Cameras")
                {
                    SiteInformation siteinfo = _siteInfo.GetSiteInfo();
                    if (siteinfo != null)
                    {
                        //process tag data
                        FormatUrl = string.Format(_endpointConfig.Url, siteinfo.FdbId);
                        queryService = new QueryService(_logger, _httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));

                        var cresult = await queryService.GetCameraData(stoppingToken);
                        // Process the data as needed
                        _ = Task.Run(() => ProcessCameraData(cresult), stoppingToken).ConfigureAwait(false);
                    }
                }
                if (_endpointConfig.MessageType == "getCameraStills")
                {
                    //process camera list and get pictures
                    foreach (CameraGeoMarker camera in _camera.GetAll())
                    {
                        byte[] result = Array.Empty<byte>();
                        try
                        {
                            if (string.IsNullOrEmpty(camera.Properties.CameraName))
                            {
                                continue;
                            }
                            FormatUrl = string.Format(_endpointConfig.Url, camera.Properties.CameraName);

                            using (var httpClient = new HttpClient())
                            {
                                result = await httpClient.GetByteArrayAsync(FormatUrl);
                                _ = Task.Run(() => _camera.LoadCameraStills(result, camera.Properties.Id), stoppingToken).ConfigureAwait(false);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e.Message);
                            result = Array.Empty<byte>();
                         _ =  Task.Run(() => _camera.LoadCameraStills(result, camera.Properties.Id), stoppingToken).ConfigureAwait(false);
                        }
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
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, cancellationToken: stoppingToken);
                }
            }
           
        }
        private async Task ProcessCameraData(JToken result)
        {
            try
            {
                if (result is not null)
                {
                    List<BICAMCameras>? BICAMcameraList = result.ToObject<List<BICAMCameras>>();
                    List<Cameras>? cameraList = new List<Cameras>();
                    foreach (var BICAMcamera in BICAMcameraList)
                    {
                        cameraList.Add(new Cameras
                        {
                            Id = BICAMcamera.CameraName,
                            DisplayName = BICAMcamera.FacilityDisplayName,
                            CameraDirection = BICAMcamera.CameraName,
                            LocaleKey = BICAMcamera.LocaleKey,
                            ModelNum = BICAMcamera.ModelNum,
                            FacilityPhysAddrTxt = BICAMcamera.FacilityPhysAddrTxt,
                            GeoProcRegionNm = BICAMcamera.GeoProcRegionNm,
                            FacilitySubtypeDesc = BICAMcamera.FacilitySubtypeDesc,
                            GeoProcDivisionNm = BICAMcamera.GeoProcDivisionNm,
                            AuthKey = BICAMcamera.AuthKey,
                            FacilityLatitudeNum = BICAMcamera.FacilitiyLatitudeNum,
                            FacilityLongitudeNum = BICAMcamera.FacilitiyLongitudeNum,
                            CameraName = BICAMcamera.CameraName,
                            IP = BICAMcamera.CameraName,
                            Description = BICAMcamera.Description,
                            Reachable = BICAMcamera.Reachable,
                            Type = "Cameras",

                        });
                    }

                    if (cameraList != null && cameraList.Any())
                    {
                        await Task.Run(() => _camera.LoadCameraData(cameraList)).ConfigureAwait(false);
                    }

                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
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
