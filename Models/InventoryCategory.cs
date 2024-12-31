using Newtonsoft.Json;

namespace EIR_9209_2.Models
{
    public class InventoryCategory
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";
        [JsonProperty("code")]
        public string Code { get; set; } = "";
        [JsonProperty("description")]
        public string Description { get; set; } = "";
        [JsonProperty("id")]
        public string Id { get; internal set; }
        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; internal set; }
    }
}
