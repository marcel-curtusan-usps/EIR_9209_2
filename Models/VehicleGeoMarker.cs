using Newtonsoft.Json;

namespace EIR_9209_2.Models
{
    public class VehicleGeoMarker
    {
        public string Type { get; set; } = "Feature";
        public VehicleMarkerGeometry Geometry { get; set; } = new VehicleMarkerGeometry();
        public Vehicle Properties { get; set; } = new Vehicle();
        public class VehicleMarkerGeometry
        {
            public string Type { get; set; } = "Point";
            public List<double> Coordinates { get; set; } = [0, 0];

        }
    }
    public class Vehicle
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Color { get; set; } = "";
        public long posAge { get; set; } = 0;
        public VehicleStatus Vehicle_Status_Data { get; set; } = new VehicleStatus();
        public Mission? Mission { get; set; } = new Mission();
        public string? FloorId { get; set; }
        public bool Visible { get; set; } = false;
        public DateTime ServerTS_txt { get; set; } = DateTime.MinValue;
        public long ServerTS { get; set; } = 0;
        public bool IsPosition { get; set; } 
        public string LocationMovementStatus { get; set; } = "";
        public string NotificationId { get; set; } = "";
    }
    public class Mission
    {
        public string RequestId { get; set; } = "";
        public string Vehicle { get; set; } = "";
        public int VehicleNumber { get; set; } = 0;
        public string NASSCode { get; set; } = "";
        public string PickupLocation { get; set; } = "";
        public string DropOffLocation { get; set; } = "";
        public string EndLocation { get; set; } = "";
        public string Door { get; set; } = "";
        public string ETA { get; set; } = "";
        public string Placard { get; set; } = "";
        public string QueuePosition { get; set; } = "";
        public string State { get; set; } = "";
        public string MissionType { get; set; } = "";
        public DateTime MissionRequestTime { get; set; }
        public DateTime MissionAssignedTime { get; set; }
        public DateTime MissionPickupTime { get; set; }
        public DateTime MissionDropOffTime { get; set; }
        public string ErrorDescription { get; set; } = "";
        public DateTime MissionErrorTime { get; set; }
    }
    public class VehicleStatus
    {
        [JsonProperty("OBJECT_TYPE")]
        public string OBJECT_TYPE { get; set; }

        [JsonProperty("MESSAGE")]
        public string MESSAGE { get; set; } = "";

        [JsonProperty("NASS_CODE")]
        public string NASS_CODE { get; set; } = "";

        [JsonProperty("VEHICLE")]
        public string VEHICLE { get; set; } = "";

        [JsonProperty("VEHICLE_MAC_ADDRESS")]
        public string VEHICLE_MAC_ADDRESS { get; set; } = "";

        [JsonProperty("VEHICLE_NUMBER")]
        public int VEHICLE_NUMBER { get; set; }

        [JsonProperty("STATE")]
        public string STATE { get; set; } = "";

        [JsonProperty("ETA")]
        public string ETA { get; set; } = "";

        [JsonProperty("BATTERYPERCENT")]
        public string BATTERYPERCENT { get; set; } = "";

        [JsonProperty("CATEGORY")]
        public int CATEGORY { get; set; }

        [JsonProperty("X_LOCATION")]
        public string X_LOCATION { get; set; } = "";

        [JsonProperty("Y_LOCATION")]
        public string Y_LOCATION { get; set; } = "";

        [JsonProperty("ERRORCODE")]
        public string ERRORCODE { get; set; } = "";

        [JsonProperty("ERRORCODE_DISCRIPTION")]
        public string ERRORCODE_DISCRIPTION { get; set; }

        [JsonProperty("TIME")]
        public DateTime TIME { get; set; }
    }
}
