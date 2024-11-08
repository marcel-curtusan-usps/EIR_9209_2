using Newtonsoft.Json;

namespace EIR_9209_2.Models
{
    public class GeoZoneKiosk
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "Feature";
        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; } = new Geometry();
        [JsonProperty("properties")]
        public KioskProperties Properties { get; set; } = new KioskProperties();
    }
    public class KioskProperties
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
        [JsonProperty("url")]
        public string URL { get; set; } = "";
    }
}

