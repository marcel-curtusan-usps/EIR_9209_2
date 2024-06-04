using EIR_9209_2.Models;

public interface IInMemoryGeoZonesRepository
{
    GeoZone Add(GeoZone geoZone);
    GeoZone Remove(string geoZoneId);
    GeoZone Get(string id);
    IEnumerable<GeoZone> GetAll();
    GeoZone Update(GeoZone geoZone);
    GeoZone GetMPEName(string MPEName);
    object GetZoneNameList(string type);
    bool ExiteingAreaDwell(DateTime hour);
    List<AreaDwell> GetAreaDwell(DateTime hour);
    void UpdateAreaDwell(DateTime hour, List<AreaDwell> newValue, List<AreaDwell> currentvalue);
    void AddAreaDwell(DateTime hour, List<AreaDwell> newValue);
    Dictionary<DateTime, MPESummary> getMPESummary(string mpe);
    void RunMPESummaryReport();
}