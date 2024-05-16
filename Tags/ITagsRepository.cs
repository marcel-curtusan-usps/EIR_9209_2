using EIR_9209_2.Models;

public interface ITagsRepository
{
    Task Add(GeoMarker tag);
    Task Delete(string id);
    Task<GeoMarker> Get(string id);
    Task<List<GeoMarker>> GetAllPersonTag();
    Task Update(GeoMarker tag);
}