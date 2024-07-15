using Newtonsoft.Json;

namespace EIR_9209_2.Models
{
    public class QuuppaTag
    {
        [JsonProperty("code")]
        public string Code { get; set; } = "";

        [JsonProperty("status")]
        public string Status { get; set; } = "";

        [JsonProperty("command")]
        public string Command { get; set; } = "";

        [JsonProperty("message")]
        public string Message { get; set; } = "";

        [JsonProperty("responseTS")]
        public long ResponseTS { get; set; } = 0;

        [JsonProperty("version")]
        public string Version { get; set; } = "";

        [JsonProperty("formatId")]
        public string FormatId { get; set; } = "";

        [JsonProperty("formatName")]
        public string FormatName { get; set; } = "";

        [JsonProperty("tags")]
        public List<Tags> Tags { get; set; } = [];

    }
    public class Tags
    {
        [JsonProperty("tagId")]
        public string TagId { get; set; } = "";
        [JsonProperty("deviceType", NullValueHandling = NullValueHandling.Ignore)]
        public string DeviceType { get; set; } = "";

        [JsonProperty("tagName", NullValueHandling = NullValueHandling.Ignore)]
        public string TagName { get; set; } = "";

        [JsonProperty("tagState", NullValueHandling = NullValueHandling.Ignore)]
        public string TagState { get; set; } = "";

        [JsonProperty("tagStateTS", NullValueHandling = NullValueHandling.Ignore)]
        public long LagStateTS { get; set; } = 0;

        [JsonProperty("color")]
        public string Color { get; set; } = "";

        [JsonProperty("tagGroupName", NullValueHandling = NullValueHandling.Ignore)]
        public string TagGroupName { get; set; } = "";

        [JsonProperty("locationType", NullValueHandling = NullValueHandling.Ignore)]
        public string LocationType { get; set; } = "";

        [JsonProperty("lastSeenTS", NullValueHandling = NullValueHandling.Ignore)]
        public long LastSeenTS { get; set; } = 0;

        [JsonProperty("locationMovementStatus", NullValueHandling = NullValueHandling.Ignore)]
        public string LocationMovementStatus { get; set; } = "";

        [JsonProperty("locationRadius", NullValueHandling = NullValueHandling.Ignore)]
        public double? LocationRadius { get; set; } = 0.0;

        [JsonProperty("location", NullValueHandling = NullValueHandling.Ignore)]
        public List<double> Location { get; set; } = new List<double>();

        [JsonProperty("locationTS", NullValueHandling = NullValueHandling.Ignore)]
        public long LocationTS { get; set; } = 0;

        [JsonProperty("lastPacketTS", NullValueHandling = NullValueHandling.Ignore)]
        public long LastPacketTS { get; set; } = 0;

        [JsonProperty("triggerCount", NullValueHandling = NullValueHandling.Ignore)]
        public long TriggerCount { get; set; } = 0;

        [JsonProperty("triggerCountTS", NullValueHandling = NullValueHandling.Ignore)]
        public long TriggerCountTS { get; set; } = 0;

        [JsonProperty("locationCoordSysId", NullValueHandling = NullValueHandling.Ignore)]
        public string LocationCoordSysId { get; set; } = "";

        [JsonProperty("locationCoordSysName", NullValueHandling = NullValueHandling.Ignore)]
        public string LocationCoordSysName { get; set; } = "";

        [JsonProperty("locationZoneIds", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> LocationZoneIds { get; set; } = [];

        [JsonProperty("locationZoneNames", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> LocationZoneNames { get; set; } = [];
        public long ServerTS { get; internal set; }
    }
}