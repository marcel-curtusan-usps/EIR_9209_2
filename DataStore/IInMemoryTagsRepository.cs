using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;
using static EIR_9209_2.Models.GeoMarker;

public interface IInMemoryTagsRepository
{
    void Add(GeoMarker tag);
    void Remove(string tagId);
    object Get(string id);
    List<GeoMarker> GetAll();
    List<GeoMarker> GetAllPerson();
    List<GeoMarker> GetAllPIV();
    List<GeoMarker> GetAllAGV();
    void Update(GeoMarker tag);
    void UpdateEmployeeInfo(JToken emp);
    void UpdateBadgeTransactionScan(JObject transaction);
    string GetCraftType(string tagId);
    object UpdateTagInfo(JObject tagInfo);
    bool UpdateTagDesignationActivity(DesignationActivityToCraftType updatedDacode);
    //void UpdateTagQPEInfo(List<Tags> tags);
    List<Marker> SearchTag(string searchValue);
    List<string> GetTagByType(string tagType);
    Task UpdateTagQPEInfo(List<Tags> tags);
}