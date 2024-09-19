using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IInMemoryTagsRepository
{
    Task Add(GeoMarker tag);
    Task Delete(string tagId);
    Task<object> UpdateTagUIInfo(JObject tagInfo);
    //void UpdateTagQPEInfo(List<Tags> tags, long responseTS);
    Task UpdateTagQPEInfo(List<Tags> tags, long responseTS);
    object Get(string id);
    List<GeoMarker> GetAll();
    List<GeoMarker> GetTagsType(string type);
    List<VehicleGeoMarker> GetAllPIV();
    List<VehicleGeoMarker> GetAllAGV();
    void UpdateEmployeeInfo(JToken emp);
    void UpdateBadgeTransactionScan(JObject transaction);
    string GetCraftType(string tagId);
    void UpdateTagDesignationActivity(DesignationActivityToCraftType updatedDacode);
    IEnumerable<JObject> SearchTag(string searchValue);
    List<string> GetTagByType(string tagType);
    Task UpdateTagCiscoSpacesClientInfo(JToken result);
    Task UpdateTagCiscoSpacesBLEInfo(JToken result);
    Task UpdateTagCiscoSpacesAPInfo(JToken result);
    bool ExistingTagTimeline(DateTime hour);
    Task<List<TagTimeline>> GetTagTimeline(string emp, DateTime hour);    
    void UpdateTagTimeline(DateTime hour, List<TagTimeline> newValue, List<TagTimeline> currentvalue);
    void AddTagTimeline(DateTime hour, List<TagTimeline> newValue);
    void RemoveTagTimeline(DateTime hour);
    List<TagTimeline> GetCurrentTagTimeline(DateTime hour);
    Task<bool> ResetTagList();
    Task<bool> SetupTagList();
}