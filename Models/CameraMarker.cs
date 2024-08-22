using Newtonsoft.Json;

namespace EIR_9209_2.Models
{
    public class CameraMarker
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public MarkerGeometry Geometry { get; set; } = new MarkerGeometry();
        [JsonProperty("cameraDirection")]
        public string CameraDirection { get; set; } = "";
        [JsonProperty("Camera_Data")]
        public Cameras CameraData { get; set; } = new Cameras();
        [JsonProperty("base64Image")]
        public string Base64Image { get; set; } = "";

        public class MarkerGeometry
        {
            public string Type { get; set; } = "Point";
            public List<double> Coordinates { get; set; } = new List<double>();

        }
    }
    public class Cameras
    {
        [JsonProperty("LOCALE_KEY")]
        public string LocaleKey { get; set; } = "";

        [JsonProperty("MODEL_NUM")]
        public string ModelNum { get; set; } = "";

        [JsonProperty("FACILITY_PHYS_ADDR_TXT")]
        public string FacilityPhysAddrTxt { get; set; } = "";

        [JsonProperty("GEO_PROC_REGION_NM")]
        public string GeoProcRegionNm { get; set; } = "";

        [JsonProperty("FACILITY_SUBTYPE_DESC")]
        public string FacilitySubtypeDesc { get; set; } = "";

        [JsonProperty("GEO_PROC_DIVISION_NM")]
        public string GeoProcDivisionNm { get; set; } = "";

        [JsonProperty("AUTH_KEY")]
        public string AuthKey { get; set; } = "";

        [JsonProperty("FACILITY_LATITUDE_NUM")]
        public double FacilitiyLatitudeNum { get; set; } = 0.0;

        [JsonProperty("FACILITY_LONGITUDE_NUM")]
        public double FacilitiyLongitudeNum { get; set; } = 0.0;

        [JsonProperty("CAMERA_NAME")]
        public string CameraName { get; set; } = "";

        [JsonProperty("Id")]
        public string Id { get; set; } = "";

        [JsonProperty("IP")]
        public string IP { get; set; } = "";

        [JsonProperty("HOSTNAME")]
        public string HOSTNAME { get; set; } = "";

        [JsonProperty("DESCRIPTION")]
        public string Description { get; set; } = "";

        [JsonProperty("REACHABLE")]
        public string Reachable { get; set; } = "";

        [JsonProperty("FACILITY_DISPLAY_NME")]
        public string FacilityDisplayName { get; set; } = "";
    }
}
