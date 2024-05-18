using Newtonsoft.Json;

public class SiteInformation
{
    [JsonProperty("siteId")]
    public string SiteId { get; set; } = "";

    [JsonProperty("type")]
    public string Type { get; set; } = "";

    [JsonProperty("name")]
    public string Name { get; set; } = "";

    [JsonProperty("displayName")]
    public string DisplayName { get; set; } = "";

    [JsonProperty("timeZoneAbbr")]
    public string TimeZoneAbbr { get; set; } = "";

    [JsonProperty("financeNumber")]
    public string FinanceNumber { get; set; } = "";

    [JsonProperty("zipCode")]
    public string ZipCode { get; set; } = "";

    [JsonProperty("areaCode")]
    public string AreaCode { get; set; } = "";

    [JsonProperty("areaName")]
    public string AreaName { get; set; } = "";

    [JsonProperty("districtCode")]
    public string DistrictCode { get; set; } = "";

    [JsonProperty("districtName")]
    public string DistrictName { get; set; } = "";

    [JsonProperty("localeKey")]
    public string LocaleKey { get; set; } = "";

    [JsonProperty("fdbId")]
    public string FdbId { get; set; } = "";

    [JsonProperty("updtUserId")]
    public string UpdtUserId { get; set; } = "";

    [JsonProperty("tours")]
    public Tours Tours { get; set; } = new Tours();

    [JsonProperty("facilityId")]
    public string FacilityId { get; set; } = "";

    [JsonProperty("agvInd")]
    public string AgvInd { get; set; } = "";
    [JsonProperty("nassCode")]
    public string? NassCode { get; internal set; }
}
public class Tours
{
    [JsonProperty("siteId")]
    public string SiteId { get; set; } = "";

    [JsonProperty("tour1Start")]
    public string Tour1Start { get; set; } = "";

    [JsonProperty("tour1End")]
    public string Tour1End { get; set; } = "";

    [JsonProperty("tour2Start")]
    public string Tour2Start { get; set; } = "";

    [JsonProperty("tour2End")]
    public string Tour2End { get; set; } = "";

    [JsonProperty("tour3Start")]
    public string Tour3Start { get; set; } = "";

    [JsonProperty("tour3End")]
    public string Tour3End { get; set; } = "";

    [JsonProperty("startingTour")]
    public int StartingTour { get; set; } = 0;

    [JsonProperty("updtUserId")]
    public string UpdtUserId { get; set; } = "";
}