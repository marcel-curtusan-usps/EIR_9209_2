using Newtonsoft.Json;

namespace EIR_9209_2.Models
{
    public class GeoZone
    {
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
    }

    public class Geometry
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "Polygon";

        [JsonProperty("coordinates")]
        public List<List<List<double>>> Coordinates { get; set; } = new List<List<List<double>>>();
    }
}
