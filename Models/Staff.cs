using Newtonsoft.Json;

namespace EIR_9209_2.Models
{
    public class Staff
    {
        [JsonProperty("mach_type")]
        public string MachType { get; set; } = "";

        [JsonProperty("machine_no")]
        public int MachineNo { get; set; } = 0;

        [JsonProperty("sortplan")]
        public string Sortplan { get; set; } = "";

        [JsonProperty("clerk")]
        public double Clerk { get; set; } = 0.0;

        [JsonProperty("mh")]
        public double Mh { get; set; } = 0.0;
        [JsonProperty("id")]
        public string Id
        {
            get
            {
                return string.Concat(MachType.ToString(), MachineNo.ToString(), Sortplan);
            }
            set
            {
                return;
            }
        }
    }
}