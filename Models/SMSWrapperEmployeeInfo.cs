using Newtonsoft.Json;

namespace EIR_9209_2.Models
{
    public class SMSWrapperEmployeeInfo
    {
        [JsonProperty("NASSCode")]
        public string? NASSCode { get; set; }

        [JsonProperty("cardholderId")]
        public int CardholderId { get; set; }

        [JsonProperty("tagId")]
        public string? TagId { get; set; }

        [JsonProperty("ein")]
        public string? Ein { get; set; }

        [JsonProperty("encodedId")]
        public string? EncodedId { get; set; }

        [JsonProperty("firstName")]
        public string? FirstName { get; set; }

        [JsonProperty("lastName")]
        public string? LastName { get; set; }

        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("designationActivity")]
        public string? DesignationActivity { get; set; }

        [JsonProperty("payLocation")]
        public string? PayLocation { get; set; }

        [JsonProperty("scanTime")]
        public DateTime? ScanTime { get; set; }
    }
}
