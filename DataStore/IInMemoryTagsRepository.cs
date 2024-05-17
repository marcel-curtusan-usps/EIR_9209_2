using EIR_9209_2.Models;

public interface IInMemoryTagsRepository
{
    void Add(GeoMarker connection);
    void Remove(string connectionId);
    GeoMarker Get(string id);
    List<GeoMarker> GetAll();
    void Update(GeoMarker connection);
}