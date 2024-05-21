using EIR_9209_2.Models;

public interface IInMemoryGeoZonesRepository
{
    void Add(GeoZone geoZone);
    void Remove(string geoZoneId);
    GeoZone Get(string id);
    IEnumerable<GeoZone> GetAll();

    void Update(GeoZone geoZone);
    GeoZone GetMPEName(string MPEName);
}