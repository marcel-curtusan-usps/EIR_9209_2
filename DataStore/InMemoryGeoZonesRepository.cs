using EIR_9209_2.Utilities.Extensions;
using EIR_9209_2.DataStore;
using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using static EIR_9209_2.DataStore.InMemoryCamerasRepository;
/// <summary>
/// InMemoryGeoZonesRepository is responsible for managing geo zones in memory.
/// </summary>
public class InMemoryGeoZonesRepository : IInMemoryGeoZonesRepository
{
    // Lock for RunMPESummaryReport

    private readonly ConcurrentDictionary<string, GeoZone> _geoZoneList = new();
    private readonly ConcurrentDictionary<string, GeoZoneDockDoor> _geoZoneDockDoorList = new();
    private readonly ConcurrentDictionary<string, GeoZoneKiosk> _geoZonekioskList = new();
    private readonly ConcurrentDictionary<string, GeoZoneCube> _geoZoneCubeList = new();
    private readonly ConcurrentDictionary<string, Dictionary<DateTime, MPESummary>> _mpeSummary = new();
    private readonly ConcurrentDictionary<DateTime, List<AreaDwell>> _QREAreaDwellResults = new();
    private readonly ConcurrentDictionary<DateTime, List<ReportContentItems>> _QREReportContentResults = new();
    private readonly ConcurrentDictionary<string, MPEActiveRun> _MPERunActivity = new();
    private readonly ConcurrentDictionary<string, MPEActiveRun> _MPEStandard = new();
    private readonly ConcurrentDictionary<string, TargetHourlyData> _MPETargets = new();
    private readonly ConcurrentDictionary<string, ReportResponse> _QREReportRequests = new();
    private readonly List<string> _MPENameList = [];
    private readonly List<string> _DockDoorList = [];
    private readonly List<string> _BullpenList = [];
    private readonly List<string> _AGVLocationList = [];
    private readonly IInMemorySiteInfoRepository _siteInfo;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InMemoryGeoZonesRepository> _logger;
    private readonly IFileService _fileService;
    protected readonly IHubContext<HubServices> _hubServices;
    private readonly IInMemoryEmployeesRepository _employeesRepository;
    private readonly string fileName = "Zones.json";
    private readonly string fileNameReportsRequest = "ReportRequests.json";
    private readonly string fileNameDockDoor = "ZonesDockDoor.json";
    private readonly string fileNameMpeTarget = "MPETargets.json";
    private readonly string fileNameKiosk = "ZonesKiosk.json";
    private readonly string fileNameCube = "ZonesCubes.json";
    /// <summary>
    /// Constructor for InMemoryGeoZonesRepository
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="configuration"></param>
    /// <param name="fileService"></param>
    /// <param name="hubServices"></param>
    /// <param name="siteInfo"></param>
    /// <param name="employeesRepository"></param>
    public InMemoryGeoZonesRepository(ILogger<InMemoryGeoZonesRepository> logger, IConfiguration configuration,
    IFileService fileService, IHubContext<HubServices> hubServices, IInMemorySiteInfoRepository siteInfo, IInMemoryEmployeesRepository employeesRepository)
    {
        _fileService = fileService;
        _logger = logger;
        _configuration = configuration;
        _hubServices = hubServices;
        _siteInfo = siteInfo;
        _employeesRepository = employeesRepository;
        // Load data from the first file into the first collection
        LoadDataFromFile().Wait();
        // Load data from the first file into the first collection
        LoadDockDoorDataFromFile().Wait();
        // Load MPE Targets data from the first file into the first collection
        LoadMPETargetDataFromFile().Wait();
        // Load Kiosk Data from the first file into the first collection
        LoadKioskDataFromFile().Wait();
        // Load Kiosk Data from the first file into the first collection
        LoadCubesDataFromFile().Wait();
        // Load data from the first file into the first collection
        LoadReportsRequestDataFromFile().Wait();
    }



    public async Task<GeoZone?> Add(GeoZone geoZone)
    {
        bool saveToFile = false;
        try
        {
            if (!Regex.IsMatch(geoZone.Properties.Type, "^(MPE)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)))
            {
                geoZone.Properties.MPERunPerformance = null;
            }
            if (_geoZoneList.TryAdd(geoZone.Properties.Id, geoZone))
            {
                saveToFile = true;
                return await Task.FromResult(geoZone);
            }
            else
            {
                _logger.LogError("Zone File {FileName} list was not saved...", fileName);
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileName, GeoZoneOutPutdata(_geoZoneList.Select(x => x.Value).ToList()));
            }
        }
    }

    public async Task<GeoZone?> Remove(string geoZoneId)
    {
        bool saveToFile = false;
        try
        {
            if (_geoZoneList.ContainsKey(geoZoneId) && _geoZoneList.TryRemove(geoZoneId, out GeoZone geoZone))
            {
                saveToFile = true;
                return await Task.FromResult(geoZone);
            }
            else
            {
                _logger.LogError("Zone File {FileName} list was not saved...", fileName);
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileName, GeoZoneOutPutdata(_geoZoneList.Select(x => x.Value).ToList()));
            }
        }
    }

    public async Task<GeoZoneDockDoor?> AddDockDoor(GeoZoneDockDoor newgeoZone)
    {
        bool saveToFile = false;
        try
        {

            if (_geoZoneDockDoorList.TryAdd(newgeoZone.Properties.Id, newgeoZone))
            {
                if (string.IsNullOrEmpty(newgeoZone.Properties.DoorNumber))
                {
                    string dockNumber = newgeoZone.Properties.Name.Replace("DockDoor", "");
                    //check if dock number is number 
                    if (Int32.TryParse(dockNumber, out int doornumber))
                    {
                        newgeoZone.Properties.DoorNumber = doornumber.ToString();
                    }
                    else
                    {
                        newgeoZone.Properties.DoorNumber = dockNumber;
                    }
                }
                if (!_DockDoorList.Contains(newgeoZone.Properties.DoorNumber))
                {
                    _DockDoorList.Add(newgeoZone.Properties.DoorNumber);
                }
                saveToFile = true;
                return await Task.FromResult(newgeoZone);
            }
            else
            {
                _logger.LogError("Dockdoor Zone File {FileNameDockDoor} list was not saved...", fileNameDockDoor);
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileNameDockDoor, GeoZoneDockDoorOutPutdata(_geoZoneDockDoorList.Select(x => x.Value).ToList()));
            }
        }
    }

    public async Task<GeoZoneDockDoor?> RemoveDockDoor(string geoZoneId)
    {
        bool saveToFile = false;
        try
        {
            if (_geoZoneDockDoorList.ContainsKey(geoZoneId) && _geoZoneDockDoorList.TryRemove(geoZoneId, out GeoZoneDockDoor geoZone))
            {
                saveToFile = true;
                return await Task.FromResult(geoZone);
            }
            else
            {
                _logger.LogError("Dock door Zone File {FileNameDockDoor} list was not saved...", fileNameDockDoor);
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileNameDockDoor, GeoZoneDockDoorOutPutdata(_geoZoneDockDoorList.Select(x => x.Value).ToList()));
            }
        }
    }

    public Task<GeoZoneDockDoor> UpdateDockDoor(GeoZoneDockDoor geoZone)
    {
        throw new NotImplementedException();
    }
    public async Task<GeoZone?> UiUpdate(Properties NewGeoZoneProperties)
    {
        bool saveToFile = false;
        try
        {
            if (_geoZoneList.ContainsKey(NewGeoZoneProperties.Id) && _geoZoneList.TryGetValue(NewGeoZoneProperties.Id, out GeoZone? currentGeoZone))
            {
                if (currentGeoZone.Properties.Name != NewGeoZoneProperties.Name)
                {
                    currentGeoZone.Properties.Name = NewGeoZoneProperties.Name;
                    saveToFile = true;
                }
                if (currentGeoZone.Properties.MpeGroup != NewGeoZoneProperties.MpeGroup)
                {
                    currentGeoZone.Properties.MpeGroup = NewGeoZoneProperties.MpeGroup;
                    saveToFile = true;
                }
                if (currentGeoZone.Properties.MpeName != NewGeoZoneProperties.MpeName)
                {
                    currentGeoZone.Properties.MpeName = NewGeoZoneProperties.MpeName;
                    saveToFile = true;
                }
                if (currentGeoZone.Properties.MpeNumber != NewGeoZoneProperties.MpeNumber)
                {
                    currentGeoZone.Properties.MpeNumber = NewGeoZoneProperties.MpeNumber;
                    saveToFile = true;
                }
                if (currentGeoZone.Properties.MpeIpAddress != NewGeoZoneProperties.MpeIpAddress)
                {
                    currentGeoZone.Properties.MpeIpAddress = NewGeoZoneProperties.MpeIpAddress;
                    saveToFile = true;
                }
                if (currentGeoZone.Properties.LDC != NewGeoZoneProperties.LDC)
                {
                    currentGeoZone.Properties.LDC = NewGeoZoneProperties.LDC;
                    saveToFile = true;
                }
                if (currentGeoZone.Properties.RejectBins != NewGeoZoneProperties.RejectBins)
                {
                    currentGeoZone.Properties.RejectBins = NewGeoZoneProperties.RejectBins;
                    saveToFile = true;
                }
                if (currentGeoZone.Properties.PayLocation != NewGeoZoneProperties.PayLocation)
                {
                    currentGeoZone.Properties.PayLocation = NewGeoZoneProperties.PayLocation;
                    saveToFile = true;
                }
                if (currentGeoZone.Properties.PayLocationColor != NewGeoZoneProperties.PayLocationColor)
                {
                    currentGeoZone.Properties.PayLocationColor = NewGeoZoneProperties.PayLocationColor;
                    saveToFile = true;
                }
                if (currentGeoZone.Properties.ExternalUrl != NewGeoZoneProperties.ExternalUrl)
                {
                    currentGeoZone.Properties.ExternalUrl = NewGeoZoneProperties.ExternalUrl;
                    saveToFile = true;
                }
                return currentGeoZone;
            }
            else
            {
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileName, GeoZoneOutPutdata(_geoZoneList.Select(x => x.Value).ToList()));
            }
        }
    }
    public async Task<object?> Get(string id)
    {
        try
        {
            // Run the LINQ queries asynchronously
            var geoZoneTask = Task.Run(() =>
                _geoZoneList.Where(r => r.Value.Properties.Id == id)
                            .Select(y => y.Value)
                            .FirstOrDefault()
            );

            var dgeoZoneTask = Task.Run(() =>
                _geoZoneDockDoorList.Where(r => r.Value.Properties.Id == id)
                                    .Select(y => y.Value)
                                    .FirstOrDefault()
            );

            // Await the tasks
            var geoZone = await geoZoneTask;
            var dgeoZone = await dgeoZoneTask;

            // Return the result based on which one is found
            if (geoZone != null)
            {
                return geoZone;
            }
            else
            {
                return dgeoZone;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return null;
        }
    }
    /// <summary>
    /// Get all the GeoZone data
    /// </summary>
    /// <returns></returns>
    public Task<object> GetAll()
    {
        try
        {

            // Convert GeoZone list to JArray
            JArray geoZones = JArray.FromObject(_geoZoneList.Values.ToList());
            // Convert GeoZoneKiosk list to JArray
            JArray geoZoneKiosk = JArray.FromObject(_geoZonekioskList.Values.ToList());
            // Convert GeoZoneDockdoor list to JArray
            JArray geoZoneDockdoor = JArray.FromObject(_geoZoneDockDoorList.Values.ToList());
            // Convert GeoZoneCude list to JArray
            JArray geoZoneCube = JArray.FromObject(_geoZoneCubeList.Values.ToList());
            // Merge the two geoZoneCube into a single JArray
            geoZones.Merge(geoZoneCube, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Concat,

            });
            // Merge the two geoZoneKiosk into a single JArray
            geoZones.Merge(geoZoneKiosk, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Concat,

            });
            // Merge the two geoZoneDockdoor into a single JArray
            geoZones.Merge(geoZoneDockdoor, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Concat,

            });
            return Task.FromResult((object)geoZones);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Task.FromResult<object>(new object());
        }
    }
    public async Task<List<GeoZoneDockDoor>?> GetDockDoor()
    {
        return _geoZoneDockDoorList.Where(r => r.Value.Properties.Type == "DockDoor").Select(y => y.Value).ToList();
    }
    public object getMPESummary(string area)
    {
        return _mpeSummary.Where(r => Regex.IsMatch(r.Key, "(" + area + ")", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10))).Select(r => r.Value).ToList();

    }
    public Task<List<MPESummary>> getMPESummaryDateRange(string area, DateTime startDT, DateTime endDT)
    {
        var ty = startDT.Date;
        //i want to select all the area that matches the area and is between startDT and endDT
        var result = _mpeSummary.Where(r => Regex.IsMatch(r.Key, "(" + area + ")", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10))).SelectMany(y => y.Value)
            .Where(u => u.Key >= startDT && u.Key <= endDT).Select(y => y.Value).ToList();
        return Task.FromResult(result);
    }
    public List<MPEActiveRun> getMPERunActivity(string area)
    {
        return _MPERunActivity.Where(r => Regex.IsMatch(r.Key, "(" + area + ")", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10))).Select(r => r.Value).ToList();

    }

    public bool ExistingAreaDwell(DateTime hour)
    {
        return _QREAreaDwellResults.ContainsKey(hour);
    }
    public List<AreaDwell> GetAreaDwell(DateTime hour)
    {
        return _QREAreaDwellResults.TryGetValue(hour, out List<AreaDwell>? value) ? value : [];
    }
    public void UpdateAreaDwell(DateTime hour, List<AreaDwell> newValue, List<AreaDwell> currentvalue)
    {
        bool savetoFile = false;
        try
        {
            if (_QREAreaDwellResults.TryUpdate(hour, newValue, currentvalue))
            {
                savetoFile = true;
            }

        }
        catch (Exception e)
        {
            _logger.LogError($"Error updating Area Dwell {e.Message}");
        }
        finally
        {
            //if (savetoFile)
            //{
            //    FileService.WriteFile(fileTagTimeline, JsonConvert.SerializeObject(_QRETagTimelineResults.Values, Formatting.Indented));
            //}
        }
    }

    public void AddAreaDwell(DateTime hour, List<AreaDwell> newValue)
    {
        bool savetoFile = false;
        try
        {
            if (_QREAreaDwellResults.TryAdd(hour, newValue))
            {
                savetoFile = true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"Error updating  Area Dwell {e.Message}");
        }
        finally
        {
            //if (savetoFile)
            //{
            //    FileService.WriteFile(fileTagTimeline, JsonConvert.SerializeObject(_QRETagTimelineResults.Values, Formatting.Indented));
            //}
        }
    }
    /// <summary>
    ///     Add a report response for the given hour
    /// </summary>
    /// <param name="reportResponse"></param>
    public Task AddReportResponse(ReportResponse reportResponse)
    {
        bool savetoFile = false;
        try
        {
            if (!_QREReportRequests.ContainsKey(reportResponse.ResourceId))
            {
                _QREReportRequests.TryAdd(reportResponse.ResourceId, reportResponse);
                savetoFile = true;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error updating Report Request {e.Message}");
            return Task.FromResult(false);
        }
        finally
        {
            if (savetoFile)
            {
                _fileService.WriteConfigurationFile(fileNameReportsRequest, JsonConvert.SerializeObject(_QREReportRequests.Values, Formatting.Indented));
            }
        }
    }
    /// <summary>
    /// Update report response for the given report ID and state
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="state"></param>
    public Task UpdateReportResponse(string reportId, string state)
    {
        bool savetoFile = false;
        try
        {
            if (_QREReportRequests.ContainsKey(reportId))
            {
                _QREReportRequests[reportId].State = state;
                savetoFile = true;
                return Task.FromResult(true);

            }
            return Task.FromResult(false);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error updating Report Request {e.Message}");
            return Task.FromResult(false);
        }
        finally
        {
            if (savetoFile)
            {
                _fileService.WriteConfigurationFile(fileNameReportsRequest, JsonConvert.SerializeObject(_QREReportRequests.Values, Formatting.Indented));
            }
        }
    }

    /// <summary>
    /// Remove a report response for the given hour
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public Task RemoveReportResponse(string reportId)
    {
        bool savetoFile = false;
        try
        {
            if (_QREReportRequests.ContainsKey(reportId))
            {
                _QREReportRequests.TryRemove(reportId, out _);
                savetoFile = true;
                return Task.FromResult(true);
            }
            else
            {
                _logger.LogError($"Report Response {reportId} not found in the repository.");
                return Task.FromResult(false);
            }

        }
        catch (Exception e)
        {
            _logger.LogError($"Error updating Report Request {e.Message}");
            return Task.FromResult(false);
        }
        finally
        {
            if (savetoFile)
            {
                _fileService.WriteConfigurationFile(fileNameReportsRequest, JsonConvert.SerializeObject(_QREReportRequests.Values, Formatting.Indented));
            }
        }
    }
    /// <summary>
    /// Checks if a report list exists for the given hour.
    /// </summary>
    /// <param name="hour"></param>
    /// <returns></returns>
    public Task<bool> ExistingReportInList(DateTime hour)
    {
        try
        {
            return Task.FromResult(_QREReportRequests.Any(r => r.Value.DateTimeRequestFor == hour));
        }
        catch (Exception e)
        {
            _logger.LogError($"Error checking existing report list {e.Message}");
            return Task.FromResult(false);
        }
    }

    /// <summary>
    ///     Get report responses for the given hour
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<List<ReportResponse>> GetReportList(CancellationToken cancellationToken)
    {
        try
        {
            if (!cancellationToken.IsCancellationRequested && _QREReportRequests.Count > 0)
            {
                return await Task.Run(() => _QREReportRequests.Select(r => r.Value).ToList(), cancellationToken);
            }
            else
            {
                return [];
            }

        }
        catch (Exception e)
        {
            _logger.LogError($"Error getting Report list {e.Message}");
            throw new Exception("Error getting Report list", e);
        }
    }

    /// <summary>
    /// Adds a report content item to the repository.
    /// </summary>
    /// <param name="hour"></param>
    /// <param name="contentItem"></param>
    /// <returns></returns>
    public async Task<bool> AddReportContentItem(DateTime hour, List<ReportContentItems> contentItem)
    {

        try
        {
            foreach (var item in contentItem)
            {
                item.Type = await _employeesRepository.GetEmployeeType(item.IntegrationKey);
            }

            if (_QREReportContentResults.TryAdd(hour, contentItem))
            {
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            _logger.LogError($"Error adding Report Content Item {e.Message}");
            return false;
        }
    }
    private static readonly object _mpeSummaryReportLock = new object();
    private static bool _mpeSummaryReportRunning = false;

    public async void RunMPESummaryReport()
    {
        // Only allow one execution at a time; skip if already running
        if (Monitor.TryEnter(_mpeSummaryReportLock))
        {
            try
            {
                if (_mpeSummaryReportRunning)
                {
                    return; // Another task is already running this method
                }
                _mpeSummaryReportRunning = true;

                List<string> areasList = _geoZoneList.Where(r => r.Value.Properties.Type == "MPE").Select(item => item.Value.Properties.Name).Distinct().ToList();
                if (areasList.Any())
                {
                    foreach (var area in areasList)
                    {
                        var mpe = _geoZoneList.Where(r => r.Value.Properties.Name == area && r.Value.Properties.Type == "MPE").Select(y => y.Value.Properties).FirstOrDefault();
                        if (mpe != null)
                        {
                            List<DateTime>? hoursInMpeDateTime = mpe.MPERunPerformance?.HourlyData.Select(x => x.Hour).ToList();
                            if (!_mpeSummary.ContainsKey(area))
                            {
                                _mpeSummary.TryAdd(area, new Dictionary<DateTime, MPESummary>());
                            }
                            if (hoursInMpeDateTime != null)
                            {
                                foreach (var hour in hoursInMpeDateTime)
                                {
                                    var hourlySummaryForHourAndArea = await GetHourlySummaryForHourAndArea(area, hour, mpe.MPERunPerformance);
                                    if (hourlySummaryForHourAndArea != null)
                                    {
                                        lock (_mpeSummary[area])
                                        {
                                            _mpeSummary[area][hour] = hourlySummaryForHourAndArea;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error running MPE Summary Report");
            }
            finally
            {
                _mpeSummaryReportRunning = false;
                System.Threading.Monitor.Exit(_mpeSummaryReportLock);
            }
        }
        // else: skip execution if lock is not acquired
    }

    private Task<MPESummary>? GetHourlySummaryForHourAndArea(string area, DateTime Dateandhour, MPERunPerformance mpe)
    {
        try
        {
            string hourFormat = Dateandhour.ToString("yyyy-MM-ddTHH:mm:ss");
            var standardPiecseFeed = 0;
            var standardStaffHrs = 0;

            var piecesCountThisHour = mpe.HourlyData.FirstOrDefault(r => r.Hour == Dateandhour)?.Count;
            var piecesSortedThisHour = mpe.HourlyData.FirstOrDefault(r => r.Hour == Dateandhour)?.Sorted;
            var piecesRejectedThisHour = mpe.HourlyData.FirstOrDefault(r => r.Hour == Dateandhour)?.Rejected;
            var actualYieldcal = 0.0;
            var laborHrs = new Dictionary<string, double>();
            var laborCounts = new Dictionary<string, int>();
            var clerkDwellTime = 0.0;
            var maintDwellTime = 0.0;
            var mhDwellTime = 0.0;
            var supervisorDwellTime = 0.0;
            var otherDwellTime = 0.0;
            var clerkPresent = 0;
            var mhPresent = 0;
            var maintPresent = 0;
            var supervisorPresent = 0;
            var otherPresent = 0;
            var piecesForYield = piecesSortedThisHour != 0 ? piecesSortedThisHour : piecesCountThisHour;

            if (_QREReportContentResults.TryGetValue(Dateandhour, out var dwellResults))
            {
                // Materialize to list to avoid 'Collection was modified' exception
                var entriesThisArea = dwellResults.Where(r => r.AreaLabel.Text.ToLower().Contains(area.ToLower())).ToList();
                if (entriesThisArea.Count > 0)
                {
                    var clerkAndMailHandlerCountThisHour = entriesThisArea
                        .Where(e => Regex.IsMatch(e.Type, "^(clerk|mail handler)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)))
                        .Sum(g => g.Duration.TotalMilliseconds) / (1000 * 60 * 60);

                    actualYieldcal = piecesForYield != null && clerkAndMailHandlerCountThisHour > 0
                        ? piecesForYield.Value / clerkAndMailHandlerCountThisHour
                        : 0.0;

                    if (mpe.HourlyData.Where(r => r.Hour == Dateandhour).Select(y => y.Count).Any())
                    {
                        actualYieldcal = mpe.HourlyData
                            .Where(r => r.Hour == Dateandhour)
                            .Select(y => y.Count)
                            .First() /
                            (entriesThisArea
                                .Where(e => Regex.IsMatch(e.Type, "^(clerk|mail handler)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)))
                                .Sum(g => g.Duration.TotalMilliseconds) / (1000 * 60 * 60));
                    }

                    laborHrs = entriesThisArea.GroupBy(e => e.Type)
                        .ToDictionary(g => g.Key, g => g.Sum(e => e.Duration.TotalMilliseconds));
                    laborCounts = entriesThisArea.GroupBy(e => e.Type)
                        .ToDictionary(g => g.Key, g => g.Count());
                    clerkDwellTime = entriesThisArea
                        .Where(e => Regex.IsMatch(e.Type, "^(clerk)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)))
                        .Sum(g => g.Duration.TotalMilliseconds);
                    maintDwellTime = entriesThisArea
                        .Where(e => Regex.IsMatch(e.Type, "^(maintenance)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)))
                        .Sum(g => g.Duration.TotalMilliseconds);
                    mhDwellTime = entriesThisArea
                        .Where(e => Regex.IsMatch(e.Type, "^(mail handler)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)))
                        .Sum(g => g.Duration.TotalMilliseconds);
                    supervisorDwellTime = entriesThisArea
                        .Where(e => Regex.IsMatch(e.Type, "^(supervisor)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)))
                        .Sum(g => g.Duration.TotalMilliseconds);
                    otherDwellTime = entriesThisArea
                        .Where(e => !Regex.IsMatch(e.Type, "^(clerk|supervisor|mail handler|maintenance)", RegexOptions.IgnoreCase))
                        .Sum(g => g.Duration.TotalMilliseconds);
                    clerkPresent = entriesThisArea
                        .Count(e => Regex.IsMatch(e.Type, "^(clerk)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)));
                    mhPresent = entriesThisArea
                        .Count(e => Regex.IsMatch(e.Type, "^(mail handler)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)));
                    maintPresent = entriesThisArea
                        .Count(e => Regex.IsMatch(e.Type, "^(maintenance)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)));
                    supervisorPresent = entriesThisArea
                        .Count(e => Regex.IsMatch(e.Type, "^(supervisor)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)));
                    otherPresent = entriesThisArea
                        .Count(e => !Regex.IsMatch(e.Type, "^(clerk|supervisor|mail handler|maintenance)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)));
                }
            }

            var r = new MPESummary
            {
                laborHrs = laborHrs,
                laborCounts = laborCounts,
                piecesFeed = piecesCountThisHour ?? 0,
                piecesSorted = piecesSortedThisHour ?? 0,
                piecesRejected = piecesRejectedThisHour ?? 0,
                mpeName = area,
                mpeNumber = mpe.MpeNumber,
                hour = hourFormat,
                maintDwellTime = maintDwellTime,
                mhDwellTime = mhDwellTime,
                supervisorDwellTime = supervisorDwellTime,
                otherDwellTime = otherDwellTime,
                clerkPresent = clerkPresent,
                clerkDwellTime = clerkDwellTime,
                mhPresent = mhPresent,
                maintPresent = maintPresent,
                supervisorPresent = supervisorPresent,
                otherPresent = otherPresent,
                actualYield = actualYieldcal,
                standardPiecseFeed = standardPiecseFeed,
                standardStaffHrs = standardStaffHrs
            };
            return Task.FromResult(r);
        }
        catch (Exception e)
        {
            _logger.LogError("An error occurred: {Message}", e.Message);
            return null;
        }
    }

    private async Task LoadReportsRequestDataFromFile()
    {
        try
        {
            // Read data from file
            var fileContent = await _fileService.ReadFile(fileNameReportsRequest);
            if (!string.IsNullOrEmpty(fileContent))
            {
                // Parse the file content to get the data. This depends on the format of your file.
                List<ReportResponse>? data = JsonConvert.DeserializeObject<List<ReportResponse>>(fileContent);

                // Insert the data into the MongoDB collection
                if (data?.Count > 0)
                {
                    foreach (ReportResponse item in data.Select(r => r).ToList())
                    {
                        _QREReportRequests.TryAdd(item.ResourceId, item);
                    }
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            // Handle the FileNotFoundException here
            _logger.LogError($"File not found: {ex.FileName}");
            // You can choose to throw an exception or take any other appropriate action
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            _logger.LogError($"An error occurred when reading the file: {ex.Message}");
        }
        catch (JsonException ex)
        {
            // Handle errors when parsing the JSON
            _logger.LogError($"An error occurred when parsing the JSON: {ex.Message}");
        }
    }
    private async Task LoadDataFromFile()
    {
        try
        {
            // Read data from file
            var fileContent = await _fileService.ReadFile(fileName);
            if (!string.IsNullOrEmpty(fileContent))
            {
                // Parse the file content to get the data. This depends on the format of your file.
                List<GeoZone>? data = JsonConvert.DeserializeObject<List<GeoZone>>(fileContent);

                // Insert the data into the MongoDB collection
                if (data?.Count > 0)
                {
                    foreach (GeoZone item in data.Select(r => r).ToList())
                    {
                        item.Properties.MPERunPerformance = new();
                        _geoZoneList.TryAdd(item.Properties.Id, item);
                        if (string.IsNullOrEmpty(item.Properties.Type))
                        {
                            item.Properties.Type = GetZoneType(item.Properties.Name);
                        }
                        if (item.Properties.Type == "MPE")
                        {
                            _MPENameList.Add(item.Properties.Name);
                        }
                    }
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            // Handle the FileNotFoundException here
            _logger.LogError($"File not found: {ex.FileName}");
            // You can choose to throw an exception or take any other appropriate action
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            _logger.LogError($"An error occurred when reading the file: {ex.Message}");
        }
        catch (JsonException ex)
        {
            // Handle errors when parsing the JSON
            _logger.LogError($"An error occurred when parsing the JSON: {ex.Message}");
        }
    }
    private async Task LoadDockDoorDataFromFile()
    {
        try
        {
            // Read data from file
            var fileContent = await _fileService.ReadFile(fileNameDockDoor);

            // Parse the file content to get the data. This depends on the format of your file.
            // Here's an example if your file was in JSON format and contained an array of T objects:
            List<GeoZoneDockDoor>? data = JsonConvert.DeserializeObject<List<GeoZoneDockDoor>>(fileContent);

            // Insert the data into the MongoDB collection
            if (data?.Count > 0)
            {
                foreach (GeoZoneDockDoor item in data.Select(r => r).ToList())
                {
                    item.Properties.RouteTrips = new();
                    item.Properties.IsTripAtDoor = false;
                    item.Properties.TripDirectionInd = "";
                    item.Properties.RouteTripId = 0;
                    item.Properties.RouteTripLegId = 0;
                    item.Properties.Route = "";
                    item.Properties.LegSiteName = "";
                    item.Properties.LegSiteId = "";
                    item.Properties.Status = "";
                    item.Properties.Trip = "";
                    item.Properties.TripMin = 0;
                    item.Properties.ContainersNotLoaded = 0;

                    //check if door exits in _DockDoorList
                    if (!_DockDoorList.Contains(item.Properties.DoorNumber))
                    {
                        _DockDoorList.Add(item.Properties.DoorNumber);
                    }
                    _geoZoneDockDoorList.TryAdd(item.Properties.DoorNumber, item);
                }

            }
        }
        catch (FileNotFoundException ex)
        {
            // Handle the FileNotFoundException here
            _logger.LogError($"File not found: {ex.FileName}");
            // You can choose to throw an exception or take any other appropriate action
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            _logger.LogError($"An error occurred when reading the file: {ex.Message}");
        }
        catch (JsonException ex)
        {
            // Handle errors when parsing the JSON
            _logger.LogError($"An error occurred when parsing the JSON: {ex.Message}");
        }
    }

    private async Task LoadMPETargetDataFromFile()
    {

        try
        {
            // Read data from file
            var fileContent = await _fileService.ReadFile(fileNameMpeTarget);

            // Parse the file content to get the data. This depends on the format of your file.
            // Here's an example if your file was in JSON format and contained an array of T objects:
            List<TargetHourlyData>? data = JsonConvert.DeserializeObject<List<TargetHourlyData>>(fileContent);

            // Insert the data into the MongoDB collection
            if (data?.Count > 0)
            {
                foreach (TargetHourlyData item in data.Select(r => r).ToList())
                {
                    if (!_MPETargets.ContainsKey(item.Id))
                    {
                        _MPETargets.TryAdd(item.Id, item);
                    }
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            // Handle the FileNotFoundException here
            _logger.LogError($"File not found: {ex.FileName}");
            // You can choose to throw an exception or take any other appropriate action
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            _logger.LogError($"An error occurred when reading the file: {ex.Message}");
        }
        catch (JsonException ex)
        {
            // Handle errors when parsing the JSON
            _logger.LogError($"An error occurred when parsing the JSON: {ex.Message}");
        }
    }
    private string GetZoneType(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "None";
        }

        var agvLocationPattern = _configuration[key: "ZoneConfiguration:AGVLocation"];
        var dockDoorPattern = _configuration[key: "ZoneConfiguration:Dockdoor"];
        var areaPattern = _configuration[key: "ZoneConfiguration:Area"];
        var viewportPattern = _configuration[key: "ZoneConfiguration:Viewport"];

        if (!string.IsNullOrEmpty(agvLocationPattern) && Regex.IsMatch(name, agvLocationPattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)))
        {
            return "AGVLocation";
        }
        if (!string.IsNullOrEmpty(dockDoorPattern) && Regex.IsMatch(name, dockDoorPattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)))
        {
            return "DockDoor";
        }
        if (!string.IsNullOrEmpty(areaPattern) && Regex.IsMatch(name, areaPattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)))
        {
            return "Area";
        }
        if (!string.IsNullOrEmpty(viewportPattern) && Regex.IsMatch(name, viewportPattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)))
        {
            return "ViewPorts";
        }

        return "MPE";
    }
    public async Task<List<string>> GetZoneNameList(string type)
    {
        try
        {
            if (type.Equals("MPE", StringComparison.CurrentCultureIgnoreCase))
            {
                return _MPENameList;
            }
            else if (type.Equals("DockDoor", StringComparison.CurrentCultureIgnoreCase))
            {
                return _DockDoorList;
            }
            else if (type.Equals("AGVLocation", StringComparison.CurrentCultureIgnoreCase))
            {
                return _AGVLocationList;
            }
            else if (type.Equals("Bullpen", StringComparison.CurrentCultureIgnoreCase))
            {
                return _BullpenList;
            }
            else
            {
                // Filter and select from _geoZoneList
                var geoZoneNames = _geoZoneList
                    .Where(r => r.Value.Properties.Type.Normalize() == type.Normalize())
                    .Select(y => y.Value.Properties.Name)
                    .ToList();

                return await Task.FromResult(geoZoneNames);
            }
        }
        catch (Exception e)
        {
            // Handle the exception
            Console.WriteLine($"An error occurred: {e.Message}");
            _logger.LogError(e.Message);
            return null;
        }



    }

    public async Task<bool> UpdateMPERunInfo(List<MPERunPerformance> mpeList, CancellationToken stoppingToken)
    {
        try
        {
            await Task.Run(() => UpdateMPERunActivity(mpeList)).ConfigureAwait(false);
            foreach (MPERunPerformance mpe in mpeList)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    return false;
                }
                mpe.MpeId = string.Concat(mpe.MpeType, "-", mpe.MpeNumber.ToString().PadLeft(3, '0'));
                DateTime CurrentRunEnd = (!string.IsNullOrEmpty(mpe.CurrentRunEnd) && mpe.CurrentRunEnd != "0")
              ? DateTime.ParseExact(mpe.CurrentRunEnd, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
              : await _siteInfo.GetCurrentTimeInTimeZone(DateTime.Now);
                //if mpename not in MPE list add it
                if (!_MPENameList.Contains(mpe.MpeId))
                {
                    _MPENameList.Add(mpe.MpeId);
                }
                //calculate estimated completion time
                int.TryParse(mpe.RpgEstVol, out int RpgEstVol);
                int.TryParse(mpe.CurThruputOphr, out int CurThruputOphr);
                int.TryParse(mpe.TotSortplanVol, out int TotSortplanVol);

                double RpgEstimatedHrs = 0;
                DateTime currentTime = await _siteInfo.GetCurrentTimeInTimeZone(DateTime.Now);
                DateTime RpgEstimatedCompletion = DateTime.MinValue;
                if (string.IsNullOrEmpty(mpe.CurrentRunEnd) || mpe.CurrentRunEnd == "0")
                {
                    if (CurThruputOphr > 0 && RpgEstVol > 0)
                    {
                        RpgEstimatedHrs = ((double)RpgEstVol - (double)TotSortplanVol) / (double)CurThruputOphr;
                        RpgEstimatedCompletion = currentTime.AddHours(RpgEstimatedHrs);
                    }
                }
                int NextOperationId = 0;
                DateTime NextRPGStartDtm = DateTime.MinValue;
                var nextPlan = _MPERunActivity.Where(r => Regex.IsMatch(r.Key, "(" + mpe.MpeId + ")", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)))
                     .Where(u => u.Value.Type == "Plan" && u.Value.CurOperationId.ToString() != mpe.CurOperationId && u.Value.CurrentRunStart >= CurrentRunEnd).OrderBy(r => r.Value.CurrentRunStart).Select(y => y.Value).FirstOrDefault();
                if (nextPlan != null)
                {
                    NextOperationId = nextPlan.CurOperationId;
                    NextRPGStartDtm = nextPlan.CurrentRunStart;
                }
                var geoZone = _geoZoneList.Where(r => r.Value.Properties.Type == "MPE" && r.Value.Properties.Name == mpe.MpeId).Select(y => y.Value).FirstOrDefault();
                if (geoZone != null)
                {
                    bool pushUIUpdate = false;

                    if (string.IsNullOrEmpty(geoZone.Properties.MPERunPerformance?.MpeType))
                    {
                        geoZone.Properties.MPERunPerformance = mpe;
                        geoZone.Properties.MPERunPerformance.MpeId = mpe.MpeId;
                        geoZone.Properties.MPERunPerformance.RpgEstimatedCompletion = RpgEstimatedCompletion;
                        geoZone.Properties.MPERunPerformance.NextOperationId = NextOperationId.ToString();
                        geoZone.Properties.MPERunPerformance.NextRPGStartDtm = NextRPGStartDtm;
                        geoZone.Properties.MPERunPerformance.ZoneId = geoZone.Properties.Id;
                        pushUIUpdate = true;
                    }
                    else
                    {
                        if (geoZone.Properties.MPERunPerformance.MpeId != mpe.MpeId)
                        {
                            geoZone.Properties.MPERunPerformance.MpeId = mpe.MpeId;
                        }
                        if (geoZone.Properties.MPERunPerformance.ZoneId != geoZone.Properties.Id)
                        {
                            geoZone.Properties.MPERunPerformance.ZoneId = geoZone.Properties.Id;
                        }
                        if (geoZone.Properties.DataSource != "IDS")
                        {
                            if (geoZone.Properties.MPERunPerformance.HourlyData != mpe.HourlyData)
                            {
                                geoZone.Properties.MPERunPerformance.HourlyData = mpe.HourlyData;
                                pushUIUpdate = true;
                            }
                        }
                        if (geoZone.Properties.MPERunPerformance.CurSortplan != mpe.CurSortplan)
                        {
                            geoZone.Properties.MPERunPerformance.CurSortplan = mpe.CurSortplan;
                            pushUIUpdate = true;

                        }
                        if (geoZone.Properties.MPERunPerformance.CurThruputOphr != mpe.CurThruputOphr)
                        {
                            geoZone.Properties.MPERunPerformance.CurThruputOphr = mpe.CurThruputOphr;
                            pushUIUpdate = true;
                        }
                        if (geoZone.Properties.MPERunPerformance.TotSortplanVol != mpe.TotSortplanVol)
                        {
                            geoZone.Properties.MPERunPerformance.TotSortplanVol = mpe.TotSortplanVol;
                            pushUIUpdate = true;
                        }
                        if (geoZone.Properties.MPERunPerformance.RpgEstVol != mpe.RpgEstVol)
                        {
                            geoZone.Properties.MPERunPerformance.RpgEstVol = mpe.RpgEstVol;
                            pushUIUpdate = true;
                        }
                        if (geoZone.Properties.MPERunPerformance.CurrentRunStart != mpe.CurrentRunStart)
                        {
                            geoZone.Properties.MPERunPerformance.CurrentRunStart = mpe.CurrentRunStart;
                            pushUIUpdate = true;
                        }
                        if (geoZone.Properties.MPERunPerformance.CurrentRunEnd != mpe.CurrentRunEnd)
                        {
                            geoZone.Properties.MPERunPerformance.CurrentRunEnd = mpe.CurrentRunEnd;
                            pushUIUpdate = true;
                        }
                        if (geoZone.Properties.MPERunPerformance.RpgEstVol != mpe.RpgEstVol)
                        {
                            geoZone.Properties.MPERunPerformance.RpgEstVol = mpe.RpgEstVol;
                            pushUIUpdate = true;
                        }
                        if (geoZone.Properties.MPERunPerformance.CurOperationId != mpe.CurOperationId)
                        {
                            geoZone.Properties.MPERunPerformance.CurOperationId = mpe.CurOperationId;
                            pushUIUpdate = true;
                        }
                        if (geoZone.Properties.MPERunPerformance.UnplanMaintSpStatus != mpe.UnplanMaintSpStatus)
                        {
                            geoZone.Properties.MPERunPerformance.UnplanMaintSpStatus = mpe.UnplanMaintSpStatus;
                            pushUIUpdate = true;
                        }
                        if (geoZone.Properties.MPERunPerformance.OpRunningLateStatus != mpe.OpRunningLateStatus)
                        {
                            geoZone.Properties.MPERunPerformance.OpRunningLateStatus = mpe.OpRunningLateStatus;
                            pushUIUpdate = true;
                        }
                        if (geoZone.Properties.MPERunPerformance.OpRunningLateTimer != mpe.OpRunningLateTimer)
                        {
                            geoZone.Properties.MPERunPerformance.OpRunningLateTimer = mpe.OpRunningLateTimer;
                            pushUIUpdate = true;
                        }
                        if (geoZone.Properties.MPERunPerformance.SortplanWrongStatus != mpe.SortplanWrongStatus)
                        {
                            geoZone.Properties.MPERunPerformance.SortplanWrongStatus = mpe.SortplanWrongStatus;
                            pushUIUpdate = true;
                        }
                        if (geoZone.Properties.MPERunPerformance.OpStartedLateStatus != mpe.OpStartedLateStatus)
                        {
                            geoZone.Properties.MPERunPerformance.OpStartedLateStatus = mpe.OpStartedLateStatus;
                            pushUIUpdate = true;
                        }
                        if (geoZone.Properties.MPERunPerformance.ThroughputStatus != mpe.ThroughputStatus)
                        {
                            geoZone.Properties.MPERunPerformance.ThroughputStatus = mpe.ThroughputStatus;
                            pushUIUpdate = true;
                        }
                        if (geoZone.Properties.MPERunPerformance.UnplanMaintSpTimer != mpe.UnplanMaintSpTimer)
                        {
                            geoZone.Properties.MPERunPerformance.UnplanMaintSpTimer = mpe.UnplanMaintSpTimer;
                            pushUIUpdate = true;
                        }
                        if (geoZone.Properties.MPERunPerformance.BinFullStatus != mpe.BinFullStatus)
                        {
                            geoZone.Properties.MPERunPerformance.BinFullStatus = mpe.BinFullStatus;
                            await Task.Run(() => BinFullProcess(mpe.MpeId, mpe.BinFullStatus, mpe.BinFullBins)).ConfigureAwait(false);
                            pushUIUpdate = true;
                        }
                        if (geoZone.Properties.MPERunPerformance.BinFullBins != mpe.BinFullBins)
                        {
                            geoZone.Properties.MPERunPerformance.BinFullBins = mpe.BinFullBins;

                            pushUIUpdate = true;
                        }
                        if (geoZone.Properties.MPERunPerformance.RpgEstimatedCompletion != RpgEstimatedCompletion)
                        {
                            geoZone.Properties.MPERunPerformance.RpgEstimatedCompletion = RpgEstimatedCompletion;

                            pushUIUpdate = true;
                        }
                        if (nextPlan != null && geoZone.Properties.MPERunPerformance.NextOperationId != NextOperationId.ToString())
                        {
                            geoZone.Properties.MPERunPerformance.NextOperationId = NextOperationId.ToString();

                            pushUIUpdate = true;
                        }
                        if (nextPlan != null && geoZone.Properties.MPERunPerformance.NextRPGStartDtm != NextRPGStartDtm)
                        {
                            geoZone.Properties.MPERunPerformance.NextRPGStartDtm = NextRPGStartDtm;

                            pushUIUpdate = true;
                        }
                    }
                    /// <summary>
                    /// Processes hourly data for each hour in the last 240 hours.
                    /// </summary>
                    /// <param name="hourslist">List of hours to process.</param>
                    /// <param name="result">The JToken result containing IDS data.</param>
                    /// <param name="mpeName">The name of the MPE to process.</param>
                    /// <param name="geoZone">The GeoZone object to update.</param>
                    /// <param name="pushUIUpdate">Flag indicating whether to push updates to the UI.</param>
                    List<DateTime> hourslist = await GetListOfHours(240);
                    if (hourslist != null)
                    {
                        foreach (DateTime hr in hourslist)
                        {
                            var mpeWatchData = mpe.HourlyData.FirstOrDefault(item => item.Hour == hr);
                            if (mpeWatchData != null)
                            {
                                var hourlyData = geoZone.Properties.MPERunPerformance.HourlyData.FirstOrDefault(h => h.Hour == hr);
                                if (hourlyData != null)
                                {
                                    bool countUpdated = mpeWatchData.Count > hourlyData.Count;
                                    if (countUpdated)
                                    {
                                        hourlyData.Count = mpeWatchData.Count;
                                        pushUIUpdate = true;
                                    }
                                }
                                else
                                {
                                    geoZone.Properties.MPERunPerformance.HourlyData.Add(new HourlyData
                                    {
                                        Hour = hr,
                                        Count = mpeWatchData.Count
                                    });
                                    pushUIUpdate = true;
                                }
                            }
                            else
                            {
                                var hourlyData = geoZone.Properties.MPERunPerformance.HourlyData.FirstOrDefault(h => h.Hour == hr);
                                if (hourlyData == null)
                                {
                                    geoZone.Properties.MPERunPerformance.HourlyData.Add(new HourlyData
                                    {
                                        Hour = hr
                                    });
                                }
                            }
                        }
                    }

                    //This line of code ensures that only the HourlyData records with hours present in the hourslist
                    //are retained in the HourlyData list of the MPERunPerformance property of a GeoZone object. All other records are removed.
                    //This is useful for maintaining a clean and relevant dataset by discarding outdated or unnecessary data.
                    geoZone.Properties.MPERunPerformance.HourlyData.RemoveAll(h => !hourslist.Contains(h.Hour));
                    if (pushUIUpdate)
                    {
                        await _hubServices.Clients.Group(geoZone.Properties.Type).SendAsync($"update{geoZone.Properties.Type}zoneRunPerformance", geoZone.Properties.MPERunPerformance);
                    }
                }
            }
            return await Task.FromResult(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return await Task.FromResult(false);
        }
        finally
        {
            _ = Task.Run(() => RunMPESummaryReport()).ConfigureAwait(false);
        }
    }

    private void BinFullProcess(string mpeId, string status, string fullBins)
    {
        List<string>? _fullBins = null;
        List<string> FullBinList = [];
        try
        {
            var geoZone = _geoZoneList.Where(r => r.Value.Properties.Type == "Bin" && r.Value.Properties.Name == mpeId).Select(y => y.Value).FirstOrDefault();
            if (geoZone != null)
            {
                switch (status)
                {
                    case "0":
                        _hubServices.Clients.Group(geoZone.Properties.Type).SendAsync($"update{geoZone.Properties.Type}zone", new JObject { ["zoneId"] = geoZone.Properties.Id, ["binFullStatus"] = status });
                        break;
                    case "1":
                        _fullBins = !string.IsNullOrEmpty(fullBins) ? fullBins.Split(',').Select(p => p.Trim().TrimStart('0')).ToList() : [];
                        for (int i = 0; i < _fullBins.Count; i++)
                        {
                            if (geoZone.Properties.Bins.Split(',').Select(p => p.Trim()).ToList().Contains(_fullBins[i]))
                            {
                                FullBinList.Add(_fullBins[i]);
                            }
                        }
                        if (FullBinList.Any())
                        {
                            _hubServices.Clients.Group(geoZone.Properties.Type).SendAsync($"update{geoZone.Properties.Type}zone", new JObject { ["zoneId"] = geoZone.Properties.Id, ["binFullStatus"] = status });
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        catch (Exception e)
        {

            _logger.LogError(e.Message);
        }
    }

    /// <summary>
    /// Processes IDS data and updates the in-memory repository accordingly.
    /// </summary>
    /// <param name="result">A JToken object containing the data to be processed.</param>
    /// <param name="stoppingToken">A CancellationToken to handle task cancellation.</param>
    /// <returns>A Task representing the asynchronous operation, with a boolean result indicating success or failure.</returns>
    /// <remarks>
    /// This method processes data from an IDS, updates the in-memory repository, and pushes updates to clients if necessary.
    /// It handles potential exceptions and supports task cancellation.
    /// </remarks>
    public async Task<bool> ProcessIDSData(JToken result, CancellationToken stoppingToken)
    {
        try
        {
            List<DateTime> hourslist = await GetListOfHours(240);
            // Check if result contains MPE_NAME
            if (result.Type == JTokenType.Array && result.Any(item => item["MPE_NAME"] != null))
            {
                List<string> mpeNames = result
                   .Where(item => item["MPE_NAME"] != null)
                   .Select(item => item["MPE_NAME"]?.ToString())
                   .Distinct()
                   .OrderBy(name => name)
                   .ToList();
                foreach (string mpeName in mpeNames)
                {
                    // Check if cancellationToken has been called
                    if (stoppingToken.IsCancellationRequested)
                    {
                        return false;
                    }
                    // If mpeName not in MPE list, add it
                    if (!_MPENameList.Contains(mpeName))
                    {
                        _MPENameList.Add(mpeName);
                    }

                    bool pushDBUpdate = false;
                    var geoZone = _geoZoneList.Where(r => r.Value.Properties.Type == "MPE" && r.Value.Properties.Name == mpeName).Select(y => y.Value).FirstOrDefault();
                    lock (_geoZoneList)
                    {
                        if (geoZone != null)
                        {
                            if (geoZone.Properties.MPERunPerformance == null)
                            {
                                geoZone.Properties.MPERunPerformance = new();
                            }
                            if (geoZone.Properties.MPERunPerformance.MpeId != mpeName)
                            {
                                geoZone.Properties.MPERunPerformance.MpeId = mpeName;
                            }
                            if (geoZone.Properties.MPERunPerformance.ZoneId != geoZone.Properties.Id)
                            {
                                geoZone.Properties.MPERunPerformance.ZoneId = geoZone.Properties.Id;
                            }
                            if (geoZone.Properties.MPERunPerformance.CurSortplan == "")
                            {
                                geoZone.Properties.MPERunPerformance.CurSortplan = "0";
                                geoZone.Properties.MPERunPerformance.CurrentRunStart = DateTime.MinValue.ToString();
                                geoZone.Properties.MPERunPerformance.CurrentRunEnd = DateTime.MinValue.ToString();
                                geoZone.Properties.MPERunPerformance.CurOperationId = "";
                            }
                            if (geoZone.Properties.DataSource != "IDS")
                            {
                                geoZone.Properties.DataSource = "IDS";
                                pushDBUpdate = true;
                            }
                            /// <summary>
                            /// Processes hourly data for each hour in the last 240 hours.
                            /// </summary>
                            /// <param name="hourslist">List of hours to process.</param>
                            /// <param name="result">The JToken result containing IDS data.</param>
                            /// <param name="mpeName">The name of the MPE to process.</param>
                            /// <param name="geoZone">The GeoZone object to update.</param>
                            /// <param name="pushDBUpdate">Flag indicating whether to push updates to the database.</param>

                            if (hourslist != null)
                            {
                                foreach (DateTime hr in hourslist)
                                {
                                    var mpeIDSData = result.FirstOrDefault(item => item["MPE_NAME"]?.ToString() == mpeName && item["HOUR"]?.ToString() == hr.ToString("yyyy-MM-dd HH:mm"));
                                    if (mpeIDSData != null)
                                    {
                                        var hourlyData = geoZone.Properties.MPERunPerformance.HourlyData.FirstOrDefault(h => h.Hour == hr);
                                        if (hourlyData != null)
                                        {
                                            bool sortedUpdated = (int)mpeIDSData["SORTED"] > hourlyData.Sorted;
                                            bool rejectedUpdated = (int)mpeIDSData["REJECTED"] > hourlyData.Rejected;
                                            bool countUpdated = (int)mpeIDSData["INDUCTED"] > hourlyData.Count;

                                            if (sortedUpdated || rejectedUpdated || countUpdated)
                                            {
                                                if (sortedUpdated)
                                                {
                                                    hourlyData.Sorted = (int)mpeIDSData["SORTED"];
                                                }

                                                if (rejectedUpdated)
                                                {
                                                    hourlyData.Rejected = (int)mpeIDSData["REJECTED"];
                                                }

                                                if (countUpdated)
                                                {
                                                    hourlyData.Count = (int)mpeIDSData["INDUCTED"];
                                                }

                                                pushDBUpdate = true;
                                            }
                                        }
                                        else
                                        {
                                            geoZone.Properties.MPERunPerformance.HourlyData.Add(new HourlyData
                                            {
                                                Hour = hr,
                                                Sorted = (int)mpeIDSData["SORTED"],
                                                Rejected = (int)mpeIDSData["REJECTED"],
                                                Count = (int)mpeIDSData["INDUCTED"]
                                            });
                                            pushDBUpdate = true;
                                        }
                                    }
                                    else
                                    {
                                        var hourlyData = geoZone.Properties.MPERunPerformance.HourlyData.FirstOrDefault(h => h.Hour == hr);
                                        if (hourlyData == null)
                                        {
                                            geoZone.Properties.MPERunPerformance.HourlyData.Add(new HourlyData
                                            {
                                                Hour = hr,
                                            });
                                        }
                                    }
                                }
                            }

                            //This line of code ensures that only the HourlyData records with hours present in the hourslist
                            //are retained in the HourlyData list of the MPERunPerformance property of a GeoZone object. All other records are removed.
                            //This is useful for maintaining a clean and relevant dataset by discarding outdated or unnecessary data.
                            geoZone.Properties.MPERunPerformance.HourlyData.RemoveAll(h => !hourslist.Contains(h.Hour));

                        }
                    }
                    if (pushDBUpdate)
                    {
                        await _hubServices.Clients.Group(geoZone.Properties.Type).SendAsync($"update{geoZone.Properties.Type}zoneRunPerformance", geoZone.Properties.MPERunPerformance);
                    }
                }
                return true;
            }
            else
            {
                // Handle the case where MPE_NAME is not present
                _logger.LogWarning("MPE_NAME not found in the result.");
                return false;
            }
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Error Processing data from Ids");
            return false;
        }
        finally
        {

            _ = Task.Run(() => RunMPESummaryReport()).ConfigureAwait(false);
        }
    }

    private async Task<List<DateTime>> GetListOfHours(int hours)
    {
        try
        {
            var currentTime = await _siteInfo.GetCurrentTimeInTimeZone(DateTime.Now);
            var localTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, (currentTime.Hour + 1), 0, 0);

            return Enumerable.Range(0, hours)
                             .Select(i => localTime.AddHours(-hours + i))
                             .OrderByDescending(dt => dt)
                             .ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Processing hour");
            return [DateTime.MinValue];
        }

    }

    public async void UpdateMPERunActivity(List<MPERunPerformance> mpeList)
    {
        bool SaveToFile = false;
        SiteInformation siteinfo = await _siteInfo.GetSiteInfo();
        try
        {
            foreach (var mpe in mpeList)
            {
                mpe.MpeId = string.Concat(mpe.MpeType, "-", mpe.MpeNumber.ToString().PadLeft(3, '0'));
                DateTime CurrentRunStart = (!string.IsNullOrEmpty(mpe.CurrentRunStart) && mpe.CurrentRunStart != "0")
                    ? DateTime.ParseExact(mpe.CurrentRunStart, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                    : DateTime.MinValue;

                DateTime CurrentRunEnd = (!string.IsNullOrEmpty(mpe.CurrentRunEnd) && mpe.CurrentRunEnd != "0")
                    ? DateTime.ParseExact(mpe.CurrentRunEnd, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                    : await _siteInfo.GetCurrentTimeInTimeZone(DateTime.Now);
                string mpe_id = string.Concat(mpe.MpeId, @"_", new DateTime(CurrentRunStart.Year, CurrentRunStart.Month, CurrentRunStart.Day, CurrentRunStart.Hour, CurrentRunStart.Minute, 0, 0).ToString(""));

                int.TryParse(mpe.MpeNumber, out int MpeNum);
                int.TryParse(mpe.CurOperationId, out int CurOperationId);
                int.TryParse(mpe.RpgEstVol, out int RpgEstVol);
                int.TryParse(mpe.RpgExpectedThruput, out int RpgExpectedThruput);
                int.TryParse(mpe.ActVolPlanVolNbr, out int ActVolPlanVolNbr);
                int.TryParse(mpe.CurThruputOphr, out int CurThruputOphr);
                int.TryParse(mpe.CurOperationId, out int OpNumber);
                int.TryParse(mpe.TotSortplanVol, out int TotSortplanVol);

                if (_MPERunActivity.ContainsKey(mpe_id) && _MPERunActivity.TryGetValue(mpe_id, out var activeRun))
                {
                    activeRun.ActiveRun = false;
                    //check if current run end is greater than the CurrentRunEnd

                    if (activeRun.CurThruputOphr != CurThruputOphr && CurThruputOphr > 0)
                    {
                        activeRun.CurThruputOphr = CurThruputOphr;
                        SaveToFile = true;
                    }
                    if (activeRun.TotSortplanVol != TotSortplanVol && TotSortplanVol > 0)
                    {
                        activeRun.TotSortplanVol = TotSortplanVol;
                        SaveToFile = true;
                    }
                    if (activeRun.RpgEstVol != RpgEstVol && RpgEstVol > 0)
                    {
                        activeRun.RpgEstVol = RpgEstVol;
                        SaveToFile = true;
                    }
                    if (activeRun.CurrentRunEnd != CurrentRunEnd)
                    {
                        activeRun.CurrentRunEnd = CurrentRunEnd;
                        activeRun.ActiveRun = true;
                        SaveToFile = true;
                    }
                }
                else
                {
                    _MPERunActivity.TryAdd(mpe_id, new MPEActiveRun
                    {
                        ActiveRun = true,
                        Type = "Run",
                        MpeType = mpe.MpeType,
                        MpeNumber = MpeNum,
                        MpeId = mpe.MpeId,
                        CurSortplan = mpe.CurSortplan,
                        CurThruputOphr = CurThruputOphr,
                        CurrentRunStart = CurrentRunStart,
                        CurrentRunEnd = CurrentRunEnd,
                        CurOperationId = CurOperationId,
                        TotSortplanVol = TotSortplanVol,
                        RpgEstVol = RpgEstVol,
                        RpgExpectedThruput = RpgExpectedThruput,
                        ActVolPlanVolNbr = ActVolPlanVolNbr
                    });

                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"Error loading MPE Run data {e.Message}");
        }
        finally
        {
            _MPERunActivity.Where(r => r.Value.CurrentRunStart <= DateTime.Now.AddDays(-7) && r.Value.Type == "Run").Select(l => l.Key).ToList().ForEach(key =>
            {
                if (_MPERunActivity.TryRemove(key, out MPEActiveRun remove))
                {
                    SaveToFile = true;
                }
            });
            if (SaveToFile)
            {
                await _fileService.WriteConfigurationFile("MPEActiveRun.json", JsonConvert.SerializeObject(_MPERunActivity.Values, Formatting.Indented));
            }
        }
    }
    public async Task<bool> LoadMPEPlan(JToken data, CancellationToken stoppingToken)
    {
        // sample data
        // {
        //  "mods_date": "2024-06-19 00:00:00",
        //  "machine_num": "151",
        //  "sort_program_name": "LV97216U-2",
        //  "rpg_start_dtm": "2024-06-19 22:45:00",
        //  "rpg_end_dtm": "2024-06-19 23:20:00",
        //  "rpg_pieces_fed": "14714",
        //  "mail_operation_nbr": "919000",
        //  "rpg_expected_thruput": "31533 pcs/hr",
        //  "mpew_start_15min_dtm": "2024-06-19 22:45:00",
        //  "mpew_end_15min_dtm": "2024-06-19 23:15:00",
        //  "mpe_type": "CIOSS",
        //  "mpe_name": "CIOSS-151"
        //}

        //loop through the sample data add to _MPERunActivity
        //if the data is already in the _MPERunActivity update the data
        //if the data is not in the _MPERunActivity add the data
        //if the data is in the _MPERunActivity and the rpg_end_dtm is greater than the current time remove the data
        bool SaveToFile = false;
        try
        {
            foreach (var item in data)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    return false;
                }
                //this to handle the unknown mpe_type for APPS
                //this is a temporary fix until the MPEWatch team fix the issue
                if (item["sort_program_name"].ToString().StartsWith("ATU"))
                {
                    int.TryParse(item["machine_num"].ToString(), out int mpeNumber);

                    item["mpe_type"] = "ATU";
                    item["mpe_name"] = string.Concat("ATU", "-", mpeNumber.ToString().PadLeft(3, '0'));
                    item["machine_num"] = mpeNumber;
                }
                if (item["sort_program_name"].ToString().StartsWith("USS") || item["sort_program_name"].ToString().StartsWith("M-USS"))
                {
                    int.TryParse(item["machine_num"].ToString(), out int mpeNumber);

                    item["mpe_type"] = "USS";
                    item["mpe_name"] = string.Concat("USS", "-", mpeNumber.ToString().PadLeft(3, '0'));
                    item["machine_num"] = mpeNumber;
                }
                if (item["sort_program_name"].ToString().StartsWith("HSTS"))
                {
                    int.TryParse(item["machine_num"].ToString(), out int mpeNumber);

                    item["mpe_type"] = "HSTS";
                    item["mpe_name"] = string.Concat("HSTS", "-", mpeNumber.ToString().PadLeft(3, '0'));
                    item["machine_num"] = mpeNumber;
                }
                if (item["mpe_type"].ToString().StartsWith("UNK"))
                {
                    int.TryParse(item["machine_num"].ToString(), out int mpeNumber);
                    item["mpe_type"] = "APPS";
                    item["mpe_name"] = string.Concat("APPS", "-", mpeNumber.ToString().PadLeft(3, '0'));
                }
                int? operationNumber = 0;
                _ = DateTime.TryParse(item["rpg_start_dtm"]?.ToString(), out DateTime rpg_start_dtm);
                _ = DateTime.TryParse(item["rpg_end_dtm"]?.ToString(), out DateTime rpg_end_dtm);
                _ = DateTime.TryParse(item["mpew_start_15min_dtm"]?.ToString(), out DateTime mpew_start_15min_dtm);
                _ = DateTime.TryParse(item["mpew_end_15min_dtm"]?.ToString(), out DateTime mpew_end_15min_dtm);
                _ = int.TryParse(item["rpg_pieces_fed"]?.ToString(), out int rpg_pieces_fed);
                _ = int.TryParse(item["rpg_expected_thruput"]?.ToString().Replace(" pcs/hr", ""), out int rpg_expected_thruput);
                // Extract the first 3 digits from mail_operation_nbr
                _ = int.TryParse(item["mail_operation_nbr"]?.ToString(), out int mail_operation_nbr);

                if (mail_operation_nbr != 0)
                {
                    operationNumber = int.Parse(mail_operation_nbr.ToString().Substring(0, 3));
                }
                _ = int.TryParse(item["machine_num"]?.ToString(), out int machine_num);
                string? mpe_name = item["mpe_name"]?.ToString();
                string? mpe_type = item["mpe_type"]?.ToString();
                string? sort_program_name = item["sort_program_name"]?.ToString();
                string? mods_date = item["mods_date"]?.ToString();
                string? mpe_id = string.Concat(mpe_name, @"_", new DateTime(rpg_start_dtm.Year, rpg_start_dtm.Month, rpg_start_dtm.Day, rpg_start_dtm.Hour, rpg_start_dtm.Minute, 0, 0).ToString(""));

                if (_MPERunActivity.ContainsKey(mpe_id) && _MPERunActivity.TryGetValue(mpe_id, out MPEActiveRun activeRun))
                {
                    if (activeRun.CurThruputOphr != rpg_expected_thruput && rpg_expected_thruput > 0)
                    {
                        activeRun.CurThruputOphr = rpg_expected_thruput;
                        SaveToFile = true;
                    }
                    if (activeRun.TotSortplanVol != rpg_pieces_fed && rpg_pieces_fed > 0)
                    {
                        activeRun.TotSortplanVol = rpg_pieces_fed;
                        SaveToFile = true;
                    }
                    if (activeRun.CurrentRunEnd != rpg_end_dtm)
                    {
                        activeRun.CurrentRunEnd = rpg_end_dtm;
                        SaveToFile = true;
                    }
                    if (activeRun.CurrentRunStart != rpg_start_dtm)
                    {
                        activeRun.CurrentRunStart = rpg_start_dtm;
                        SaveToFile = true;
                    }
                    //check sortplan
                    if (activeRun.CurSortplan != sort_program_name)
                    {
                        activeRun.CurSortplan = sort_program_name;
                        SaveToFile = true;
                    }

                }
                else
                {
                    _MPERunActivity.TryAdd(mpe_id, new MPEActiveRun
                    {
                        ActiveRun = true,
                        Type = "Plan",
                        MpeType = mpe_type,
                        MpeNumber = machine_num,
                        MpeId = mpe_name,
                        CurSortplan = sort_program_name,
                        CurThruputOphr = rpg_expected_thruput,
                        CurrentRunStart = rpg_start_dtm,
                        CurrentRunEnd = rpg_end_dtm,
                        CurOperationId = (int)operationNumber,
                        TotSortplanVol = rpg_pieces_fed,
                        RpgEstVol = rpg_pieces_fed,
                        RpgExpectedThruput = rpg_expected_thruput,
                        ActVolPlanVolNbr = rpg_pieces_fed
                    });
                    SaveToFile = true;
                }
            }
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError($"Error loading MPE Plan data {e.Message}");

            return false;
        }
        finally
        {
            _MPERunActivity.Where(r => r.Value.CurrentRunStart <= DateTime.Now.AddDays(-5) && r.Value.Type == "Plan").Select(l => l.Key).ToList().ForEach(key =>
            {
                if (_MPERunActivity.TryRemove(key, out MPEActiveRun remove))
                {
                    SaveToFile = true;
                }
            });
            if (SaveToFile)
            {
                await _fileService.WriteConfigurationFile("MPEActiveRun.json", JsonConvert.SerializeObject(_MPERunActivity.Values, Formatting.Indented));
            }
        }

    }

    public async Task LoadWebEORMPERun(JToken data)
    {
        bool SaveToFile = false;
        try
        {

            foreach (MPEActiveRun? item in data.ToObject<List<MPEActiveRun>>())
            {

                string mpe_id = string.Concat(item.MpeId, @"_", new DateTime(item.CurrentRunStart.Year, item.CurrentRunStart.Month, item.CurrentRunStart.Day, item.CurrentRunStart.Hour, item.CurrentRunStart.Minute, 0, 0).ToString(""));

                if (_MPERunActivity.ContainsKey(mpe_id) && _MPERunActivity.TryGetValue(mpe_id, out MPEActiveRun? activeRun))
                {
                    if (activeRun.CurThruputOphr != item.CurThruputOphr && item.CurThruputOphr > 0)
                    {
                        activeRun.CurThruputOphr = item.CurThruputOphr;
                        SaveToFile = true;
                    }
                    if (activeRun.TotSortplanVol != item.TotSortplanVol && item.TotSortplanVol > 0)
                    {
                        activeRun.TotSortplanVol = item.TotSortplanVol;
                        SaveToFile = true;
                    }
                    if (activeRun.CurrentRunEnd != item.CurrentRunEnd)
                    {
                        activeRun.CurrentRunEnd = item.CurrentRunEnd;
                        SaveToFile = true;
                    }
                    if (activeRun.CurrentRunStart != item.CurrentRunStart)
                    {
                        activeRun.CurrentRunStart = item.CurrentRunStart;
                        SaveToFile = true;
                    }
                    //check sortplan
                    if (activeRun.CurSortplan != item.CurSortplan)
                    {
                        activeRun.CurSortplan = item.CurSortplan;
                        SaveToFile = true;
                    }

                }
                else
                {
                    _MPERunActivity.TryAdd(mpe_id, new MPEActiveRun
                    {
                        ActiveRun = true,
                        Type = "Run",
                        MpeType = item.MpeType,
                        MpeNumber = item.MpeNumber,
                        MpeId = item.MpeId,
                        CurSortplan = item.CurSortplan,
                        CurThruputOphr = item.CurThruputOphr,
                        CurrentRunStart = item.CurrentRunStart,
                        CurrentRunEnd = item.CurrentRunEnd,
                        CurOperationId = item.CurOperationId,
                        TotSortplanVol = item.TotSortplanVol,

                    });
                    SaveToFile = true;
                }

            }
        }
        catch (Exception e)
        {
            _logger.LogError($"Error loading MPE Plan data {e.Message}");

        }
        finally
        {
            _MPERunActivity.Where(r => r.Value.CurrentRunStart <= DateTime.Now.AddDays(-5)).Select(l => l.Key).ToList().ForEach(key =>
            {
                if (_MPERunActivity.TryRemove(key, out MPEActiveRun? remove))
                {
                    SaveToFile = true;
                }
            });
            if (SaveToFile)
            {
                await _fileService.WriteConfigurationFile("MPEActiveRun.json", JsonConvert.SerializeObject(_MPERunActivity.Values, Formatting.Indented));
            }
        }
    }

    public async Task<object?> GetMPENameList()
    {
        return await Task.Run(() => _MPENameList.Select(y => y).ToList()).ConfigureAwait(false);
    }
    public async Task<object?> GetDockDoorNameList()
    {
        return await Task.Run(() => _DockDoorList.Select(y => y).ToList()).ConfigureAwait(false);
    }
    public async Task<object?> GetMPEGroupList(string type)
    {
        return await Task.Run(() => _geoZoneList.Where(r => r.Value.Properties.Type.StartsWith(type) && !string.IsNullOrEmpty(r.Value.Properties.MpeGroup)).Select(y => y.Value.Properties.MpeGroup).ToList()).ConfigureAwait(false);
    }

    //public List<TagTimeline> GetTagTimelineList(string ein)
    //{
    //    throw new NotImplementedException();
    //}
    private string GeoZoneDockDoorOutPutdata(List<GeoZoneDockDoor> DockDoor)
    {
        try
        {
            var jsonResolver = new PropertyRenameAndIgnoreSerializerContractResolver();
            jsonResolver.IgnoreProperty(typeof(RouteTrips), "RouteTrips");
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = jsonResolver;
            return JsonConvert.SerializeObject(DockDoor, Formatting.Indented, serializerSettings);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return "";
        }
    }
    private string GeoZoneOutPutdata(List<GeoZone> zone)
    {
        try
        {
            var jsonResolver = new PropertyRenameAndIgnoreSerializerContractResolver();
            jsonResolver.IgnoreProperty(typeof(Properties), "MPERunPerformance");
            jsonResolver.IgnoreProperty(typeof(Properties), "Bins");
            jsonResolver.IgnoreProperty(typeof(Properties), "Emails");
            jsonResolver.IgnoreProperty(typeof(Properties), "DataSource");
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = jsonResolver;
            return JsonConvert.SerializeObject(zone, Formatting.Indented, serializerSettings);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return "";
        }
    }
    /// <summary>
    ///  Get GeoZones by FloorId and type
    /// </summary>
    /// <param name="floorId"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public Task<object> GetGeoZonesTypeByFloorId(string floorId, string type)
    {
        try
        {

            // Convert GeoZone list to JArray
            JArray geoZones = JArray.FromObject(_geoZoneList.Values.Where(gz => gz.Properties.FloorId == floorId && gz.Properties.Type == type).ToList());
            // Convert GeoZoneKiosk list to JArray
            JArray geoZoneKiosk = JArray.FromObject(_geoZonekioskList.Values.Where(gz => gz.Properties.FloorId == floorId && gz.Properties.Type == type).ToList());
            // Convert GeoZoneDockdoor list to JArray
            JArray geoZoneDockdoor = JArray.FromObject(_geoZoneDockDoorList.Values.Where(gz => gz.Properties.FloorId == floorId && gz.Properties.Type == type).ToList());
            // Convert GeoZoneCude list to JArray
            JArray geoZoneCube = JArray.FromObject(_geoZoneCubeList.Values.Where(gz => gz.Properties.FloorId == floorId && gz.Properties.Type == type).ToList());
            // Merge the two geoZoneCube into a single JArray
            geoZones.Merge(geoZoneCube, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Concat,

            });
            // Merge the two geoZoneKiosk into a single JArray
            geoZones.Merge(geoZoneKiosk, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Concat,

            });
            // Merge the two geoZoneDockdoor into a single JArray
            geoZones.Merge(geoZoneDockdoor, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Concat,

            });
            return Task.FromResult((object)geoZones);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }

    #region
    /// <summary>
    /// dock door processing from SV Web
    /// </summary>
    /// <param name="result"></param>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    public async Task<bool> ProcessSVDoorsData(JToken result, CancellationToken stoppingToken)
    {
        bool saveDockDoorToFile = false;
        try
        {
            // Ensure result is a JArray
            if (result is JArray resultArray)
            {
                // Define the regex pattern to match the door number
                var regex = new Regex(@"\d+");
                // Convert to list and sort by doorNumber
                var sortedItems = resultArray
                    .OfType<JObject>()
                    .Where(item => item.ContainsKey("doorNumber"))
                    .OrderBy(item =>
                    {
                        var doorNumber = item["doorNumber"].ToString();
                        return Regex.IsMatch(doorNumber, @"^\d+$") ? int.Parse(doorNumber) : int.MaxValue;
                    })
                    .ThenBy(item => item["doorNumber"].ToString())
                    .ToList();
                foreach (JObject item in sortedItems)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        saveDockDoorToFile = false;
                        return false;
                    }
                    bool updateDockDoorUI = false;
                    string doorNumber = item["doorNumber"].ToString();
                    if (!_DockDoorList.Contains(doorNumber))
                    {
                        _DockDoorList.Add(doorNumber);
                    }
                    var DockDoorZone = _geoZoneDockDoorList.Where(r => r.Value.Properties.Type == "DockDoor" && r.Value.Properties.DoorNumber.PadLeft(3, '0') == doorNumber.PadLeft(3, '0')).Select(y => y.Value).FirstOrDefault();
                    if (DockDoorZone != null)
                    {
                        if (item.ContainsKey("routeTripId"))
                        {
                            updateDockDoorUI = await UpdateGeoZoneProperties(item, DockDoorZone);
                        }
                        else
                        {
                            ResetGeoZoneProperties(DockDoorZone);
                            updateDockDoorUI = true;
                        }
                    }
                    if (updateDockDoorUI)
                    {
                        await _hubServices.Clients.Group(DockDoorZone.Properties.Type)
                            .SendAsync($"update{DockDoorZone.Properties.Type}zone", DockDoorZone.Properties);
                    }
                }
                return true;
            }
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return false;
        }
        finally
        {
            if (saveDockDoorToFile)
            {
            }
        }


    }
    private async Task<bool> UpdateGeoZoneProperties(JObject item, GeoZoneDockDoor geoZone)
    {
        bool updateUI = false;

        // Use local variables to hold property values
        var tripDirectionInd = geoZone.Properties.TripDirectionInd;
        var routeTripId = geoZone.Properties.RouteTripId;
        var routeTripLegId = geoZone.Properties.RouteTripLegId;
        var route = geoZone.Properties.Route;
        var trip = geoZone.Properties.Trip;
        var legSiteName = geoZone.Properties.LegSiteName;
        var legSiteId = geoZone.Properties.LegSiteId;
        var status = geoZone.Properties.Status;
        var scheduledDtm = geoZone.Properties.ScheduledDtm;
        var tripMin = geoZone.Properties.TripMin;
        var isTripAtDoor = geoZone.Properties.IsTripAtDoor;

        // Update properties using local variables
        updateUI |= UpdateProperty(item, "tripDirectionInd", ref tripDirectionInd);
        bool routeTripIdUpdated = UpdateProperty(item, "routeTripId", ref routeTripId);
        bool routeTripLegIdUpdated = UpdateProperty(item, "routeTripLegId", ref routeTripLegId);
        updateUI |= routeTripIdUpdated;
        updateUI |= routeTripLegIdUpdated;
        updateUI |= UpdateProperty(item, "route", ref route);
        updateUI |= UpdateProperty(item, "trip", ref trip);
        updateUI |= UpdateProperty(item, "legSiteName", ref legSiteName);
        updateUI |= UpdateProperty(item, "legSiteId", ref legSiteId);
        updateUI |= UpdateProperty(item, "status", ref status, true);
        updateUI |= UpdateProperty(item, "scheduledDtm", ref scheduledDtm, true, GetSVDate);
        // Calculate tripMin from scheduledDtm
        var newTripMin = await GetTripMin(scheduledDtm);

        // Check if tripMin has changed
        if (tripMin != newTripMin)
        {
            tripMin = newTripMin;
            updateUI = true;
        }

        // Set IsTripAtDoor to true if routeTripId or routeTripLegId is updated
        if (routeTripIdUpdated || routeTripLegIdUpdated)
        {
            isTripAtDoor = true;
            updateUI = true;
        }
        // Assign updated values back to properties
        geoZone.Properties.TripDirectionInd = tripDirectionInd;
        geoZone.Properties.RouteTripId = routeTripId;
        geoZone.Properties.RouteTripLegId = routeTripLegId;
        geoZone.Properties.Route = route;
        geoZone.Properties.Trip = trip;
        geoZone.Properties.LegSiteName = legSiteName;
        geoZone.Properties.LegSiteId = legSiteId;
        geoZone.Properties.Status = status;
        geoZone.Properties.ScheduledDtm = scheduledDtm;
        geoZone.Properties.TripMin = tripMin;
        geoZone.Properties.IsTripAtDoor = isTripAtDoor;
        return updateUI;
    }

    private void ResetGeoZoneProperties(GeoZoneDockDoor geoZone)
    {
        geoZone.Properties.IsTripAtDoor = false;
        geoZone.Properties.TripDirectionInd = string.Empty;
        geoZone.Properties.RouteTripId = 0;
        geoZone.Properties.RouteTripLegId = 0;
        geoZone.Properties.Route = string.Empty;
        geoZone.Properties.Trip = string.Empty;
        geoZone.Properties.LegSiteName = string.Empty;
        geoZone.Properties.LegSiteId = string.Empty;
        geoZone.Properties.Status = string.Empty;
        geoZone.Properties.TripMin = 0;
        geoZone.Properties.ContainersNotLoaded = 0;
    }

    private bool UpdateProperty<T>(JObject item, string key, ref T property, bool isNullable = false, Func<JObject, T>? converter = null)
    {
        if (item.ContainsKey(key))
        {
            var newValue = converter != null ? converter((JObject)item[key]) : (T)Convert.ChangeType(item[key], typeof(T));
            if (!EqualityComparer<T>.Default.Equals(property, newValue))
            {
                property = newValue;
                return true;
            }
        }
        else if (isNullable)
        {
            property = default!;
            return true;
        }
        return false;
    }
    #endregion

    public Task ProcessSVContainerData(JToken result)
    {
        throw new NotImplementedException();
    }
    private DateTime GetSVDate(JObject scheduledDtm)
    {
        try
        {
            DateTime tripDtm = new(year: (int)scheduledDtm["year"],
                                                  month: (int)scheduledDtm["month"] + 1,
                                                  day: (int)scheduledDtm["dayOfMonth"],
                                                  hour: (int)scheduledDtm["hourOfDay"],
                                                  minute: (int)scheduledDtm["minute"],
                                                  second: (int)scheduledDtm["second"], kind: DateTimeKind.Local);
            return tripDtm;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return DateTime.MinValue;
        }

    }
    private async Task<int> GetTripMin(DateTime tripDtm)
    {
        try
        {
            return (int)Math.Ceiling(tripDtm.Subtract(await GetDTMNow()).TotalMinutes);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return 0;
        }

    }
    private async Task<DateTime> GetDTMNow()
    {
        try
        {
            return await _siteInfo.GetCurrentTimeInTimeZone(DateTime.Now);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return DateTime.Now;
        }

    }
    /// <summary>
    /// Get GeoZones by type and name
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public Task<object> GetGeoZonebyName(string type, string name)
    {
        try
        {
            var regexPattern = string.Concat("(", name, ")");
            // Convert GeoZone list to JArray
            JArray geoZones = JArray.FromObject(_geoZoneList.Values.Where(gz => gz.Properties.Type == type && gz.Properties.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).ToList());
            // Convert GeoZoneKiosk list to JArray
            JArray geoZoneKiosk = JArray.FromObject(_geoZonekioskList.Values.Where(gz => gz.Properties.Type == type && gz.Properties.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).ToList());
            // Convert GeoZoneDockdoor list to JArray
            JArray geoZoneDockdoor = JArray.FromObject(_geoZoneDockDoorList.Values.Where(gz => gz.Properties.Type == type && gz.Properties.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).ToList());
            // Convert GeoZoneCude list to JArray
            JArray geoZoneCube = JArray.FromObject(_geoZoneCubeList.Values.Where(gz => gz.Properties.Type == type && gz.Properties.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).ToList());
            // Merge the two geoZoneCube into a single JArray
            geoZones.Merge(geoZoneCube, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Concat,

            });
            // Merge the two geoZoneKiosk into a single JArray
            geoZones.Merge(geoZoneKiosk, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Concat,

            });
            // Merge the two geoZoneDockdoor into a single JArray
            geoZones.Merge(geoZoneDockdoor, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Concat,

            });
            return Task.FromResult((object)geoZones);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Task.FromResult<object>(new object());
        }
    }
    /// <summary>
    /// Get GeoZones by type
    /// </summary>
    /// <param name="zoneType"></param>
    /// <returns></returns>
    public Task<object> GetGeoZonebyType(string zoneType)
    {
        try
        {
            // Convert GeoZone list to JArray
            JArray geoZones = JArray.FromObject(_geoZoneList.Values.Where(gz => gz.Properties.Type.Equals(zoneType, StringComparison.CurrentCultureIgnoreCase)).ToList());
            // Convert GeoZoneKiosk list to JArray
            JArray geoZoneKiosk = JArray.FromObject(_geoZonekioskList.Values.Where(gz => gz.Properties.Type.Equals(zoneType, StringComparison.CurrentCultureIgnoreCase)).ToList());
            // Convert GeoZoneDockDoor list to JArray
            JArray geoZoneDockdoor = JArray.FromObject(_geoZoneDockDoorList.Values.Where(gz => gz.Properties.Type.Equals(zoneType, StringComparison.CurrentCultureIgnoreCase)).ToList());
            // Convert GeoZoneCube list to JArray
            JArray geoZoneCube = JArray.FromObject(_geoZoneCubeList.Values.Where(gz => gz.Properties.Type.Equals(zoneType, StringComparison.CurrentCultureIgnoreCase)).ToList());
            // Merge the two geoZoneCube into a single JArray
            geoZones.Merge(geoZoneCube, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Concat
            });
            // Merge the two geoZoneKiosk into a single JArray
            geoZones.Merge(geoZoneKiosk, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Concat
            });
            // Merge the two geoZoneDockdoor into a single JArray
            geoZones.Merge(geoZoneDockdoor, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Concat
            });
            return Task.FromResult((object)geoZones);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Task.FromResult<object>(new object());
        }
    }

    public Task<bool> ResetGeoZoneList()
    {
        try
        {
            _geoZoneList.Clear();
            _geoZoneCubeList.Clear();
            _geoZoneDockDoorList.Clear();
            _geoZonekioskList.Clear();
            _mpeSummary.Clear();
            _QREAreaDwellResults.Clear();
            _MPENameList.Clear();
            _DockDoorList.Clear();
            _MPERunActivity.Clear();
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Task.FromResult(true);
        }
    }

    public Task<bool> SetupGeoZoneData()
    {
        try
        {
            // Load data from the first file into the first collection
            LoadDataFromFile().Wait();
            // Load data from the first file into the first collection
            LoadDockDoorDataFromFile().Wait();
            // Load data from the first file into the first collection
            LoadKioskDataFromFile().Wait();
            // Load data from the first file into the first collection
            LoadCubesDataFromFile().Wait();
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Task.FromResult(true);
        }
    }

    public async Task<bool> ProcessQPEGeoZone(List<CoordinateSystem> coordinateSystems, CancellationToken stoppingToken)
    {
        bool saveToFile = false;
        bool docdoorsaveToFile = false;
        try
        {
            if (coordinateSystems.Count > 0)
            {
                foreach (var coordinateSystem in coordinateSystems)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        saveToFile = false;
                        return false;
                    }
                    if (coordinateSystem.zones.Count > 0)
                    {
                        foreach (var zone in coordinateSystem.zones)
                        {
                            if (stoppingToken.IsCancellationRequested)
                            {
                                saveToFile = false;
                                return false;
                            }
                            var TempGeometry = QuuppaZoneGeometry(zone.polygonData);
                            var geoZone = _geoZoneList.Where(r => r.Value.Properties.Id == zone.id).Select(y => y.Value).FirstOrDefault();
                            var geoZoneDockDoor = _geoZoneDockDoorList.Where(r => r.Value.Properties.Id == zone.id).Select(y => y.Value).FirstOrDefault();

                            if (geoZone != null)
                            {
                                if (geoZone.Geometry.Coordinates.ToString() != TempGeometry.Coordinates.ToString())
                                {
                                    geoZone.Geometry.Coordinates = TempGeometry.Coordinates;
                                    await _hubServices.Clients.Group(geoZone.Properties.Type).SendAsync($"update{geoZone.Properties.Type}zoneShape", geoZone.Geometry.Coordinates);
                                    saveToFile = true;
                                }
                                if (geoZone.Properties.Name != geoZone.Properties.Name)
                                {
                                    geoZone.Properties.Name = zone.name;
                                    await _hubServices.Clients.Group(geoZone.Properties.Type).SendAsync($"update{geoZone.Properties.Type}zoneShape", geoZone.Properties);
                                    saveToFile = true;
                                }
                            }
                            else if (geoZoneDockDoor != null)
                            {
                                if (geoZoneDockDoor.Geometry.Coordinates.ToString() != TempGeometry.Coordinates.ToString())
                                {
                                    geoZoneDockDoor.Geometry.Coordinates = TempGeometry.Coordinates;
                                    await _hubServices.Clients.Group(geoZoneDockDoor.Properties.Type).SendAsync($"update{geoZoneDockDoor.Properties.Type}zoneShape", geoZoneDockDoor.Geometry.Coordinates);
                                    docdoorsaveToFile = true;
                                }
                                if (geoZoneDockDoor.Properties.Name != geoZoneDockDoor.Properties.Name)
                                {
                                    geoZoneDockDoor.Properties.Name = zone.name;
                                    await _hubServices.Clients.Group(geoZoneDockDoor.Properties.Type).SendAsync($"update{geoZoneDockDoor.Properties.Type}zoneShape", geoZoneDockDoor.Properties);
                                    docdoorsaveToFile = true;
                                }
                            }
                            else
                            {
                                string zoneType = GetZoneType(zone.name);
                                if (zoneType.Equals("MPE", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    if (!_MPENameList.Contains(zone.name))
                                    {
                                        _MPENameList.Add(zone.name);
                                    }
                                    string[] mpeSplit = zone.name.Split('-');
                                    if (_geoZoneList.TryAdd(zone.id, new GeoZone
                                    {
                                        Geometry = TempGeometry,
                                        Properties = new Properties
                                        {
                                            Id = zone.id,
                                            Name = zone.name,
                                            MpeName = mpeSplit.Length > 1 ? mpeSplit[0] : mpeSplit[0],
                                            MpeNumber = mpeSplit.Length > 1 ? mpeSplit[1] : "0",
                                            Type = zoneType,
                                            Visible = true,
                                            FloorId = coordinateSystem.id
                                        }

                                    }))
                                    {
                                        saveToFile = true;
                                    }
                                }
                                else if (zoneType.Equals("DockDoor", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    if (!_DockDoorList.Contains(ExtractNumbers(zone.name)))
                                    {
                                        _DockDoorList.Add(ExtractNumbers(zone.name));
                                    }
                                    if (_geoZoneDockDoorList.TryAdd(zone.id, new GeoZoneDockDoor
                                    {
                                        Geometry = TempGeometry,
                                        Properties = new DockDoorProperties
                                        {
                                            Id = zone.id,
                                            Name = zone.name,
                                            DoorNumber = ExtractNumbers(zone.name),
                                            Type = zoneType,
                                            Visible = true,
                                            FloorId = coordinateSystem.id
                                        }
                                    }))
                                    {
                                        docdoorsaveToFile = true;
                                    }
                                }
                                else
                                {
                                    if (_geoZoneList.TryAdd(zone.id, new GeoZone
                                    {
                                        Geometry = TempGeometry,
                                        Properties = new Properties
                                        {
                                            Id = zone.id,
                                            Name = zone.name,
                                            Type = zoneType,
                                            Visible = zone.visible,
                                            FloorId = coordinateSystem.id
                                        }

                                    }))
                                    {
                                        saveToFile = true;
                                    }
                                }
                            }
                        }
                    }


                }
            }
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return false;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileName, GeoZoneOutPutdata(_geoZoneList.Select(x => x.Value).ToList()));
            }
            if (docdoorsaveToFile)
            {
                await _fileService.WriteConfigurationFile(fileNameDockDoor, GeoZoneDockDoorOutPutdata(_geoZoneDockDoorList.Select(x => x.Value).ToList()));
            }
        }
    }
    private Geometry QuuppaZoneGeometry(string polygonData)
    {
        try
        {
            JObject geometry = new JObject();
            JArray temp = new JArray();

            string[] polygonDatasplit = polygonData.Split('|');
            if (polygonDatasplit.Length > 0)
            {
                JArray xyar = new JArray();
                foreach (var polygonitem in polygonDatasplit)
                {
                    string[] polygonitemsplit = polygonitem.Split(',');
                    xyar.Add(new JArray(Convert.ToDouble(polygonitemsplit[0]), Convert.ToDouble(polygonitemsplit[1])));
                }
                temp.Add(xyar);
            }

            geometry["coordinates"] = temp;

            return geometry.ToObject<Geometry>();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }

    }

    // ...

    public string ExtractNumbers(string input)
    {
        string pattern = @"\d+";
        MatchCollection matches = Regex.Matches(input, pattern);
        string result = string.Join("", matches.Select(m => m.Value));
        return result;
    }

    public Task<MPERunPerformance> GetGeoZoneMPEPerformanceData(string zoneName)
    {
        try
        {
            return Task.FromResult(_geoZoneList.Where(r => r.Value.Properties.Type == "MPE" && r.Value.Properties.Name == zoneName).Select(y => y.Value.Properties.MPERunPerformance).ToList().FirstOrDefault());
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }
    #region //Mpe Targets
    public async Task<List<TargetHourlyData>> GetAllMPETragets()
    {
        try
        {
            return _MPETargets.Select(y => y.Value).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }

    public async Task<List<TargetHourlyData>>? GetMPETargets(string mpeId)
    {
        try
        {
            return _MPETargets.Where(r => r.Value.MpeId == mpeId).Select(y => y.Value).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;

        }
    }

    public async Task<List<TargetHourlyData>> AddMPETargets(JToken mpeData)
    {
        bool saveToFile = false;
        try
        {
            TargetHourlyData targetHourlyDatas = mpeData.ToObject<TargetHourlyData>();
            if (targetHourlyDatas != null)
            {
                string mpeIdandHour = $"{targetHourlyDatas.MpeName}-{targetHourlyDatas.MpeNumber.ToString().PadLeft(3, '0')}{targetHourlyDatas.TargetHour}";
                targetHourlyDatas.Id = mpeIdandHour;
                if (!_MPETargets.ContainsKey(mpeIdandHour))
                {
                    if (_MPETargets.TryAdd(mpeIdandHour, targetHourlyDatas))
                    {
                        saveToFile = true;
                        return await GetMPETargets(targetHourlyDatas.MpeId);
                    }

                }
                else if (_MPETargets.TryGetValue(mpeIdandHour, out TargetHourlyData current) && _MPETargets.TryUpdate(mpeIdandHour, targetHourlyDatas, current))
                {
                    saveToFile = true;
                    return await GetMPETargets(targetHourlyDatas.MpeId);
                }

                return null;
            }
            else
            {
                return null;
            }

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileNameMpeTarget, JsonConvert.SerializeObject(_MPETargets.Select(x => x.Value).ToList(), Formatting.Indented));
            }
        }
    }

    public async Task<List<TargetHourlyData>> UpdateMPETargets(JToken mpeData)
    {
        bool saveToFile = false;
        try
        {
            TargetHourlyData targetHourlyDatas = mpeData.ToObject<TargetHourlyData>();
            if (targetHourlyDatas != null)
            {
                string mpeIdandHour = $"{targetHourlyDatas.MpeName}-{targetHourlyDatas.MpeNumber.ToString().PadLeft(3, '0')}{targetHourlyDatas.TargetHour}";
                if (string.IsNullOrEmpty(targetHourlyDatas.Id))
                {
                    targetHourlyDatas.Id = mpeIdandHour;
                }
                if (_MPETargets.ContainsKey(mpeIdandHour) && _MPETargets.TryGetValue(mpeIdandHour, out TargetHourlyData current) && _MPETargets.TryUpdate(mpeIdandHour, targetHourlyDatas, current))
                {
                    saveToFile = true;
                    await _hubServices.Clients.Group("MPETartgets").SendAsync($"updateMPEzoneTartgets", await GetMPETargets(targetHourlyDatas.MpeId));
                    return await GetMPETargets(targetHourlyDatas.MpeId);
                }
                else
                {
                    if (_MPETargets.TryAdd(mpeIdandHour, targetHourlyDatas))
                    {
                        saveToFile = true;
                        await _hubServices.Clients.Group("MPETartgets").SendAsync($"updateMPEzoneTartgets", await GetMPETargets(targetHourlyDatas.MpeId));
                        return await GetMPETargets(targetHourlyDatas.MpeId);
                    }
                }
                return null;
            }
            else
            {
                return null;
            }

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileNameMpeTarget, JsonConvert.SerializeObject(_MPETargets.Select(x => x.Value).ToList(), Formatting.Indented));
            }
        }
    }
    public async Task<bool> RemoveMPETargetsTour(JToken mpeData)
    {
        bool saveToFile = false;
        try
        {
            List<string> deleteHourlyDatas = mpeData.ToObject<List<string>>();
            if (deleteHourlyDatas.Any())
            {
                foreach (var deleteitem in deleteHourlyDatas)
                {
                    if (_MPETargets.ContainsKey(deleteitem) && _MPETargets.TryRemove(deleteitem, out TargetHourlyData deleted))
                    {
                        saveToFile = true;
                    }
                }
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
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileNameMpeTarget, JsonConvert.SerializeObject(_MPETargets.Select(x => x.Value).ToList(), Formatting.Indented));
            }
        }
    }

    public async Task<TargetHourlyData> RemoveMPETargets(JToken mpeData)
    {
        bool saveToFile = false;
        try
        {
            List<TargetHourlyData> targetHourlyDatas = mpeData.ToObject<List<TargetHourlyData>>();
            if (targetHourlyDatas.Any())
            {
                foreach (var item in targetHourlyDatas)
                {
                    string mpeIdandHour = $"{item.MpeName}-{item.MpeNumber.ToString().PadLeft(3, '0')}{item.TargetHour}";
                    if (_MPETargets.ContainsKey(mpeIdandHour) && _MPETargets.TryRemove(mpeIdandHour, out TargetHourlyData deleted))
                    {
                        saveToFile = true;
                        return deleted;
                    }
                }
                return null;
            }
            else
            {
                return null;
            }

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileNameMpeTarget, JsonConvert.SerializeObject(_MPETargets.Select(x => x.Value).ToList(), Formatting.Indented));
            }
        }
    }

    public async Task<bool> LoadCSVMpeTargets(List<TargetHourlyData> targetHourly)
    {
        bool saveToFile = false;
        try
        {
            if (targetHourly != null)
            {
                foreach (var targetHourlyDatas in targetHourly)
                {
                    if (_MPETargets.ContainsKey(targetHourlyDatas.Id) && _MPETargets.TryGetValue(targetHourlyDatas.Id, out TargetHourlyData current) && _MPETargets.TryUpdate(targetHourlyDatas.Id, targetHourlyDatas, current))
                    {
                        saveToFile = true;
                    }
                    else
                    {
                        if (_MPETargets.TryAdd(targetHourlyDatas.Id, targetHourlyDatas))
                        {
                            saveToFile = true;
                        }
                    }
                }

                return saveToFile;

            }
            else
            {
                return saveToFile;
            }

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return false;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileNameMpeTarget, JsonConvert.SerializeObject(_MPETargets.Select(x => x.Value).ToList(), Formatting.Indented));
            }
        }
    }
    #endregion

    #region Kiosk
    public async Task<object> GetAllKiosk()
    {
        try
        {
            return _geoZonekioskList.Select(y => y.Value).ToList().Select(r => r.Properties);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return false;
        }
    }
    public async Task<GeoZoneKiosk> AddKiosk(GeoZoneKiosk newgeoZone)
    {
        bool saveToFile = false;
        try
        {

            if (_geoZonekioskList.TryAdd(newgeoZone.Properties.Id, newgeoZone))
            {
                saveToFile = true;
                return newgeoZone;
            }
            else
            {
                _logger.LogError($"Kiosk Zone File {fileNameKiosk} list was not saved...");
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileNameKiosk, JsonConvert.SerializeObject(_geoZonekioskList.Values, Formatting.Indented));
            }
        }
    }
    public async Task<GeoZoneKiosk> UpdateKiosk(KioskProperties updategeoZone)
    {
        bool saveToFile = false;
        try
        {

            if (_geoZonekioskList.ContainsKey(updategeoZone.Id) && _geoZonekioskList.TryGetValue(updategeoZone.Id, out GeoZoneKiosk currrentgeoZone))
            {
                if (currrentgeoZone.Properties.Name != updategeoZone.Name)
                {
                    currrentgeoZone.Properties.Name = updategeoZone.Name;
                }
                if (currrentgeoZone.Properties.Number != updategeoZone.Number)
                {
                    currrentgeoZone.Properties.Number = updategeoZone.Number;
                }
                if (currrentgeoZone.Properties.DeviceId != updategeoZone.DeviceId)
                {
                    currrentgeoZone.Properties.DeviceId = updategeoZone.DeviceId;
                }
                saveToFile = true;
                return currrentgeoZone;
            }
            else
            {
                _logger.LogError($"Kiosk Zone File {fileNameKiosk} list was not saved...");
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileNameKiosk, JsonConvert.SerializeObject(_geoZonekioskList.Values, Formatting.Indented));
            }
        }
    }

    public async Task<GeoZoneKiosk?> RemoveKiosk(string geoZoneId)
    {
        bool saveToFile = false;
        try
        {
            if (_geoZonekioskList.TryRemove(geoZoneId, out GeoZoneKiosk geoZone))
            {
                saveToFile = true;
                return await Task.FromResult(geoZone);
            }
            else
            {
                _logger.LogError($"Dock door Zone File {fileNameDockDoor} list was not saved...");
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileNameKiosk, JsonConvert.SerializeObject(_geoZonekioskList.Values, Formatting.Indented));
            }
        }
    }
    private async Task LoadKioskDataFromFile()
    {
        try
        {
            // Read data from file
            var fileContent = await _fileService.ReadFile(fileNameKiosk);

            // Parse the file content to get the data. This depends on the format of your file.
            // Here's an example if your file was in JSON format and contained an array of T objects:
            List<GeoZoneKiosk>? data = JsonConvert.DeserializeObject<List<GeoZoneKiosk>>(fileContent);

            // Insert the data into the MongoDB collection
            if (data?.Count > 0)
            {
                foreach (GeoZoneKiosk item in data.Select(r => r).ToList())
                {
                    _geoZonekioskList.TryAdd(item.Properties.Id, item);
                }

            }
        }
        catch (FileNotFoundException ex)
        {
            // Handle the FileNotFoundException here
            _logger.LogError($"File not found: {ex.FileName}");
            // You can choose to throw an exception or take any other appropriate action
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            _logger.LogError($"An error occurred when reading the file: {ex.Message}");
        }
        catch (JsonException ex)
        {
            // Handle errors when parsing the JSON
            _logger.LogError($"An error occurred when parsing the JSON: {ex.Message}");
        }
    }

    public async Task<GeoZoneKiosk?> GetKiosk(string id)
    {
        try
        {
            return await Task.Run(() => _geoZonekioskList.Where(r => r.Value.Properties.KioskId == id).Select(y => y.Value).FirstOrDefault());
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }
    public async Task<KioskDetails> CheckKioskZone(string id)
    {
        try
        {
            var kioskZone = await Task.Run(() => _geoZonekioskList
             .Where(zone => zone.Value.Properties.DeviceId == id)
             .Select(zone => zone.Value)
             .FirstOrDefault());

            if (kioskZone != null)
            {
                return new KioskDetails
                {
                    IsFound = true,
                    KioskName = kioskZone.Properties.Name,
                    KioskNumber = kioskZone.Properties.Number,
                    KioskId = kioskZone.Properties.KioskId
                };
            }
            else
            {
                return new KioskDetails
                {
                    IsFound = false,
                };
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return new KioskDetails
            {
                IsFound = false,
            };
        }
    }
    #endregion
    #region Cude
    /// <summary>
    /// Update the cube list with the scan data.
    /// </summary>
    /// <param name="scan"></param>
    public void updateEpacsScanInCube(ScanInfo scan)
    {
        try
        {
            // find assigned employeed and update the inZone flag to true.
            string? zoneid = _geoZoneCubeList.Where(r => r.Value.Properties.EIN == scan.Data.Transactions[0].CardholderData.EIN).Select(y => y.Key).FirstOrDefault();

            if (zoneid != null)
            {
                if (_geoZoneCubeList.ContainsKey(zoneid) && _geoZoneCubeList.TryGetValue(zoneid, out GeoZoneCube geoZoneCube))
                {
                    geoZoneCube.Properties.ScanTime = scan.Data.Transactions[0].TransactionDateTime;
                    string deviceId = scan.Data.Transactions[0].DeviceID.ToString();

                    List<string> inBuildDevices = _geoZonekioskList.Where(r => r.Value.Properties.Name.Equals("ePACS-In", StringComparison.CurrentCultureIgnoreCase)).Select(r => r.Value.Properties.DeviceId).ToList();

                    List<string> outBuildDevices = _geoZonekioskList.Where(r => r.Value.Properties.Name.Equals("ePACS-Out", StringComparison.CurrentCultureIgnoreCase)).Select(r => r.Value.Properties.DeviceId).ToList();

                    if (inBuildDevices.Contains(deviceId))
                    {
                        geoZoneCube.Properties.InZone = true;
                    }
                    if (outBuildDevices.Contains(deviceId))
                    {
                        geoZoneCube.Properties.InZone = false;
                    }
                    // count the number of inZone employees.
                    _ = Task.Run(() => UpdateInZoneCount());
                }
            }
            else
            {
                bool updateCount = false;
                // Update any _geoZoneCubeList InZone flag to false if zone scantime is greater than 8 hours
                DateTime currentTime = DateTime.Now;
                foreach (var cube in _geoZoneCubeList.Values)
                {
                    TimeSpan timeDifference = currentTime - cube.Properties.ScanTime;
                    if (cube.Properties.InZone && timeDifference.TotalHours > 8)
                    {

                        cube.Properties.InZone = false;
                        updateCount = true;
                    }
                }
                if (updateCount)
                {
                    // count the number of inZone employees.
                    _ = Task.Run(() => UpdateInZoneCount());
                }

            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
    }
    private async Task UpdateInZoneCount()
    {
        try
        {
            int inZoneCount = _geoZoneCubeList.Values.Where(r => r.Properties.InZone == true).Count();
            await _hubServices.Clients.Group("OSL").SendAsync($"updateInZoneCount", new { inZoneCount = inZoneCount });
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
    }

    public async Task<object> GetAllCube()
    {
        try
        {
            return _geoZoneCubeList.Select(y => y.Value).ToList().Select(r => r.Properties);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return false;
        }
    }

    public async Task<GeoZoneCube?> AddCube(GeoZoneCube geoZoneCube)
    {
        bool saveToFile = false;
        try
        {
            if (_geoZoneCubeList.TryAdd(geoZoneCube.Properties.Id, geoZoneCube))
            {
                saveToFile = true;
                return await Task.FromResult(geoZoneCube);
            }
            else
            {
                _logger.LogError("Cube Zone File {FileName} list was not saved...", fileName);
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileNameCube, JsonConvert.SerializeObject(_geoZoneCubeList.Values, Formatting.Indented));
            }
        }
    }

    public async Task<GeoZoneCube?> RemoveCube(string geoZoneId)
    {
        bool saveToFile = false;
        try
        {
            if (_geoZoneCubeList.TryRemove(geoZoneId, out GeoZoneCube geoZoneCube))
            {
                saveToFile = true;
                return await Task.FromResult(geoZoneCube);
            }
            else
            {
                _logger.LogError("Cube Zone File {FileName} list was not saved...", fileName);
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileNameCube, JsonConvert.SerializeObject(_geoZoneCubeList.Values, Formatting.Indented));
            }
        }
    }

    public async Task<GeoZoneCube?> UpdateCube(CubeProperties newCubeProperties)
    {
        bool saveToFile = false;
        try
        {
            if (_geoZoneCubeList.ContainsKey(newCubeProperties.Id) && _geoZoneCubeList.TryGetValue(newCubeProperties.Id, out GeoZoneCube? currentGeoZoneCube))
            {
                if (currentGeoZoneCube.Properties.Name != newCubeProperties.Name)
                {
                    currentGeoZoneCube.Properties.Name = newCubeProperties.Name;
                    saveToFile = true;
                }
                if (currentGeoZoneCube.Properties.EIN != newCubeProperties.EIN)
                {
                    currentGeoZoneCube.Properties.EIN = newCubeProperties.EIN;
                    saveToFile = true;
                }
                if (currentGeoZoneCube.Properties.Number != newCubeProperties.Number)
                {
                    currentGeoZoneCube.Properties.Number = newCubeProperties.Number;
                    saveToFile = true;
                }
                if (currentGeoZoneCube.Properties.AssignTo != newCubeProperties.AssignTo)
                {
                    currentGeoZoneCube.Properties.AssignTo = newCubeProperties.AssignTo;
                    saveToFile = true;
                }

                return currentGeoZoneCube;
            }
            else
            {
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileNameCube, JsonConvert.SerializeObject(_geoZoneCubeList.Values, Formatting.Indented));
            }
        }
    }

    public async Task<GeoZoneCube?> GetCube(string id)
    {
        try
        {
            var geoZoneCube = _geoZoneCubeList.Where(r => r.Value.Properties.Id == id)
                                              .Select(y => y.Value)
                                              .FirstOrDefault();
            return await Task.FromResult(geoZoneCube);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return null;
        }
    }
    private async Task LoadCubesDataFromFile()
    {
        try
        {
            // Read data from file
            var fileContent = await _fileService.ReadFile(fileNameCube);

            // Parse the file content to get the data. This depends on the format of your file.
            // Here's an example if your file was in JSON format and contained an array of T objects:
            List<GeoZoneCube>? data = JsonConvert.DeserializeObject<List<GeoZoneCube>>(fileContent);

            // Insert the data into the MongoDB collection
            if (data?.Count > 0)
            {
                foreach (GeoZoneCube item in data.Select(r => r).ToList())
                {
                    _geoZoneCubeList.TryAdd(item.Properties.Id, item);
                }

            }
        }
        catch (FileNotFoundException ex)
        {
            // Handle the FileNotFoundException here
            _logger.LogError($"File not found: {ex.FileName}");
            // You can choose to throw an exception or take any other appropriate action
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            _logger.LogError($"An error occurred when reading the file: {ex.Message}");
        }
        catch (JsonException ex)
        {
            // Handle errors when parsing the JSON
            _logger.LogError($"An error occurred when parsing the JSON: {ex.Message}");
        }
    }
    #endregion
}