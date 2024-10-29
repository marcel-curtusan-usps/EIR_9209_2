using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IInMemoryGeoZonesRepository
{
    Task<GeoZone> Add(GeoZone geoZone);
    Task<GeoZone> Remove(string geoZoneId);
    Task<GeoZoneDockDoor> AddDockDoor(GeoZoneDockDoor geoZone);
    Task<GeoZoneDockDoor> RemoveDockDoor(string geoZoneId);
    Task<GeoZoneDockDoor> UpdateDockDoor(GeoZoneDockDoor geoZone);
    Task<GeoZone> UiUpdate(Properties geoZone);
    Task<object> Get(string id);
    IEnumerable<GeoZone> GetAll();
    Task<List<string>> GetZoneNameList(string type);
    bool ExistingAreaDwell(DateTime hour);
    List<AreaDwell> GetAreaDwell(DateTime hour);
    void UpdateAreaDwell(DateTime hour, List<AreaDwell> newValue, List<AreaDwell> currentvalue);
    void AddAreaDwell(DateTime hour, List<AreaDwell> newValue);
    object getMPESummary(string mpe);
    List<MPEActiveRun> getMPERunActivity(string mpe);
    void RunMPESummaryReport();
    Task<bool> UpdateMPERunInfo(List<MPERunPerformance> mpe, CancellationToken stoppingToken);
    Task ProcessIDSData(JToken result);
    void UpdateMPERunActivity(List<MPERunPerformance> mpe);
    Task<bool> LoadMPEPlan(JToken data, CancellationToken stoppingToken);
    Task LoadWebEORMPERun(JToken data);
        //List<TagTimeline> GetTagTimelineList(string ein);
    Task<object?> GetMPENameList();
    Task<object?> GetDockDoorNameList();
    Task<object?> GetMPEGroupList(string type);
    Task<List<MPESummary>> getMPESummaryDateRange(string mpe, DateTime startDT, DateTime endDT);
    Task<List<GeoZoneDockDoor>> GetDockDoor();
    Task<bool> ProcessSVDoorsData(JToken result, CancellationToken stoppingToken);
    Task<IEnumerable<GeoZone>> GetGeoZone(string zoneType);
    Task ProcessSVContainerData(JToken result);
    Task<bool> ResetGeoZoneList();
    Task<bool> SetupGeoZoneData();
    Task<bool> ProcessQPEGeoZone(List<CoordinateSystem> coordinateSystems, CancellationToken stoppingToken);
    Task<MPERunPerformance> GetGeoZoneMPEPerformanceData(string zoneName);
    #region
    Task<List<TargetHourlyData>> GetAllMPETragets();
    Task<List<TargetHourlyData>> GetMPETargets(string mpeId);
    Task<bool> AddMPETargets(JToken mpeData);
    Task<bool> UpdateMPETargets(JToken mpeData);
    Task<bool> RemoveMPETargets(string mpeData);
    Task<bool> RemoveMPETargetsTour(JToken mpeData);
    #endregion
}