namespace EIR_9209_2.Models
{
    public class CameraGeoMarker
    {
        public string Type { get; set; } = "Feature";
        public MarkerGeometry Geometry { get; set; } = new MarkerGeometry();
        public Cameras Properties { get; set; } = new Cameras();
        public class MarkerGeometry
        {
            public string Type { get; set; } = "Point";
            public List<double> Coordinates { get; set; } = new List<double>();

        }
    }
    public class Cameras
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string CameraDirection { get; set; } = "";
        public string Base64Image { get; set; } = "";
        public string LocaleKey { get; set; } = "";
        public string ModelNum { get; set; } = "";
        public string FacilityPhysAddrTxt { get; set; } = "";
        public string GeoProcRegionNm { get; set; } = "";
        public string FacilitySubtypeDesc { get; set; } = "";
        public string GeoProcDivisionNm { get; set; } = "";
        public string AuthKey { get; set; } = "";
        public double FacilityLatitudeNum { get; set; } = 0.0;
        public double FacilityLongitudeNum { get; set; } = 0.0;
        public string CameraName { get; set; } = "";
        public string IP { get; set; } = "";
        public string HostName { get; set; } = "";
        public string Description { get; set; } = "";
        public string Reachable { get; set; } = "";
        public string FacilityDisplayName { get; set; } = "";
        public string Type { get; set; } = "";
        public string FloorId { get; set; } = "";
        public bool Visible { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.MinValue;
    }
}
