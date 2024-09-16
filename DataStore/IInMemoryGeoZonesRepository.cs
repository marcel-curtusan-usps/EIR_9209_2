using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IInMemoryGeoZonesRepository
{
    Task<GeoZone> Add(GeoZone geoZone);
    Task<GeoZone> Remove(string geoZoneId);
    Task<GeoZoneDockDoor> AddDockDoor(GeoZoneDockDoor geoZone);
    Task<GeoZoneDockDoor> RemoveDockDoor(string geoZoneId);
    Task<GeoZoneDockDoor> UpdateDockDoor(GeoZoneDockDoor geoZone);
    Task<JObject> Update(JObject geoZone);
    Task<GeoZone> UiUpdate(GeoZone geoZone);
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
    Task<bool> UpdateMPERunInfo(List<MPERunPerformance> mpe);
    Task ProcessIDSData(JToken result);
    void UpdateMPERunActivity(List<MPERunPerformance> mpe);
    Task LoadMPEPlan(JToken data);
    Task LoadWebEORMPERun(JToken data);
        //List<TagTimeline> GetTagTimelineList(string ein);
    Task<object?> GetMPENameList();
    Task<object?> GetDockDoorNameList();
    Task<object?> GetMPEGroupList(string type);
    Task<List<MPESummary>> getMPESummaryDateRange(string mpe, DateTime startDT, DateTime endDT);
    IEnumerable<GeoZoneDockDoor>? GetDockDoor();
    Task ProcessSVDoorsData(JToken result);
    Task<IEnumerable<GeoZone>> GetGeoZone(string zoneType);
    Task ProcessSVContainerData(JToken result);
}