
using Newtonsoft.Json;

namespace EIR_9209_2.Models
{
    public class InventoryTracking
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "";
        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }= DateTime.MinValue;
        [JsonProperty("checkOutDate")]
        public DateTime CheckOutDate { get; set; } = DateTime.MinValue;
        [JsonProperty("checkInDate")]
        public DateTime CheckInDate { get; set; } = DateTime.MinValue;
        [JsonProperty("ein")]
        public string EIN { get; set; } = "";
        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; } = "";
        [JsonProperty("tourNumber")]
        public string TourNumber { get; set; } = "";
        [JsonProperty("isCheckedOut")]
        public bool IsCheckedOut { get; set; }
    }
}