using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;
using static EIR_9209_2.Models.GeoMarker;

public interface IInMemoryTagsRepository
{
    Task Add(GeoMarker tag);
    Task Delete(string tagId);
    Task<object> UpdateTagUIInfo(JObject tagInfo);
    Task UpdateTagQPEInfo(List<Tags> tags);
    object Get(string id);
    List<GeoMarker> GetAll();
    List<GeoMarker> GetTagsType(string type);
    List<GeoMarker> GetAllPIV();
    List<GeoMarker> GetAllAGV();
    void UpdateEmployeeInfo(JToken emp);
    void UpdateBadgeTransactionScan(JObject transaction);
    string GetCraftType(string tagId);
    bool UpdateTagDesignationActivity(DesignationActivityToCraftType updatedDacode);
    //void UpdateTagQPEInfo(List<Tags> tags);
    List<Marker> SearchTag(string searchValue);
    List<string> GetTagByType(string tagType);
    Task UpdateTagCiscoSpacesClientInfo(JToken result);
    Task UpdateTagCiscoSpacesBLEInfo(JToken result);
    Task UpdateTagCiscoSpacesAPInfo(JToken result);
    bool ExistingTagTimeline(DateTime hour);
    List<TagTimeline> GetTagTimeline(DateTime hour);
    void UpdateTagTimeline(DateTime hour, List<TagTimeline> newValue, List<TagTimeline> currentvalue);
    void AddTagTimeline(DateTime hour, List<TagTimeline> newValue);
    void RemoveTagTimeline(DateTime hour);
    List<TagTimeline> GetTagTimelineList(string ein);
}