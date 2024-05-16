using EIR_9209_2.Models;

public interface IGeoZonesRepository
{
    Task<GeoZone> Get(string id);
    Task<List<GeoZone>> GetAll();
    Task Add(GeoZone zone);
    Task Update(GeoZone zone);
    Task Delete(string id);
}