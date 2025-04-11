using Newtonsoft.Json;

namespace EIR_9209_2.Models
{
    /// <summary>
    /// Represents employee information for the SMS Wrapper.
    /// </summary>
    public class SMSWrapperEmployeeInfo
    {
        /// <summary>
        /// Gets or sets the NASS code.
        /// </summary>
        [JsonProperty("NASSCode")]
        public string NASSCode { get; set; } = "";

        /// <summary>
        /// Gets or sets the cardholder ID.
        /// </summary>
        [JsonProperty("cardholderId")]
        public int CardholderId { get; set; }

        /// <summary>
        /// Gets or sets the tag ID.
        /// </summary>
        [JsonProperty("tagId")]
        public string TagId { get; set; } = "";

        /// <summary>
        /// Gets or sets the EIN (Employer Identification Number).
        /// </summary>
        [JsonProperty("ein")]
        public string Ein { get; set; } = "";

        /// <summary>
        /// Gets or sets the encoded ID.
        /// </summary>
        [JsonProperty("encodedId")]
        public string EncodedId { get; set; } = "";

        /// <summary>
        /// Gets or sets the first name of the employee.
        /// </summary>
        [JsonProperty("firstName")]
        public string FirstName { get; set; } = "";

        /// <summary>
        /// Gets or sets the last name of the employee.
        /// </summary>
        [JsonProperty("lastName")]
        public string LastName { get; set; } = "";

        /// <summary>
        /// Gets or sets the title of the employee.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; } = "";

        /// <summary>
        /// Gets or sets the designation activity of the employee.
        /// </summary>
        [JsonProperty("designationActivity")]
        public string DesignationActivity { get; set; } = "";

        /// <summary>
        /// Gets or sets the pay location of the employee.
        /// </summary>
        [JsonProperty("payLocation")]
        public string PayLocation { get; set; } = "";

        /// <summary>
        /// Gets or sets the scan time.
        /// </summary>
        [JsonProperty("scanTime")]
        public DateTime ScanTime { get; set; } = DateTime.MinValue;
    }
}
