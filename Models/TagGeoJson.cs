
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Newtonsoft.Json;
using System.Reflection;

namespace EIR_9209_2.Models
{
    public class TagGeoJson
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string _id { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; } = "Feature";
        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; } = new Geometry();
        [JsonProperty("properties")]
        public Marker Properties { get; set; } = new Marker();

        public class Marker
        {
            [JsonProperty("id")]
            public string Id { get; set; } = "";
            [JsonProperty("floorId")]
            public string FloorId { get; set; }
            [JsonProperty("visible")]
            public bool Visible { get; set; } = false;
            [JsonProperty("zones")]
            public List<string> Zones { get; set; } = new List<string>();
            [JsonProperty("color")]
            public string Color { get; set; } = "";
            [JsonProperty("posAge")]
            public long posAge { get; set; } = 0;
            [JsonProperty("name")]
            public string Name { get; set; } = "";
            [JsonProperty("craftName")]
            public string CraftName { get; set; } = "N/A";
            [JsonProperty("positionTS_txt")]
            public DateTime PositionTS_txt { get; set; } = DateTime.MinValue;
            [JsonProperty("positionTS")]
            public long PositionTS { get; set; } = 0;
            [JsonProperty("Tag_Type")]
            public string TagType { get; set; } = "";
            [JsonProperty("Tag_Update")]
            public bool TagUpdate { get; set; }
            [JsonProperty("serverTS_txt")]
            public DateTime ServerTS_txt { get; set; } = DateTime.MinValue;
            [JsonProperty("serverTS")]
            public long ServerTS { get; set; } = 0;
            [JsonProperty("isSch")]
            public bool isSch { get; set; }
            [JsonProperty("isTacs")]
            public bool isTacs { get; set; }
            [JsonProperty("isePacs")]
            public bool isePacs { get; set; }
            [JsonProperty("isPosition")]
            public bool isPosition { get; set; }
            [JsonProperty("cardholderId")]
            public int CardHolderId { get; set; } = 0;
            [JsonProperty("ein")]
            public string EIN { get; set; } = "";
            [JsonProperty("designationActivity")]
            public string DesignationActivity { get; set; } = "";
            [JsonProperty("ldc")]
            public string LDC { get; set; } = "";
            [JsonProperty("payLocation")]
            public string PayLocation { get; set; } = "";
            [JsonProperty("empFirstName")]
            public string EmpFirstName { get; set; } = "";
            [JsonProperty("empLastName")]
            public string EmpLastName { get; set; } = "";
            [JsonProperty("encodedID")]
            public string EncodedID { get; set; } = "";
            [JsonProperty("bdate", NullValueHandling = NullValueHandling.Ignore)]
            public DateTime Bdate { get; set; } = DateTime.MinValue;
            [JsonProperty("blunch", NullValueHandling = NullValueHandling.Ignore)]
            public string Blunch { get; set; } = "";
            [JsonProperty("elunch", NullValueHandling = NullValueHandling.Ignore)]
            public string Elunch { get; set; } = "";
            [JsonProperty("edate", NullValueHandling = NullValueHandling.Ignore)]
            public DateTime Edate { get; set; } = DateTime.MinValue;
            [JsonProperty("tourNumber")]
            public string TourNumber { get; set; } = "";
            [JsonProperty("reqDate")]
            public DateTime ReqDate { get; set; } = DateTime.MinValue;
            [JsonProperty("daysOff")]
            public string DaysOff { get; set; } = "";
            [JsonProperty("source")]
            public string Source { get; set; } = "";
            [JsonProperty("notificationId")]
            public string NotificationId { get; set; } = "";
            [JsonProperty("zoneName")]
            public string ZonesNames { get; internal set; } = "";
            [JsonProperty("locationMovementStatus")]
            public string LocationMovementStatus { get; internal set; } = "";
            [JsonProperty("locationType")]
            public string LocationType { get; set; } = "";
            [JsonProperty("lastSeenTS_txt")]
            public DateTime LastSeenTS_txt { get; internal set; } = DateTime.MinValue;
            [JsonProperty("lastSeenTS")]
            public long LastSeenTS { get; internal set; } = 0;
        }
    }
}
