
using EIR_9209_2.DataStore;
using EIR_9209_2.Service;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using static EIR_9209_2.Models.GeoMarker;

namespace EIR_9209_2.Utilities
{

    public class ResetApplication : IResetApplication
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IInMemoryGeoZonesRepository _geoZones;
        private readonly IInMemoryConnectionRepository _connections;
        private readonly IInMemoryBackgroundImageRepository _backgroundImage;
        private readonly IInMemoryTagsRepository _tags;
        private readonly IInMemoryEmailRepository _email;
        private readonly IInMemorySiteInfoRepository _siteInfo;
        private readonly IInMemoryEmployeesRepository _employees;
        private readonly IInMemoryTACSReports _tacs;
        private readonly IInMemoryCamerasRepository _cameras;
        private readonly IFileService _fileService;
        private readonly IInMemoryEmployeesSchedule _schedule;
        private readonly Worker _worker;

        public ResetApplication(ILogger<ResetApplication> logger, 
            IConfiguration configuration,
            IInMemoryGeoZonesRepository geoZones, 
            IInMemoryTagsRepository tags,
            IInMemoryBackgroundImageRepository backgroundImage,
            IInMemoryConnectionRepository connections,
            IInMemoryEmailRepository email,
            IInMemorySiteInfoRepository siteInfo,
            IInMemoryEmployeesRepository employees,
            IInMemoryTACSReports tacs,
            IInMemoryCamerasRepository cameras,
            IFileService fileService,
            IInMemoryEmployeesSchedule schedule,
            Worker worker)
        {
            _logger = logger;
            _configuration = configuration;
            _geoZones = geoZones;
            _tags = tags;
            _backgroundImage = backgroundImage;
            _cameras = cameras;
            _connections = connections;
            _email = email;
            _siteInfo = siteInfo;
            _employees = employees;
            _tacs = tacs;
            _worker = worker;
            _fileService = fileService;
            _schedule = schedule;
        }

        public async Task<bool> GetNewSiteInfo(string? newNassCode)
        {
            try
            {
                // Step 1: Get the URL from the configuration
                var applicationSettings = _configuration.GetSection("ApplicationConfiguration");
                var url = applicationSettings.GetSection("SiteDetailsUrl");
                var nassCode = newNassCode;
                if (string.IsNullOrEmpty(url.Value))
                {
                    _logger.LogError("Site Details Url is not configured.");
                    return false;
                }
                var FormatUrl = string.Concat(url.Value, nassCode);
                // Step 2: Make an HTTP GET request to the URL
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(FormatUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Failed to fetch site info. Status code: {response.StatusCode}");
                        return false;
                    }

                    // Step 3: Deserialize the response into a SiteInformation object
                    var responseData = await response.Content.ReadAsStringAsync();
                    var newSiteInfo = JsonConvert.DeserializeObject<List<SiteInformation>>(responseData)?.FirstOrDefault();
                    if (newSiteInfo == null)
                    {
                        _logger.LogError("Failed to de-serialize site info.");
                        return false;
                    }

                    if (await _siteInfo.ResetSiteInfoList())
                    {
                        // Step 4: Add the new SiteInformation object to _siteInfo
                        _siteInfo.Add(newSiteInfo);
                    }
                   
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return false;
            }
        }

        public async Task<bool> Reset()
        {
            try
            {
                if (await _worker.DeactivateAllEndpoints())
                {
                    var clearZones = await _geoZones.ResetGeoZoneList();
                    var clearTags = await _tags.ResetTagList();
                    var clearBackgroundImage = await _backgroundImage.ResetBackgroundImageList();
                    var clearConnections = await _connections.ResetConnectionsList();
                    var clearEmails = await _email.ResetEmailsList();
                    var clearSiteInfo = await _siteInfo.ResetSiteInfoList();
                    var clearEmployees = await _employees.Reset();
                    var clearTacs = await _tacs.Reset();
                    var clearSchedule = await _schedule.ResetScheduleList();
                    var clearCameras = await _cameras.ResetCamerasList();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return false;
            }
        }

        public async Task<bool> Setup()
        {
            try
            {
                var clearSiteInfo = await _siteInfo.SetupSiteInfoList();
                var setupZones = await _geoZones.SetupGeoZoneData();
                var setupTags = await _tags.SetupTagList();
                var setupBackgroundImage = await _backgroundImage.SetupBackgroundImageList();
                var setupEmails = await _email.SetupEmailsList();
                var setupEmployees = await _employees.Setup();
                var setupTacs = await _tacs.Setup();
                if (await _connections.SetupConnectionsList())
                {
                    foreach (var endpoint in await _connections.GetAll())
                    {
                        _worker.AddEndpoint(endpoint);
                    }
                }
                
                return true;

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return false;
            }
        }
    }
}