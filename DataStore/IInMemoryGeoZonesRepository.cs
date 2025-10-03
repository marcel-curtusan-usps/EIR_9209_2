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
    /// <summary>
    /// Get a geo zone by its ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<object> Get(string id);
    /// <summary>
    /// Get all zones
    /// </summary>
    /// <returns></returns>
    Task<object> GetAll();
    /// <summary>
    /// Get all zones by type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    Task<List<string>> GetZoneNameList(string type);
    /// <summary>
    /// Check if area dwell data exists for the given hour
    /// </summary>
    /// <param name="hour"></param>
    /// <returns></returns>
    bool ExistingAreaDwell(DateTime hour);
    /// <summary>
    /// Get area dwell data for the given hour
    /// </summary>
    /// <param name="hour"></param>
    /// <returns></returns>
    List<AreaDwell> GetAreaDwell(DateTime hour);
    /// <summary>
    /// Update area dwell data for the given hour
    /// </summary>
    /// <param name="hour"></param>
    /// <param name="newValue"></param>
    /// <param name="currentvalue"></param>
    void UpdateAreaDwell(DateTime hour, List<AreaDwell> newValue, List<AreaDwell> currentvalue);
    /// <summary>
    /// Add area dwell data for the given hour
    /// </summary>
    /// <param name="hour"></param>
    /// <param name="newValue"></param>

    /// <summary>
    /// Add report content item to the repository
    /// </summary>
    /// <param name="contentItem"></param>
    Task<bool> AddReportContentItem(DateTime hour, List<ReportContentItems> contentItem);
    /// <summary>
    /// Add area dwell data for the given hour
    /// </summary>
    /// <param name="hour"></param>
    /// <param name="newValue"></param>
    void AddAreaDwell(DateTime hour, List<AreaDwell> newValue);
    /// <summary>
    /// Add report request for the given hour
    /// </summary>
    /// <param name="newValue"></param>
    Task AddReportResponse(ReportResponse newValue);
    /// <summary>
    /// Remove report response for the given report ID
    /// </summary>
    /// <param name="reportId"></param>
    Task RemoveReportResponse(string reportId);
    /// <summary>
    /// Update report response for the given report ID and state
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="state"></param>
    Task UpdateReportResponse(string reportId, string state);
    /// <summary>
    ///     Get report responses for the given hour
    /// </summary>
    /// <returns></returns>
    Task<List<ReportResponse>> GetReportList(CancellationToken cancellationToken);
    /// <summary>
    /// Check if a report exists in the list for the given hour
    /// </summary>
    /// <param name="hour"></param>
    /// <returns></returns>
    Task<bool> ExistingReportInList(DateTime hour);

    /// <summary>
    ///     Get report requests for the given hour
    /// </summary>
    /// <param name="mpe"></param>
    /// <returns></returns>
    object getMPESummary(string mpe);
    /// <summary>
    /// Get MPE run activity for the given MPE
    /// </summary>
    /// <param name="mpe"></param>
    /// <returns></returns>
    List<MPEActiveRun> getMPERunActivity(string mpe);
    /// <summary>
    /// Run MPE summary report
    /// </summary>
    void RunMPESummaryReport();
    /// <summary>
    /// Update MPE run information
    /// </summary>
    /// <param name="mpe"></param>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    Task<bool> UpdateMPERunInfo(List<MPERunPerformance> mpe, CancellationToken stoppingToken);
    Task<bool> ProcessIDSData(JToken result, CancellationToken stoppingToken);
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
    Task<object> GetGeoZonebyType(string zoneType);
    Task<object> GetGeoZonebyName(string type, string name);
    Task<object> GetGeoZonesTypeByFloorId(string floorId, string type);
    Task ProcessSVContainerData(JToken result);
    Task<bool> ResetGeoZoneList();
    Task<bool> SetupGeoZoneData();
    Task<bool> ProcessQPEGeoZone(List<CoordinateSystem> coordinateSystems, CancellationToken stoppingToken);
    Task<MPERunPerformance> GetGeoZoneMPEPerformanceData(string zoneName);

    #region //MPE Targets
    Task<List<TargetHourlyData>> GetAllMPETragets();
    Task<List<TargetHourlyData>> GetMPETargets(string mpeId);
    Task<List<TargetHourlyData>> AddMPETargets(JToken mpeData);
    Task<List<TargetHourlyData>> UpdateMPETargets(JToken mpeData);
    Task<TargetHourlyData> RemoveMPETargets(JToken mpeData);
    Task<bool> LoadCSVMpeTargets(List<TargetHourlyData> targetHourlyDatas);

    #endregion
    #region //CRS Kiosk
    Task<object> GetAllKiosk();
    Task<KioskDetails> CheckKioskZone(string deviceId);
    Task<GeoZoneKiosk> AddKiosk(GeoZoneKiosk newZone);
    Task<GeoZoneKiosk> RemoveKiosk(string id);
    Task<GeoZoneKiosk> UpdateKiosk(KioskProperties updatedKioskZone);
    Task<GeoZoneKiosk> GetKiosk(string id);


    #endregion
    #region //Cubes
    void updateEpacsScanInCube(ScanInfo scan);
    Task<object> GetAllCube();
    Task<GeoZoneCube> AddCube(GeoZoneCube newZone);
    Task<GeoZoneCube> RemoveCube(string id);
    Task<GeoZoneCube> UpdateCube(CubeProperties updatedCubeZone);
    Task<GeoZoneCube> GetCube(string id);
    #endregion
}