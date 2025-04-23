using Newtonsoft.Json;
using System.Globalization;

namespace EIR_9209_2.Models
{
    public class GeoMarker
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "Feature";
        [JsonProperty("geometry")]
        public MarkerGeometry Geometry { get; set; } = new MarkerGeometry();
        [JsonProperty("properties")]
        public Marker Properties { get; set; } = new Marker();

        public class Marker
        {
            [JsonProperty("id")]
            public string Id { get; set; } = "";
            [JsonProperty("floorId")]
            public string? FloorId { get; set; }
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
            [JsonProperty("type")]
            public string Type { get; set; } = "";
            [JsonProperty("tagUpdate")]
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
            public bool isPosition
            {
                get => Visible;
                set => Visible = value;
            }
            [JsonProperty("cardHolderId")]
            public int CardHolderId { get; set; } = 0;
            [JsonProperty("ein")]
            public string EIN { get; set; } = "";
            [JsonProperty("ldc")]
            public string LDC { get; set; } = "";
            [JsonProperty("payLocation")]
            public string PayLocation { get; set; } = "";
            private string _empFirstName = "";
            [JsonProperty("EmpFirstName")]
            public string EmpFirstName
            {
                get => _empFirstName;
                set => _empFirstName = ConvertToTitleCase(value);
            }

            private string _empLastName = "";
            [JsonProperty("empLastName")]
            public string EmpLastName
            {
                get => _empLastName;
                set => _empLastName = ConvertToTitleCase(value);
            }

            private string _title = "";
            [JsonProperty("Title")]
            public string Title
            {
                get => _title;
                set => _title = ConvertToTitleCase(value);
            }
            [JsonProperty("designationActivity")]
            public string DesignationActivity { get; set; } = "";
            [JsonProperty("empPayLocation")]
            public string EmpPayLocation { get; set; } = "";
            [JsonProperty("encodedId")]
            public string EncodedId { get; set; } = "";
            [JsonProperty("bdate")]
            public DateTime Bdate { get; set; } = DateTime.MinValue;
            [JsonProperty("blunch")]
            public string Blunch { get; set; } = "";
            [JsonProperty("elunch")]
            public string Elunch { get; set; } = "";
            [JsonProperty("edate")]
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
            [JsonProperty("zonesNames")]
            public string ZonesNames { get; set; } = "";
            [JsonProperty("locationMovementStatus")]
            public string LocationMovementStatus { get; set; } = "";
            [JsonProperty("locationType")]
            public string LocationType { get; set; } = "";
            [JsonProperty("lastSeenTS")]
            public long LastSeenTS { get; set; } = 0;
            [JsonProperty("badgeScan")]
            public List<ScanTransaction> BadgeScan { get; set; } = [];

            private string ConvertToTitleCase(string input)
            {
                if (string.IsNullOrEmpty(input))
                {
                    return input;
                }

                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
            }

        }
        public class MarkerGeometry
        {
            [JsonProperty("type")]
            public string Type { get; set; } = "Point";
            [JsonProperty("coordinates")]
            public List<double> Coordinates { get; set; } = [0, 0];

        }
    }
}
