using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IInMemoryGeoZonesRepository
{
    GeoZone Add(GeoZone geoZone);
    GeoZone Remove(string geoZoneId);
    GeoZone Get(string id);
    IEnumerable<GeoZone> GetAll();
    GeoZone Update(GeoZone geoZone);
    GeoZone GetMPEName(string MPEName);
    object GetZoneNameList(string type);
    bool ExistingAreaDwell(DateTime hour);
    List<AreaDwell> GetAreaDwell(DateTime hour);
    void UpdateAreaDwell(DateTime hour, List<AreaDwell> newValue, List<AreaDwell> currentvalue);
    void AddAreaDwell(DateTime hour, List<AreaDwell> newValue);
    object getMPESummary(string mpe);
    List<MPEActiveRun> getMPERunActivity(string mpe);
    void RunMPESummaryReport();
    void UpdateMPERunInfo(MPERunPerformance mpe);
    void ProcessIDSData(JToken result);
    void UpdateMPERunActivity(MPERunPerformance mpe);
}