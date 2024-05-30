using Newtonsoft.Json;
using System.Text.Json;

namespace EIR_9209_2.Models
{
    public class GeoZone
    {
        public string Type { get; set; } = "Feature";

        public Geometry Geometry { get; set; } = new Geometry();

        public Properties Properties { get; set; } = new Properties();
    }

    public class Properties
    {
        public string Id { get; set; } = "";
        public string FloorId { get; set; } = "";
        public bool Visible { get; set; } = false;
        public string Color { get; set; } = "";
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string ZoneType { get; set; } = "";
        public string MpeType { get; set; } = "";
        public MPERunPerformance? MPERunPerformance { get; set; } = new();
        public string? DataSource { get; set; } = "";
        public string? Emails { get; set; } = "";
    }
    public class DockDoorProperties
    {
        public string Id { get; set; } = "";
        public string FloorId { get; set; } = "";
        public bool Visible { get; set; } = false;
        public string Color { get; set; } = "";
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string ZoneType { get; set; } = "";
        public string MpeType { get; set; } = "";

        public string? DataSource { get; set; } = "";
        public string? Emails { get; set; } = "";
    }

    public class Geometry
    {
        public string Type { get; set; } = "Polygon";

        public List<List<List<double>>> Coordinates { get; set; } = new List<List<List<double>>>();
    }
}
