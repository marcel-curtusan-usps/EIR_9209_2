using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace EIR_9209_2.Models
{
    public class GeoZone
    {
        [BsonId]
        [JsonIgnore]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string _id { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; } = "Feature";

        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; } = new Geometry();

        [JsonProperty("properties")]
        public Properties Properties { get; set; } = new Properties();
    }

    public class Properties
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "";

        [JsonProperty("floorId")]
        public string FloorId { get; set; } = "";

        [JsonProperty("visible")]
        public bool Visible { get; set; } = false;

        [JsonProperty("color")]
        public string Color { get; set; } = "";

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("type")]
        public string Type { get; set; } = "";

        [JsonProperty("zoneType")]
        public string ZoneType { get; set; } = "";

        [JsonProperty("mpeType")]
        public string MpeType { get; set; } = "";

        [JsonProperty("MPERunPerformance")]
        public MPERunPerformance? MPERunPerformance { get; set; } = new();
        public string? DataSource { get; set; } = "";
    }

    public class Geometry
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "Polygon";

        [JsonProperty("coordinates")]
        public List<List<List<double>>> Coordinates { get; set; } = new List<List<List<double>>>();
    }
}
