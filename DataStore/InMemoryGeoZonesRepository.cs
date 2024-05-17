using EIR_9209_2.Models;
using MongoDB.Driver.GeoJsonObjectModel;
using System.Collections.Concurrent;

public class InMemoryGeoZonesRepository : IInMemoryGeoZonesRepository
{
    public readonly static ConcurrentDictionary<string, GeoZone> _geoZoneList = new();

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

    public IEnumerable<GeoZone> GetAll() => _geoZoneList.Values;

    public void Update(GeoZone geoZone)
    {
        if (_geoZoneList.TryGetValue(geoZone.Properties.Id, out GeoZone currentGeoZone))
        {
            _geoZoneList.TryUpdate(geoZone.Properties.Id, geoZone, currentGeoZone);
        }
    }
}