using Newtonsoft.Json;

namespace EIR_9209_2.Models
{
    public class GeoZoneCube
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "Feature";
        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; } = new Geometry();
        [JsonProperty("properties")]
        public CubeProperties Properties { get; set; } = new CubeProperties();
    }
    public class CubeProperties
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "";
        [JsonProperty("floorId")]
        public string FloorId { get; set; } = "";
        [JsonProperty("visible")]
        public bool Visible { get; set; } = false;
        [JsonProperty("name")]
        public string Name { get; set; } = "";
        [JsonProperty("number")]
        public string Number { get; set; } = "";
        [JsonProperty("type")]
        public string Type { get; set; } = "";
        [JsonProperty("ein")]
        public string EIN { get; set; } = "";
        [JsonProperty("assignTo")]
        public string AssignTo { get; set; } = "";
        [JsonProperty("inZone")]
        public bool InZone { get; set; }
        [JsonProperty("scanTime")]
        public DateTime ScanTime { get; set; }
    }
}

