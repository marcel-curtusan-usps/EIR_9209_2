using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IInMemoryTagsRepository
{
    void Add(GeoMarker tag);
    void Remove(string tagId);
    object Get(string id);
    List<GeoMarker> GetAll();
    List<GeoMarker> GetAllPerson();
    List<GeoMarker> GetAllPIV();
    void Update(GeoMarker tag);
    void LocalAdd(GeoMarker tag);
    void UpdateEmployeeInfo(JToken emp);
    void UpdateBadgeTransactionScan(JObject transaction);
    string GetCraftType(string tagId);
    void UpdateTagInfo(List<Tags> tags);
}