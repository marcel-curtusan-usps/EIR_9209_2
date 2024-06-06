using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IInMemoryTagsRepository
{
    void Add(GeoMarker tag);
    void Remove(string tagId);
    object Get(string id);
    List<GeoMarker> GetAll();
    void Update(GeoMarker tag);
    void LocalAdd(GeoMarker tag);
    void UpdateEmployeeInfo(JObject emp);
    void UpdateBadgeTransactionScan(JObject transaction);
    string GetCraftType(string tagId);
}