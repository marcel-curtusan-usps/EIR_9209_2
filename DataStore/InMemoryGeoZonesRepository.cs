using EIR_9209_2.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using static EIR_9209_2.Models.GeoMarker;

public class InMemoryGeoZonesRepository : IInMemoryGeoZonesRepository
{
    private readonly ConcurrentDictionary<string, GeoZone> _geoZoneList = new();
    private readonly ConcurrentDictionary<string, Dictionary<DateTime, MPESummary>> _mpeSummary = new();
    private readonly IConfiguration _configuration;
    private readonly ILogger<InMemoryGeoZonesRepository> _logger;
    private readonly IFileService FileService;
    private readonly IInMemoryTagsRepository _tags;
    public InMemoryGeoZonesRepository(ILogger<InMemoryGeoZonesRepository> logger, IConfiguration configuration, IFileService fileService, IInMemoryTagsRepository tags)
    {
        FileService = fileService;
        _logger = logger;
        _configuration = configuration;
        _tags = tags;

        string BuildPath = Path.Combine(configuration[key: "ApplicationConfiguration:BaseDrive"], configuration[key: "ApplicationConfiguration:BaseDirectory"], configuration[key: "SiteIdentity:NassCode"], configuration[key: "ApplicationConfiguration:ConfigurationDirectory"], $"{configuration[key: "InMemoryCollection:CollectionZones"]}.json");
        // Load data from the first file into the first collection
        _ = LoadDataFromFile(BuildPath);

    }
    public void Add(GeoZone geoZone)
    {
        _geoZoneList.TryAdd(geoZone.Properties.Id, geoZone);
    }

    public void Remove(string geoZoneId)
    {
        _geoZoneList.TryRemove(geoZoneId, out _);
    }

    public GeoZone Get(string id)
    {
        _geoZoneList.TryGetValue(id, out GeoZone geoZone);

        return geoZone;
    }
    public GeoZone GetMPEName(string MPEName)
    {
        return _geoZoneList.Where(r => r.Value.Properties.ZoneType == "MPEZone" && r.Value.Properties.Name == MPEName).Select(y => y.Value).FirstOrDefault();
    }

    public IEnumerable<GeoZone> GetAll() => _geoZoneList.Values;

    public void Update(GeoZone geoZone)
    {
        if (_geoZoneList.TryGetValue(geoZone.Properties.Id, out GeoZone currentGeoZone))
        {
            _geoZoneList.TryUpdate(geoZone.Properties.Id, geoZone, currentGeoZone);
        }
    }

    public void RunMPESummaryReport()
    {
        try
        {
            List<string> areasList = _geoZoneList.Values.Select(item => item.Properties.Name).Distinct().ToList();
            if (areasList.Any())
            {
                foreach (var area in areasList)
                {
                    var mpe = _geoZoneList.Where(r => r.Value.Properties.Name == area && r.Value.Properties.ZoneType == "MPEZone").Select(y => y.Value.Properties).FirstOrDefault();
                    List<DateTime> hoursInMpeDateTime = mpe.MPERunPerformance.HourlyData.Select(x => DateTime.Parse(x.Hour, CultureInfo.CurrentCulture, DateTimeStyles.None)).ToList();
                    if (!_mpeSummary.ContainsKey(area))
                    {
                        _mpeSummary.TryAdd(area, new Dictionary<DateTime, MPESummary>());
                        foreach (var hour in hoursInMpeDateTime)
                        {
                            var hourlySummaryForHourAndArea = GetHourlySummaryForHourAndArea(area, hour, mpe.MPERunPerformance);
                            _mpeSummary[area][hour] = hourlySummaryForHourAndArea;
                        }
                    }
                    else
                    {
                        foreach (var hour in hoursInMpeDateTime)
                        {
                            var hourlySummaryForHourAndArea = GetHourlySummaryForHourAndArea(area, hour, mpe.MPERunPerformance);
                            _mpeSummary[area][hour] = hourlySummaryForHourAndArea;
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

    private MPESummary GetHourlySummaryForHourAndArea(string area, DateTime Dateandhour, MPERunPerformance mpe)
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
            var entriesThisArea = _tags.GetAreaDwell(Dateandhour);
            if (entriesThisArea != null)
            {
                //if where pieces Sorted is available the calculate the actual yield using the sorted pieces
                var piecesForYield = piecesSortedThisHour != 0 ? piecesSortedThisHour : piecesCountThisHour;
                var clerkAndMailHandlerCountThisHour = ((entriesThisArea.Where(e => Regex.IsMatch(e.Type, "^(clerk|mail handler)", RegexOptions.IgnoreCase)).Sum(g => g.DwellTimeDurationInArea.TotalMilliseconds)) / (1000 * 60 * 60));
                actualYieldcal = piecesForYield != null && clerkAndMailHandlerCountThisHour > 0 ? piecesForYield.Value / clerkAndMailHandlerCountThisHour : 0.0;
                if (mpe.HourlyData.Where(r => r.Hour == hour).Select(y => y.Count).Any())
                {
                    actualYieldcal = mpe.HourlyData.Where(r => r.Hour == hour).Select(y => y.Count).First() / ((entriesThisArea.Where(e => Regex.IsMatch(e.Type, "^(clerk|mail handler)", RegexOptions.IgnoreCase)).Sum(g => g.DwellTimeDurationInArea.TotalMilliseconds)) / (1000 * 60 * 60));
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
                mpeName = mpe.MpeId,
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

    private async Task LoadDataFromFile(string filePath)
    {
        try
        {
            // Read data from file
            var fileContent = await FileService.ReadFile(filePath);

            // Parse the file content to get the data. This depends on the format of your file.
            // Here's an example if your file was in JSON format and contained an array of T objects:
            List<GeoZone> data = JsonConvert.DeserializeObject<List<GeoZone>>(fileContent);

            // Insert the data into the MongoDB collection
            if (data.Any())
            {
                foreach (GeoZone item in data.Select(r => r).ToList())
                {
                    _geoZoneList.TryAdd(item.Properties.Id, item);
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

    public object GetZoneNameList(string type)
    {
        return _geoZoneList.Where(r => r.Value.Properties.ZoneType.StartsWith(type)).Select(y => y.Value.Properties.Name).ToList();
    }
}