
using Newtonsoft.Json;
using System.Reflection;

namespace EIR_9209_2.Models
{
    public class GeoMarker
    {
        public string Type { get; set; } = "Feature";
        public MarkerGeometry Geometry { get; set; } = new MarkerGeometry();
        public Marker Properties { get; set; } = new Marker();

        public class Marker
        {
            public string Id { get; set; } = "";
            public string FloorId { get; set; }
            public bool Visible { get; set; } = false;
            public List<string> Zones { get; set; } = new List<string>();
            public string Color { get; set; } = "";
            public long posAge { get; set; } = 0;
            public string Name { get; set; } = "";
            public string CraftName { get; set; } = "N/A";
            public DateTime PositionTS_txt { get; set; } = DateTime.MinValue;
            public long PositionTS { get; set; } = 0;
            public string TagType { get; set; } = "";
            public bool TagUpdate { get; set; }
            public DateTime ServerTS_txt { get; set; } = DateTime.MinValue;
            public long ServerTS { get; set; } = 0;
            public bool isSch { get; set; }
            public bool isTacs { get; set; }
            public bool isePacs { get; set; }
            public bool isPosition { get; set; }
            public int CardHolderId { get; set; } = 0;
            public string EIN { get; set; } = "";
            public string DesignationActivity { get; set; } = "";
            public string LDC { get; set; } = "";
            public string PayLocation { get; set; } = "";
            public string EmpFirstName { get; set; } = "";
            public string EmpLastName { get; set; } = "";
            public string EmpTitle { get; set; } = "";
            public string EmpDesignationActivity { get; set; } = "";
            public string EmpPayLocation { get; set; } = "";
            public string EncodedID { get; set; } = "";
            public DateTime Bdate { get; set; } = DateTime.MinValue;
            public string Blunch { get; set; } = "";
            public string Elunch { get; set; } = "";
            public DateTime Edate { get; set; } = DateTime.MinValue;
            public string TourNumber { get; set; } = "";
            public DateTime ReqDate { get; set; } = DateTime.MinValue;
            public string DaysOff { get; set; } = "";
            public string Source { get; set; } = "";
            public string NotificationId { get; set; } = "";
            public string ZonesNames { get; set; } = "";
            public string LocationMovementStatus { get; set; } = "";
            public string LocationType { get; set; } = "";
            public long LastSeenTS { get; set; } = 0;
            public List<ScanTransaction> BadgeScan { get; set; } = new();
        }
        public class MarkerGeometry
        {
            public string Type { get; set; } = "Point";
            public List<double> Coordinates { get; set; }
        }
    }
}
