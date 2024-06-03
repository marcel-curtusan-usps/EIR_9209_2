using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

public class ReportQuery
{
    //Example:
    //"tagIds":null,
    //"groupIds":["2"],
    //"integrationIds":null,
    //"minTimeOnArea":300000,
    //"timeStep":60000,
    //"activationTime":1000,
    //"deactivationTime":2000,
    //"disappearTime":10000,
    //"areaIds":["01H2GH3H040WYW9HF092ZJPBVH"],
    //"areaGroupIds":[],
    //"startTime":1702011600000,
    //"endTime":1702015200000,
    //"type":"TAG_queryREGATION"

    [JsonProperty("ID")] public List<string> TagIds { get; set; } //if you want to filter by a specific badge ID, otherwise leave null
    public List<string> GroupIds { get; set; } //represents employee type - comes from the "api/v1/trackgroup/list?types=STATS_GROUP&showNonEmpty=true" call
    public List<object> IntegrationIds { get; } = null; //unknown - leave null
    [Required, JsonConverter(typeof(TimeSpanMillisecondsConverter))] public TimeSpan? MinTimeOnArea { get; set; } //change this based on desired minimum tag in area duration
    [JsonConverter(typeof(TimeSpanMillisecondsConverter))] public TimeSpan TimeStep { get; set; }
    [JsonConverter(typeof(TimeSpanMillisecondsConverter))] public TimeSpan ActivationTime { get; set; }
    [JsonConverter(typeof(TimeSpanMillisecondsConverter))] public TimeSpan DeactivationTime { get; set; }
    [JsonConverter(typeof(TimeSpanMillisecondsConverter))] public TimeSpan DisappearTime { get; set; }
    public List<string> AreaIds { get; set; } //represents individual site areas - comes from the "api/usps/area/list" call
    public List<string> AreaGroupIds { get; set; } //represents a group of site areas - comes from the "api/usps/group/area/list" call
    [JsonConverter(typeof(DateTimeUnixEpochMillisecondsTimeConverter)), Required] public DateTime? StartTime { get; set; } //unix timestamp in local time
    [JsonConverter(typeof(DateTimeUnixEpochMillisecondsTimeConverter)), Required] public DateTime? EndTime { get; set; } //unix timestamp in local time
    [JsonConverter(typeof(EnumDescriptionConverter<ESelsReportQueryType>)), Required] public ESelsReportQueryType? Type { get; set; } //defines the query type - see enum SelsDataQueryType;
    public string Search { get; set; } = null;
    public string Order { get; set; } = null;
    public string Desc { get; set; } = null;
}