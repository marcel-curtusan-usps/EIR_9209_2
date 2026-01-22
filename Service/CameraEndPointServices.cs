using EIR_9209_2.DataStore;
using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;


namespace EIR_9209_2.Service
{
    /// <summary>
    /// This class is responsible for fetching data from the Camera endpoint and processing it.
    /// </summary>
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
        /// <summary>
        /// Fetches data from the endpoint based on the message type specified in the configuration.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
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
            // stop if cancellation requested
            if (stoppingToken.IsCancellationRequested)
            {
                await _loggerService.LogData(JToken.FromObject("Cancellation requested - stopping camera stills fetch loop."), "Information", _endpointConfig.MessageType, _endpointConfig.Url);
                return;
            }
            // The Cameras URL expected server and site FdbId (see previous implementation)
            SiteInformation siteinfo = await _siteInfo.GetSiteInfo();
            if (siteinfo == null)
            {
                await _loggerService.LogData(JToken.FromObject("Site information not available for camera list fetch"), "Warning", _endpointConfig.MessageType, _endpointConfig.Url);
                return;
            }
            string FormatUrl = "";
            Dictionary<string, string> UrlParameters = new Dictionary<string, string>();

            if (_endpointConfig.Url.Contains("ServerIpOrHostName"))
            {
                UrlParameters.Add("ServerIpOrHostName", server);
            }
            else
            {
                UrlParameters.Add("0", server);
            }

            if (_endpointConfig.Url.Contains("SiteFDBID"))
            {
                UrlParameters.Add("SiteFDBID", siteinfo.FdbId);
            }
            else
            {
                UrlParameters.Add("1", siteinfo.FdbId);
            }

            FormatUrl = BuildUrl(_endpointConfig.Url, UrlParameters);
            var queryService = new QueryService(_loggerService, _httpClientFactory, jsonSettings,
            new QueryServiceSettings(new Uri(FormatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));

            var cresult = await queryService.GetCameraData(stoppingToken);
            if (_endpointConfig.LogData)
            {
                _ = Task.Run(() => _loggerService.LogData(cresult.ToString(),
                 _endpointConfig.MessageType,
                 _endpointConfig.Name,
                 FormatUrl), stoppingToken);
            }
            _endpointConfig.Status = EWorkerServiceState.Idle;
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
            var cameras = await _camera.GetAll();
            foreach (CameraGeoMarker camera in cameras)
            {
                // stop if cancellation requested
                if (stoppingToken.IsCancellationRequested)
                {
                    await _loggerService.LogData(JToken.FromObject("Cancellation requested - stopping camera stills fetch loop."), "Information", _endpointConfig.MessageType, _endpointConfig.Url);
                    break;
                }

                if (camera == null || camera.Properties == null || string.IsNullOrEmpty(camera.Properties.CameraName))
                {
                    continue;
                }

                await ProcessSingleCameraStillsAsync(camera, stoppingToken);
            }
        }

        private async Task ProcessSingleCameraStillsAsync(CameraGeoMarker camera, CancellationToken stoppingToken)
        {
            var cameraName = camera.Properties.CameraName;
            var cameraZoneId = camera?.Properties?.Id ?? string.Empty;
            byte[] imageResult = Array.Empty<byte>();

            var (resolved, ipv4, hostName) = await NslookupAsync(cameraName);
            if (!resolved)
            {
                await _loggerService.LogData(JToken.FromObject($"Camera {cameraName} is not reachable."), "Warning", _endpointConfig.MessageType, _endpointConfig.Url);
                await _camera.LoadCameraStills(Array.Empty<byte>(), cameraZoneId);
                return;
            }

            if (!string.IsNullOrEmpty(hostName) && !hostName.StartsWith("AXIS-", StringComparison.OrdinalIgnoreCase))
            {
                // Non-AXIS device - skip
                return;
            }

            string cameraId = camera?.Properties?.CameraId.ToString() ?? "";
            Dictionary<string, string> urlParameters = new Dictionary<string, string>();

            if (_endpointConfig.Url.Contains("ServerIpOrHostName"))
                urlParameters.Add("ServerIpOrHostName", hostName);
            else
                urlParameters.Add("0", hostName);

            if (_endpointConfig.Url.Contains("CameraId"))
                urlParameters.Add("CameraId", cameraId);
            else
                urlParameters.Add("1", cameraId);

            string formatUrl = BuildUrl(_endpointConfig.Url, urlParameters);

            try
            {
                // Use IHttpClientFactory to obtain an HttpClient (do not create per-iteration new instances)
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromMilliseconds(_endpointConfig.MillisecondsTimeout);
                var response = await httpClient.GetAsync(formatUrl, stoppingToken);
                response.EnsureSuccessStatusCode();
                imageResult = await response.Content.ReadAsByteArrayAsync();

                if (_endpointConfig.LogData)
                {
                    _ = Task.Run(() => _loggerService.LogData(imageResult.ToString(),
                        _endpointConfig.MessageType,
                        _endpointConfig.Name,
                        formatUrl), stoppingToken);
                }

                await _camera.LoadCameraStills(imageResult, cameraZoneId);
            }
            catch (HttpRequestException hre)
            {
                await _loggerService.LogData(JToken.FromObject(hre.Message), "Warning", _endpointConfig.MessageType, formatUrl);
                await _camera.LoadCameraStills(imageResult, cameraZoneId);
            }
            catch (Exception e)
            {
                await _loggerService.LogData(JToken.FromObject(e.Message), "Error", _endpointConfig.MessageType, formatUrl);
                await _camera.LoadCameraStills(imageResult, cameraZoneId);
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
                var bicamList = result.ToObject<List<BicamCameras>>() ?? new List<BicamCameras>();

                var cameraTasks = bicamList.Select(async bicam =>
                {
                    var camerasItem = new Cameras
                    {
                        DisplayName = bicam.FacilityDisplayName,
                        LocaleKey = bicam.LocaleKey,
                        ModelNum = bicam.ModelNum,
                        FacilityPhysAddrTxt = bicam.FacilityPhysAddrTxt,
                        GeoProcRegionNm = bicam.GeoProcRegionNm,
                        FacilitySubtypeDesc = bicam.FacilitySubtypeDesc,
                        GeoProcDivisionNm = bicam.GeoProcDivisionNm,
                        AuthKey = bicam.AuthKey,
                        FacilityLatitudeNum = bicam.FacilitiyLatitudeNum,
                        FacilityLongitudeNum = bicam.FacilitiyLongitudeNum,
                        Description = bicam.Description,
                        CameraId = bicam.CameraId,
                        CameraName = bicam.CameraName,
                        Type = "Cameras",
                    };

                    try
                    {
                        var ns = await NslookupAsync(bicam.CameraName);
                        camerasItem.IP = ns.IPv4 ?? string.Empty;

                        var ping = await PingHostAsync(string.IsNullOrWhiteSpace(ns.HostName) ? (ns.IPv4 ?? bicam.CameraName) : ns.HostName);
                        camerasItem.Reachable = ping.Success;
                    }
                    catch
                    {
                    
                        camerasItem.IP = string.Empty;
                        camerasItem.Reachable = false;
                    }

                    return camerasItem;
                }).ToList();

                var cameras = (await Task.WhenAll(cameraTasks)).Where(c => c != null).ToList();
                if (cameras.Count > 0)
                {
                    await _camera.LoadCameraData(cameras);
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
        public static async Task<(bool Resolved, string IPv4, string HostName)> NslookupAsync(string client, int timeoutMs = 1000)
        {
            if (string.IsNullOrWhiteSpace(client))
                return (false, string.Empty, string.Empty);

            // sanitize input: remove surrounding brackets (IPv6), strip port if present
            string sanitized = client.Trim();
            if (sanitized.StartsWith("[") && sanitized.EndsWith("]"))
            {
                sanitized = sanitized.Substring(1, sanitized.Length - 2);
            }

            // if there's a port (host:port) remove it
            var colonIndex = sanitized.LastIndexOf(':');
            if (colonIndex > 0)
            {
                // check if this is an IPv6 address (contains multiple colons)
                if (sanitized.Count(c => c == ':') == 1)
                {
                    sanitized = sanitized.Substring(0, colonIndex);
                }
            }

            string ipv4 = string.Empty;
            string hostName = string.Empty;

            try
            {
                using (var cts = new CancellationTokenSource(timeoutMs))
                {
                    Task<IPHostEntry> lookupTask;

                    if (IPAddress.TryParse(sanitized, out IPAddress ipAddress))
                        lookupTask = Dns.GetHostEntryAsync(ipAddress);
                    else
                        lookupTask = Dns.GetHostEntryAsync(sanitized);

                    var completedTask = await Task.WhenAny(lookupTask, Task.Delay(timeoutMs, cts.Token));
                    if (completedTask != lookupTask)
                        return (false, string.Empty, string.Empty); // Timeout

                    var entry = await lookupTask;

                    hostName = entry?.HostName ?? string.Empty;
                    var ipv4Addr = entry?.AddressList != null
                        ? Array.Find(entry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork)
                        : null;

                    if (ipv4Addr != null)
                        ipv4 = ipv4Addr.ToString();

                    bool resolved = !string.IsNullOrEmpty(ipv4) || !string.IsNullOrEmpty(hostName);
                    return (resolved, ipv4, hostName);
                }
            }
            catch (SocketException se)
            {
                System.Diagnostics.Debug.WriteLine($"Nslookup SocketException for '{client}': {se.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Nslookup Exception for '{client}': {ex.Message}");
            }

            return (false, string.Empty, string.Empty);
        }

        public static async Task<(bool Success, long RoundtripTime, string Status)> PingHostAsync(string target, int timeoutMs = 5000)
        {
            if (string.IsNullOrWhiteSpace(target))
                return (false, -1, "Invalid target");

            try
            {
                using (var ping = new System.Net.NetworkInformation.Ping())
                {
                    var reply = await ping.SendPingAsync(target, timeoutMs);
                    return (reply.Status == IPStatus.Success, reply.RoundtripTime, reply.Status.ToString());
                }
            }
            catch (PingException ex)
            {
                return (false, -1, $"PingException: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, -1, $"Exception: {ex.Message}");
            }
        }
        public class BicamCameras
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

            [JsonProperty("CAMERA_ID")]
            public int CameraId { get; set; } = 0;

            [JsonProperty("FACILITY_DISPLAY_NME")]
            public string FacilityDisplayName { get; set; } = "";
        }
    }
}
