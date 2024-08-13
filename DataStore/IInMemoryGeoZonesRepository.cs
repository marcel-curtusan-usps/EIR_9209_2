using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IInMemoryGeoZonesRepository
{
    Task<GeoZone> Add(GeoZone geoZone);
    Task<GeoZone> Remove(string geoZoneId);
    Task<GeoZone> Update(GeoZone geoZone);
    Task<GeoZone> UiUpdate(GeoZone geoZone);
    GeoZone Get(string id);
    IEnumerable<GeoZone> GetAll();
    GeoZone GetMPEName(string MPEName);
    object GetZoneNameList(string type);
    bool ExistingAreaDwell(DateTime hour);
    List<AreaDwell> GetAreaDwell(DateTime hour);
    void UpdateAreaDwell(DateTime hour, List<AreaDwell> newValue, List<AreaDwell> currentvalue);
    void AddAreaDwell(DateTime hour, List<AreaDwell> newValue);
    object getMPESummary(string mpe);
    List<MPEActiveRun> getMPERunActivity(string mpe);
    void RunMPESummaryReport();
    Task<bool> UpdateMPERunInfo(List<MPERunPerformance> mpe);
    void ProcessIDSData(JToken result);
    void UpdateMPERunActivity(MPERunPerformance mpe);
    Task LoadMPEPlan(JToken data);
    Task LoadWebEORMPERun(JToken data);
}