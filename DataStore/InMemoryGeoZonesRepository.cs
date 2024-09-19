using EIR_9209_2.DataStore;
using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using static EIR_9209_2.DataStore.InMemoryCamerasRepository;

public class InMemoryGeoZonesRepository : IInMemoryGeoZonesRepository
{
    private readonly ConcurrentDictionary<string, GeoZone> _geoZoneList = new();
    private readonly ConcurrentDictionary<string, GeoZoneDockDoor> _geoZoneDockDoorList = new();
    private readonly ConcurrentDictionary<string, Dictionary<DateTime, MPESummary>> _mpeSummary = new();
    private readonly ConcurrentDictionary<DateTime, List<AreaDwell>> _QREAreaDwellResults = new();
    private readonly ConcurrentDictionary<string, MPEActiveRun> _MPERunActivity = new();
    private readonly ConcurrentDictionary<string, MPEActiveRun> _MPEStandard = new();
    private readonly List<string> _MPENameList = new();
    private readonly List<string> _DockDoorList = new();
    private readonly IInMemorySiteInfoRepository _siteInfo;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InMemoryGeoZonesRepository> _logger;
    private readonly IFileService _fileService;
    protected readonly IHubContext<HubServices> _hubServices;
    private readonly string fileName = "Zone.json";
    private readonly string fileNameDockDoor = "ZonesDockDoor.json";
    public InMemoryGeoZonesRepository(ILogger<InMemoryGeoZonesRepository> logger, IConfiguration configuration, IFileService fileService, IHubContext<HubServices> hubServices,IInMemorySiteInfoRepository siteInfo)
    {
        _fileService = fileService;
        _logger = logger;
        _configuration = configuration;
        _hubServices = hubServices;
        _siteInfo = siteInfo;
        // Load data from the first file into the first collection
        LoadDataFromFile().Wait();
        // Load data from the first file into the first collection
        LoadDockDoorDataFromFile().Wait();

    }
    public async Task<GeoZone>? Add(GeoZone geoZone)
    {
        bool saveToFile = false;
        try
        {
            if (!Regex.IsMatch(geoZone.Properties.Type, "^(MPE)", RegexOptions.IgnoreCase))
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
                _logger.LogError($"Zone File list was not saved...");
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
                await _fileService.WriteFileAsync(fileName, GeoZoneOutPutdata(_geoZoneList.Select(x => x.Value).ToList()));
            }
        }
    }

    public async Task<GeoZone>? Remove(string geoZoneId)
    {
        bool saveToFile = false;
        try
        {
            if (_geoZoneList.TryRemove(geoZoneId, out GeoZone geoZone))
            {
                saveToFile = true;
                return await Task.FromResult(geoZone);
            }
            else
            {
                _logger.LogError($"Zone File list was not saved...");
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
                await _fileService.WriteFileAsync(fileName, GeoZoneOutPutdata(_geoZoneList.Select(x => x.Value).ToList()));
            }
        }
    }

    public async Task<GeoZoneDockDoor>? AddDockDoor(GeoZoneDockDoor newgeoZone)
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
                    if (Int32.TryParse(dockNumber,out int doornumber))
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
                _logger.LogError($"Zone File list was not saved...");
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
                await _fileService.WriteFileAsync(fileNameDockDoor, GeoZoneDockDoorOutPutdata(_geoZoneDockDoorList.Select(x => x.Value).ToList()));
            }
        }
    }

    public async Task<GeoZoneDockDoor>? RemoveDockDoor(string geoZoneId)
    {
        bool saveToFile = false;
        try
        {
            if (_geoZoneDockDoorList.TryRemove(geoZoneId, out GeoZoneDockDoor geoZone))
            {
                saveToFile = true;
                return await Task.FromResult(geoZone);
            }
            else
            {
                _logger.LogError($"Zone File list was not saved...");
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
                await _fileService.WriteFileAsync(fileNameDockDoor, GeoZoneDockDoorOutPutdata(_geoZoneDockDoorList.Select(x => x.Value).ToList()));
            }
        }
    }
    public async Task<JObject> Update(JObject properties)
    {
        bool saveToFile = false;
        try
        {
            if (_geoZoneList.TryGetValue(properties["id"].ToString(), out GeoZone Properties))
            {
                //check if properties then update the properties type
                if (properties.ContainsKey("type"))
                {
                    Properties.Properties.Type = properties["type"].ToString();
                    saveToFile = true;
                }


                return await Task.FromResult(properties);
            }
            else
            {
                _logger.LogError($"Zone File list was not saved...");
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
                await _fileService.WriteFileAsync(fileName, GeoZoneOutPutdata(_geoZoneList.Select(x => x.Value).ToList()));
            }
        }
    }
    public Task<GeoZoneDockDoor> UpdateDockDoor(GeoZoneDockDoor geoZone)
    {
        throw new NotImplementedException();
    }
    public async Task<GeoZone>? UiUpdate(GeoZone geoZone)
    {
        bool saveToFile = false;
        try
        {
            if (_geoZoneList.TryGetValue(geoZone.Properties.Id, out GeoZone? currentConnection) && _geoZoneList.TryUpdate(geoZone.Properties.Id, geoZone, currentConnection))
            {
                saveToFile = true;
                if (_geoZoneList.TryGetValue(geoZone.Properties.Id, out GeoZone? zone))
                {

                    return await Task.FromResult(zone);
                }
                else
                {
                    return null;
                }
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
                await _fileService.WriteFileAsync(fileName, GeoZoneOutPutdata(_geoZoneList.Select(x => x.Value).ToList()));
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
            _logger.LogError(e.Message);
            return null;
        }
    }
    public IEnumerable<GeoZone> GetAll() => _geoZoneList.Values;
    public IEnumerable<GeoZoneDockDoor>? GetDockDoor()
    {
        return _geoZoneDockDoorList.Values;
    }
    public object getMPESummary(string area)
    {
        return _mpeSummary.Where(r => Regex.IsMatch(r.Key, "(" + area + ")", RegexOptions.IgnoreCase)).Select(r => r.Value).ToList();

    }
    public Task<List<MPESummary>> getMPESummaryDateRange(string area, DateTime startDT, DateTime endDT)
    {
        var ty = startDT.Date;
        //i want to select all the area that matches the area and is between startDT and endDT
        var result = _mpeSummary.Where(r => Regex.IsMatch(r.Key, "(" + area + ")", RegexOptions.IgnoreCase)).SelectMany(y => y.Value)
            .Where(u => u.Key >= startDT && u.Key <= endDT).Select(y => y.Value).ToList();
        return Task.FromResult(result);
    }
    public List<MPEActiveRun> getMPERunActivity(string area)
    {
        return _MPERunActivity.Where(r => Regex.IsMatch(r.Key, "(" + area + ")", RegexOptions.IgnoreCase)).Select(r => r.Value).ToList();

    }

    public bool ExistingAreaDwell(DateTime hour)
    {
        return _QREAreaDwellResults.ContainsKey(hour);

    }
    public List<AreaDwell> GetAreaDwell(DateTime hour)
    {
        return _QREAreaDwellResults[hour];
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
            if (_QREAreaDwellResults.TryAdd(hour, newValue)) {
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
    public void RunMPESummaryReport()
    {
        try
        {
            List<string> areasList = _geoZoneList.Where(r => r.Value.Properties.Type == "MPE").Select(item => item.Value.Properties.Name).Distinct().ToList();
            if (areasList.Any())
            {
                foreach (var area in areasList)
                {
                    var mpe = _geoZoneList.Where(r => r.Value.Properties.Name == area && r.Value.Properties.Type == "MPE").Select(y => y.Value.Properties).FirstOrDefault();
                    if (mpe != null)
                    {
                        List<DateTime> hoursInMpeDateTime = mpe.MPERunPerformance.HourlyData.Select(x => DateTime.Parse(x.Hour, CultureInfo.CurrentCulture, DateTimeStyles.None)).ToList();
                        if (!_mpeSummary.ContainsKey(area))
                        {
                            _mpeSummary.TryAdd(area, new Dictionary<DateTime, MPESummary>());
                        }
                        foreach (var hour in hoursInMpeDateTime)
                        {
                            var hourlySummaryForHourAndArea = GetHourlySummaryForHourAndArea(area, hour, mpe.MPERunPerformance);
                            lock (_mpeSummary[area])
                            {
                                _mpeSummary[area][hour] = hourlySummaryForHourAndArea;
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
    }

    private MPESummary? GetHourlySummaryForHourAndArea(string area, DateTime Dateandhour, MPERunPerformance mpe)
    {
        try
        {
            string hourFormat = Dateandhour.ToString("yyyy-MM-ddTHH:mm:ss");
            string hour = Dateandhour.ToString("yyyy-MM-dd HH:mm");
            var standardPiecseFeed = 0;
            var standardStaffHrs = 0;
            //MPEActiveRun AR = AppParameters.MPEStandard.Values
            //       .Where(x => x.MpeId == mpe.MpeId && x.Hourlydata.Any(y => y.Hour == hour))
            //       .Select(x => x)
            //       .FirstOrDefault();
            //if (AR != null)
            //{
            //    standardPiecseFeed = AR.Hourlydata.Where(r => r.Hour == hour).FirstOrDefault()?.Count ?? 0;
            //    standardStaffHrs = AR.Hourlydata.Where(r => r.Hour == hour).FirstOrDefault()?.StaffCount ?? 0;
            //}
            var piecesCountThisHour = mpe.HourlyData.Where(r => r.Hour == hour).FirstOrDefault()?.Count;
            var piecesSortedThisHour = mpe.HourlyData.Where(r => r.Hour == hour).FirstOrDefault()?.Sorted;
            var piecesRejectedThisHour = mpe.HourlyData.Where(r => r.Hour == hour).FirstOrDefault()?.Rejected;
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
            var entriesThisArea = _QREAreaDwellResults.ContainsKey(Dateandhour) ? _QREAreaDwellResults[Dateandhour].Where(r => r.AreaName.Equals(area)) : null; //_QREAreaDwellResults(Dateandhour);
            if (entriesThisArea != null)
            {
                //if where pieces Sorted is available the calculate the actual yield using the sorted pieces

                var clerkAndMailHandlerCountThisHour = entriesThisArea.Where(e => Regex.IsMatch(e.Type, "^(clerk|mail handler)", RegexOptions.IgnoreCase)).Sum(g => g.DwellTimeDurationInArea.TotalMilliseconds) / (1000 * 60 * 60);
                actualYieldcal = piecesForYield != null && clerkAndMailHandlerCountThisHour > 0 ? piecesForYield.Value / clerkAndMailHandlerCountThisHour : 0.0;
                if (mpe.HourlyData.Where(r => r.Hour == hour).Select(y => y.Count).Any())
                {
                    actualYieldcal = mpe.HourlyData.Where(r => r.Hour == hour).Select(y => y.Count).First() / (entriesThisArea.Where(e => Regex.IsMatch(e.Type, "^(clerk|mail handler)", RegexOptions.IgnoreCase)).Sum(g => g.DwellTimeDurationInArea.TotalMilliseconds) / (1000 * 60 * 60));
                }
                laborHrs = entriesThisArea.GroupBy(e => e.Type).ToDictionary(g => g.Key, g => g.Sum(e => e.DwellTimeDurationInArea.TotalMilliseconds));
                laborCounts = entriesThisArea.GroupBy(e => e.Type).ToDictionary(g => g.Key, g => g.Count());
                clerkDwellTime = entriesThisArea.Where(e => Regex.IsMatch(e.Type, "^(clerk)", RegexOptions.IgnoreCase)).Sum(g => g.DwellTimeDurationInArea.TotalMilliseconds);
                maintDwellTime = entriesThisArea.Where(e => Regex.IsMatch(e.Type, "^(maintenance)", RegexOptions.IgnoreCase)).Sum(g => g.DwellTimeDurationInArea.TotalMilliseconds);
                mhDwellTime = entriesThisArea.Where(e => Regex.IsMatch(e.Type, "^(mail handler)", RegexOptions.IgnoreCase)).Sum(g => g.DwellTimeDurationInArea.TotalMilliseconds);
                supervisorDwellTime = entriesThisArea.Where(e => Regex.IsMatch(e.Type, "^(supervisor)", RegexOptions.IgnoreCase)).Sum(g => g.DwellTimeDurationInArea.TotalMilliseconds);
                otherDwellTime = entriesThisArea.Where(e => !Regex.IsMatch(e.Type, "^(clerk|supervisor|mail handler|maintenance)", RegexOptions.IgnoreCase)).Sum(g => g.DwellTimeDurationInArea.TotalMilliseconds);
                clerkPresent = entriesThisArea.Where(e => Regex.IsMatch(e.Type, "^(clerk)", RegexOptions.IgnoreCase)).Count();
                mhPresent = entriesThisArea.Where(e => Regex.IsMatch(e.Type, "^(mail handler)", RegexOptions.IgnoreCase)).Count();
                maintPresent = entriesThisArea.Where(e => Regex.IsMatch(e.Type, "^(maintenance)", RegexOptions.IgnoreCase)).Count();
                supervisorPresent = entriesThisArea.Where(e => Regex.IsMatch(e.Type, "^(supervisor)", RegexOptions.IgnoreCase)).Count();
                otherPresent = entriesThisArea.Where(e => !Regex.IsMatch(e.Type, "^(clerk|supervisor|mail handler|maintenance)", RegexOptions.IgnoreCase)).Count();
            }
            return new MPESummary
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
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
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

                        item.Properties.Type = GetZoneType(item.Properties.Name);

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
                  

                    if (string.IsNullOrEmpty(item.Properties.DoorNumber))
                    {
                        string dockNumber = item.Properties.Name.Replace("DockDoor", "");
                        //check if dock number is number 
                        if (Int32.TryParse(dockNumber, out int doornumber))
                        {
                            item.Properties.DoorNumber = doornumber.ToString();
                        }
                        else
                        {
                            item.Properties.DoorNumber = dockNumber;
                        }
                    }

                    //check if door exits in _DockDoorList
                    if (!_DockDoorList.Contains(item.Properties.DoorNumber))
                    {
                        _DockDoorList.Add(item.Properties.DoorNumber);
                    }
                    _geoZoneDockDoorList.TryAdd(item.Properties.Id, item);
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
        try
        {
            if (!string.IsNullOrEmpty(name))
            {
                if (Regex.IsMatch(name, _configuration[key: "ZoneConfiguration:AGVLocation"], RegexOptions.IgnoreCase))
                {
                    return "AGVLocation";
                }
                else if (Regex.IsMatch(name, _configuration[key: "ZoneConfiguration:Dockdoor"], RegexOptions.IgnoreCase))
                {
                    return "DockDoor";
                }
                else if (Regex.IsMatch(name, _configuration[key: "ZoneConfiguration:Area"], RegexOptions.IgnoreCase))
                {
                    return "Area";
                }
                else if (Regex.IsMatch(name, _configuration[key: "ZoneConfiguration:Viewport"], RegexOptions.IgnoreCase))
                {
                    return "ViewPorts";
                }
                else
                {
                    return "MPE";
                }
            }
            else
            {
                return "None";
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return "None";
        }
    }
    public async Task<List<string>> GetZoneNameList(string type)
    {
        try
        {
            // Filter and select from _geoZoneList
            var geoZoneNames = _geoZoneList
                .Where(r => r.Value.Properties.Type.Normalize() == type.Normalize())
                .Select(y => y.Value.Properties.Name)
                .ToList();

            // Filter and select from _geoZoneDockDoorList
            var dockDoorNames = _geoZoneDockDoorList
                .Where(r => r.Value.Properties.Type.Normalize() == type.Normalize())
                .Select(y => y.Value.Properties.Name)
                .ToList();

            // Combine the results and ensure uniqueness
            var allMPEZones = geoZoneNames.Concat(dockDoorNames).Distinct().ToList();

            return await Task.FromResult(allMPEZones);
        }
        catch (Exception e)
        { 
            // Handle the exception
            Console.WriteLine($"An error occurred: {e.Message}");
            _logger.LogError(e.Message);
            return null;
        }


       
    }

    public async Task<bool> UpdateMPERunInfo(List<MPERunPerformance> mpeList)
    {
        try
        {
            await Task.Run(() => UpdateMPERunActivity(mpeList)).ConfigureAwait(false);
            foreach (MPERunPerformance mpe in mpeList)
            {
                mpe.MpeId = string.Concat(mpe.MpeType, "-", mpe.MpeNumber.ToString().PadLeft(3, '0'));
                //if mpename not in MPE list add it
                if (!_MPENameList.Contains(mpe.MpeId))
                {
                    _MPENameList.Add(mpe.MpeId);
                }
                
                var geoZone = _geoZoneList.Where(r => r.Value.Properties.Type == "MPE" && r.Value.Properties.Name == mpe.MpeId).Select(y => y.Value).FirstOrDefault();
                if (geoZone != null)
                {
                    bool pushUIUpdate = false;

                    if (string.IsNullOrEmpty(geoZone.Properties.MPERunPerformance?.MpeType))
                    {
                        geoZone.Properties.MPERunPerformance = mpe;
                        geoZone.Properties.MPERunPerformance.MpeId = mpe.MpeId;
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
                    }
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

    public async Task ProcessIDSData(JToken result)
    {
        try
        {
            List<string> mpeNames = result.Select(item => item["MPE_NAME"]?.ToString()).Distinct().OrderBy(name => name).ToList();
            foreach (string mpeName in mpeNames)
            {
                //if mpename not in MPE list add it
                if (!_MPENameList.Contains(mpeName))
                {
                    _MPENameList.Add(mpeName);
                }

                bool pushDBUpdate = false;
                var geoZone = _geoZoneList.Where(r => r.Value.Properties.Type == "MPE" && r.Value.Properties.Name == mpeName).Select(y => y.Value).FirstOrDefault();
                lock (_geoZoneList)
                {


                    if (geoZone != null && geoZone.Properties.MPERunPerformance != null)
                    {
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
                        List<string> hourslist = GetListOfHours(336);
                        foreach (string hr in hourslist)
                        {
                            var mpeData = result.Where(item => item["MPE_NAME"]?.ToString() == mpeName && item["HOUR"]?.ToString() == hr).FirstOrDefault();
                            if (mpeData != null)
                            {
                                if (geoZone.Properties.MPERunPerformance.HourlyData.Where(h => h.Hour == hr).Any())
                                {
                                    geoZone.Properties.MPERunPerformance.HourlyData.Where(h => h.Hour == hr).ToList().ForEach(h =>
                                    {
                                        if (h.Sorted != (int)mpeData["SORTED"])
                                        {
                                            h.Sorted = (int)mpeData["SORTED"];
                                            pushDBUpdate = true;
                                        }

                                        if (h.Rejected != (int)mpeData["REJECTED"])
                                        {
                                            h.Rejected = (int)mpeData["REJECTED"];
                                            pushDBUpdate = true;
                                        }
                                        if (h.Count != (int)mpeData["INDUCTED"])
                                        {
                                            h.Count = (int)mpeData["INDUCTED"];
                                            pushDBUpdate = true;
                                        }

                                    });
                                }
                                else
                                {
                                    geoZone.Properties.MPERunPerformance.HourlyData.Add(new HourlyData
                                    {
                                        Hour = hr,
                                        Sorted = (int)mpeData["SORTED"],
                                        Rejected = (int)mpeData["REJECTED"],
                                        Count = (int)mpeData["INDUCTED"]
                                    });
                                    pushDBUpdate = true;
                                }
                            }
                            else
                            {
                                if (geoZone.Properties.MPERunPerformance.HourlyData.Where(h => h.Hour == hr).Any())
                                {
                                    geoZone.Properties.MPERunPerformance.HourlyData.Where(h => h.Hour == hr).ToList().ForEach(h =>
                                    {
                                        h.Sorted = 0;
                                        h.Rejected = 0;
                                        h.Count = 0;
                                    });
                                }
                                else
                                {
                                    geoZone.Properties.MPERunPerformance.HourlyData.Add(new HourlyData
                                    {
                                        Hour = hr,
                                        Sorted = 0,
                                        Rejected = 0,
                                        Count = 0
                                    });
                                }
                            }
                        }

                    }
                }
                if (pushDBUpdate)
                {
                    await _hubServices.Clients.Group("MPEZones").SendAsync($"update{geoZone.Properties.Type}zoneRunPerformance", geoZone.Properties.MPERunPerformance);
                }
            }
            _ = Task.Run(() => RunMPESummaryReport()).ConfigureAwait(false); 
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Error Processing data from");
        }
    }

    private List<string> GetListOfHours(int hours)
    {
        var localTime = _siteInfo.GetCurrentTimeInTimeZone(DateTime.Now);
        return Enumerable.Range(0, hours).Select(i => localTime.AddHours(-hours).AddHours(i).ToString("yyyy-MM-dd HH:00")).ToList();
    }

    public async void UpdateMPERunActivity(List<MPERunPerformance> mpeList)
    {
        bool SaveToFile = false;
        SiteInformation siteinfo = await _siteInfo.GetSiteInfo();
        try
        {
            foreach (var mpe in mpeList)
            {
                DateTime CurrentRunStart = !string.IsNullOrEmpty(mpe.CurrentRunStart)
                          ? DateTime.ParseExact(mpe.CurrentRunStart, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                          : DateTime.MinValue;
                DateTime CurrentRunEnd = !string.IsNullOrEmpty(mpe.CurrentRunEnd)
                 ? DateTime.ParseExact(mpe.CurrentRunEnd, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                 : _siteInfo.GetCurrentTimeInTimeZone(DateTime.Now);
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
                await _fileService.WriteFileAsync("MPEActiveRun.json", JsonConvert.SerializeObject(_MPERunActivity.Values, Formatting.Indented));
            }
        }
    }

    public async Task LoadMPEPlan(JToken data)
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
                //this to handle the unknown mpe_type for APPS
                //this is a temporary fix until the MPEWatch team fix the issue
                if (item["sort_program_name"].ToString().StartsWith("ATU"))
                {
                    int.TryParse(item["machine_num"].ToString().Substring(1), out int mpeNumber);

                    item["mpe_type"] = "ATU";
                    item["mpe_name"] = string.Concat("ATU", "-", mpeNumber.ToString().PadLeft(3, '0'));
                    item["machine_num"] = mpeNumber;
                }
                if (item["sort_program_name"].ToString().StartsWith("USS") || item["sort_program_name"].ToString().StartsWith("M-USS"))
                {
                    int.TryParse(item["machine_num"].ToString().Substring(1), out int mpeNumber);

                    item["mpe_type"] = "USS";
                    item["mpe_name"] = string.Concat("USS", "-", mpeNumber.ToString().PadLeft(3, '0'));
                    item["machine_num"] = mpeNumber;
                }
                if (item["sort_program_name"].ToString().StartsWith("HSTS"))
                {
                    int.TryParse(item["machine_num"].ToString().Substring(1), out int mpeNumber);

                    item["mpe_type"] = "HSTS";
                    item["mpe_name"] = string.Concat("HSTS", "-", mpeNumber.ToString().PadLeft(3, '0'));
                    item["machine_num"] = mpeNumber;
                }
                if (item["mpe_type"].ToString().StartsWith("UNK"))
                {
                    int.TryParse(item["machine_num"].ToString().Substring(1), out int mpeNumber);
                    item["mpe_type"] = "APPS";
                    item["mpe_name"] = string.Concat("APPS", "-", mpeNumber.ToString().PadLeft(3, '0'));
                }
                int operationNumber = 0;
                DateTime.TryParse(item["rpg_start_dtm"]?.ToString(), out DateTime rpg_start_dtm);
                DateTime.TryParse(item["rpg_end_dtm"]?.ToString(), out DateTime rpg_end_dtm);
                DateTime.TryParse(item["mpew_start_15min_dtm"]?.ToString(), out DateTime mpew_start_15min_dtm);
                DateTime.TryParse(item["mpew_end_15min_dtm"]?.ToString(), out DateTime mpew_end_15min_dtm);
                int.TryParse(item["rpg_pieces_fed"]?.ToString(), out int rpg_pieces_fed);
                int.TryParse(item["rpg_expected_thruput"]?.ToString().Replace(" pcs/hr", ""), out int rpg_expected_thruput);
                // Extract the first 3 digits from mail_operation_nbr
                int.TryParse(item["mail_operation_nbr"]?.ToString(), out int mail_operation_nbr);
                if (mail_operation_nbr != 0)
                {
                    operationNumber = int.Parse(mail_operation_nbr.ToString().Substring(0, 3));
                }
                int.TryParse(item["machine_num"]?.ToString(), out int machine_num);
                string mpe_name = item["mpe_name"]?.ToString();
                string mpe_type = item["mpe_type"]?.ToString();
                string sort_program_name = item["sort_program_name"]?.ToString();
                string mods_date = item["mods_date"]?.ToString();
                string mpe_id = string.Concat(mpe_name, @"_", new DateTime(rpg_start_dtm.Year, rpg_start_dtm.Month, rpg_start_dtm.Day, rpg_start_dtm.Hour, rpg_start_dtm.Minute, 0, 0).ToString(""));

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
                        CurOperationId = operationNumber,
                        TotSortplanVol = rpg_pieces_fed,
                        RpgEstVol = rpg_pieces_fed,
                        RpgExpectedThruput = rpg_expected_thruput,
                        ActVolPlanVolNbr = rpg_pieces_fed
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
            _MPERunActivity.Where(r => r.Value.CurrentRunStart <= DateTime.Now.AddDays(-5) && r.Value.Type == "Plan").Select(l => l.Key).ToList().ForEach(key =>
            {
                if (_MPERunActivity.TryRemove(key, out MPEActiveRun remove))
                {
                    SaveToFile = true;
                }
            });
            if (SaveToFile)
            {
                await _fileService.WriteFileAsync("MPEActiveRun.json", JsonConvert.SerializeObject(_MPERunActivity.Values, Formatting.Indented));
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
                await _fileService.WriteFileAsync("MPEActiveRun.json", JsonConvert.SerializeObject(_MPERunActivity.Values, Formatting.Indented));
            }
        }
    }

    public async Task<object> GetMPENameList()
    {
        return await Task.Run(() => _MPENameList.Select(y => y).ToList()).ConfigureAwait(false);
    }
    public async Task<object> GetDockDoorNameList()
    {
        return await Task.Run(() => _DockDoorList.Select(y => y).ToList()).ConfigureAwait(false);
    }
    public async Task<object> GetMPEGroupList(string type)
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

    public async Task ProcessSVDoorsData(JToken result)
    {
        bool SaveToFile = false;
        try
        {   // Ensure result is a JArray
            if (result is JArray resultArray)
            {
                // Convert to list and sort by doorNumber
                var sortedItems = resultArray
                    .OfType<JObject>()
                    .Where(item => item.ContainsKey("doorNumber"))
                    .OrderBy(item => item["doorNumber"].ToString())
                    .ToList();
                foreach (JObject item in sortedItems)
                {
                 
                    bool updateUI = false;
                    GeoZoneDockDoor geoZone = new GeoZoneDockDoor();
                    if (item.ContainsKey("doorNumber"))
                    {
                        string doorNumber = item["doorNumber"].ToString();
                        //check if door exits in _DockDoorList
                        if (!_DockDoorList.Contains(doorNumber))
                        {
                            _DockDoorList.Add(doorNumber);
                        }
                        //find door number in _geoZoneDockDoorList
                        geoZone = _geoZoneDockDoorList.Where(r => r.Value.Properties.DoorNumber == doorNumber).Select(y => y.Value).FirstOrDefault();
                        if (geoZone != null)
                        {
                            if (item.ContainsKey("routeTripId"))
                            {
                                if (!geoZone.Properties.IsTripAtDoor)
                                {
                                    geoZone.Properties.IsTripAtDoor = true;
                                    updateUI = true;
                                }
                                //if TripDirectionInd is not equal to item["tripDirectionInd"].ToString()
                                if (geoZone.Properties.TripDirectionInd != item["tripDirectionInd"].ToString())
                                {
                                    geoZone.Properties.TripDirectionInd = item["tripDirectionInd"].ToString();
                                    updateUI = true;
                                }
                                if (geoZone.Properties.RouteTripId != (int)item["routeTripId"])
                                {
                                    geoZone.Properties.RouteTripId = (int)item["routeTripId"];
                                    updateUI = true;
                                }
                                if (geoZone.Properties.RouteTripLegId != (int)item["routeTripLegId"])
                                {
                                    geoZone.Properties.RouteTripLegId = (int)item["routeTripLegId"];
                                    updateUI = true;
                                }
                                if (geoZone.Properties.Route != item["route"].ToString())
                                {
                                    geoZone.Properties.Route = item["route"].ToString();
                                    updateUI = true;
                                }
                                if (geoZone.Properties.Trip != item["trip"].ToString())
                                {
                                    geoZone.Properties.Trip = item["trip"].ToString();
                                    updateUI = true;
                                }
                                if (geoZone.Properties.LegSiteName != item["legSiteName"].ToString())
                                {
                                    geoZone.Properties.LegSiteName = item["legSiteName"].ToString();
                                    updateUI = true;
                                }
                                if (geoZone.Properties.LegSiteId != item["legSiteId"].ToString())
                                {
                                    geoZone.Properties.LegSiteId = item["legSiteId"].ToString();
                                    updateUI = true;
                                }
                                if (item.ContainsKey("status") && geoZone.Properties.Status != item["status"].ToString())
                                {
                                    geoZone.Properties.Status = item["status"].ToString();
                                    updateUI = true;
                                }
                                else
                                {
                                    geoZone.Properties.Status = "";
                                    updateUI = true;
                                }
                                if (geoZone.Properties.ScheduledDtm != GetSVDate((JObject)item["scheduledDtm"]))
                                {
                                    geoZone.Properties.ScheduledDtm = GetSVDate((JObject)item["scheduledDtm"]);
                                    updateUI = true;
                                }
                                int tripMin = GetTripMin(GetSVDate((JObject)item["scheduledDtm"]));
                                if (geoZone.Properties.TripMin != tripMin)
                                {
                                    geoZone.Properties.TripMin = tripMin;
                                    updateUI = true;
                                }

                                geoZone.Properties.ContainersNotLoaded = 0;
                            }
                            else
                            {
                                if (geoZone.Properties.IsTripAtDoor)
                                {
                                    geoZone.Properties.IsTripAtDoor = false;
                                    updateUI = true;
                                }
                                geoZone.Properties.TripDirectionInd = "";
                                geoZone.Properties.RouteTripId = 0;
                                geoZone.Properties.RouteTripLegId = 0;
                                geoZone.Properties.Route = "";
                                geoZone.Properties.Trip = "";
                                geoZone.Properties.LegSiteName = "";
                                geoZone.Properties.LegSiteId = "";
                                geoZone.Properties.Status = "";
                                geoZone.Properties.TripMin = 0;
                                geoZone.Properties.ContainersNotLoaded = 0;


                            }
                            if (updateUI)
                            {
                                await _hubServices.Clients.Group(geoZone.Properties.Type).SendAsync($"update{geoZone.Properties.Type}zone", geoZone.Properties);
                            }
                        }
                        
                    }
                
                }

            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
        finally
        {
            if (SaveToFile)
            {
               // await _fileService.WriteFileAsync("MPEActiveRun.json", JsonConvert.SerializeObject(_MPERunActivity.Values, Formatting.Indented));
            }
        }
    }
    public async Task ProcessSVContainerData(JToken result)
    {
        throw new NotImplementedException();
    }
    private DateTime GetSVDate(JObject scheduledDtm)
    {
        try
        {
            DateTime tripDtm = new DateTime((int)scheduledDtm["year"],
                                                  (int)scheduledDtm["month"] + 1,
                                                  (int)scheduledDtm["dayOfMonth"],
                                                  (int)scheduledDtm["hourOfDay"],
                                                  (int)scheduledDtm["minute"],
                                                  (int)scheduledDtm["second"], DateTimeKind.Local);
            return tripDtm;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return DateTime.MinValue;
        }
     
    }
    private int GetTripMin(DateTime tripDtm)
    {
        try
        {
            return (int)Math.Ceiling(tripDtm.Subtract(GetDTMNow()).TotalMinutes);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return 0;
        }

    }
    private DateTime GetDTMNow()
    {
        try
        {
           return _siteInfo.GetCurrentTimeInTimeZone(DateTime.Now);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return DateTime.Now;
        }

    }

    public async Task<IEnumerable<GeoZone>>  GetGeoZone(string zoneType)
    {
        try
        {
            var geoZones = await Task.Run(() =>
                _geoZoneList.Values.Where(gz => gz.Properties.Type == zoneType).ToList()
            );
            return geoZones;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Enumerable.Empty<GeoZone>();
        }
    }

    public Task<bool> ResetGeoZoneList()
    {
        try
        {
            _geoZoneList.Clear();
            _geoZoneDockDoorList.Clear();
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
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Task.FromResult(true);
        }
    }
}