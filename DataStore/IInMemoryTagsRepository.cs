using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;
using static EIR_9209_2.Service.CiscoSpacesEndPointServices;

/// <summary>
/// Interface for managing in-memory tags repository.
/// </summary>
public interface IInMemoryTagsRepository
{
    /// <summary>Adds a new GeoMarker tag.</summary>
    Task Add(GeoMarker tag);

    /// <summary>Deletes a tag by its ID.</summary>
    Task Delete(string tagId);

    /// <summary>Updates the UI information of a tag.</summary>
    Task<object> UpdateTagUIInfo(JObject tagInfo);

    /// <summary>Updates QPE information for a list of tags.</summary>
    Task<bool> UpdateTagQPEInfo(List<Tags> tags, long responseTS, CancellationToken stoppingToken);

    /// <summary>Gets a tag by its ID.</summary>
    Task<object> Get(string id);

    /// <summary>Gets all GeoMarker tags.</summary>
    List<GeoMarker> GetAll();

    /// <summary>Updates badge transaction scan information.</summary>
    void UpdateBadgeTransactionScan(JObject transaction);

    /// <summary>Gets the craft type for a given tag ID.</summary>
    string GetCraftType(string tagId);

    /// <summary>Updates the designation activity for a tag.</summary>
    void UpdateTagDesignationActivity(DesignationActivityToCraftType updatedDacode);

    /// <summary>Searches for tags based on a search value.</summary>
    Task<List<JObject>> SearchTag(string searchValue);

    /// <summary>Gets a tag by its type.</summary>
    Task<object> GetTagByType(string tagType);

    /// <summary>Updates Cisco Spaces client information for tags.</summary>
    Task<bool> UpdateTagCiscoSpacesClientInfo(List<BLE_TAG> result, CancellationToken stoppingToken);

    /// <summary>Updates Cisco Spaces BLE information for tags.</summary>
    Task<bool> UpdateTagCiscoSpacesBLEInfo(List<BLE_TAG> result, CancellationToken stoppingToken);

    /// <summary>Updates Cisco Spaces AP information for tags.</summary>
    Task<bool> UpdateTagCiscoSpacesAPInfo(JToken result, CancellationToken stoppingToken);

    /// <summary>Checks if a tag timeline exists for a specific hour.</summary>
    bool ExistingTagTimeline(DateTime hour);

    /// <summary>Gets the tag timeline for a specific employee and hour.</summary>
    Task<List<TagTimeline>> GetTagTimeline(string emp, DateTime hour);

    /// <summary>Updates the tag timeline with new and current values.</summary>
    void UpdateTagTimeline(DateTime hour, List<TagTimeline> newValue, List<TagTimeline> currentvalue);

    /// <summary>Adds a new tag timeline for a specific hour.</summary>
    void AddTagTimeline(DateTime hour, List<TagTimeline> newValue);

    /// <summary>Removes a tag timeline for a specific hour.</summary>
    void RemoveTagTimeline(DateTime hour);

    /// <summary>Gets the current tag timeline for a specific hour.</summary>
    List<TagTimeline> GetCurrentTagTimeline(DateTime hour);

    /// <summary>Resets the tag list.</summary>
    Task<bool> ResetTagList();

    /// <summary>Sets up the tag list.</summary>
    Task<bool> SetupTagList();
}