using EIR_9209_2.Models;

public interface ITagsRepository
{
    Task Add(TagGeoJson tag);
    Task Delete(string id);
    Task<TagGeoJson> Get(string id);
    Task<List<TagGeoJson>> GetAllPersonTag();
    Task Update(TagGeoJson tag);
}