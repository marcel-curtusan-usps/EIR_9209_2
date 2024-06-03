using EIR_9209_2.Models;

public interface IInMemoryTagsRepository
{
    void Add(GeoMarker connection);
    void Remove(string connectionId);
    object Get(string id);
    List<GeoMarker> GetAll();
    void Update(GeoMarker connection);
    void LocalAdd(GeoMarker connection);
    bool ExiteingAreaDwell(DateTime hour);
    List<AreaDwell> GetAreaDwell(DateTime hour);
    void UpdateAreaDwell(DateTime hour, List<AreaDwell> newValue, List<AreaDwell> currentvalue);
    void AddAreaDwell(DateTime hour, List<AreaDwell> newValue);
}