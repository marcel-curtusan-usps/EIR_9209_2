using Microsoft.AspNetCore.Mvc.Diagnostics;
using Newtonsoft.Json;
using Serilog;
using System.ComponentModel;

namespace EIR_9209_2.Models
{
    public class GeoZoneDockDoor
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "Feature";
        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; } = new Geometry();
        [JsonProperty("properties")]
        public DockDoorProperties Properties { get; set; } = new DockDoorProperties();
    }
    public class DockDoorProperties
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "";
        [JsonProperty("floorId")]
        public string FloorId { get; set; } = "";
        [JsonProperty("visible")]
        public bool Visible { get; set; } = false;
        [JsonProperty("color")]
        public string Color { get; set; } = "";
        [JsonProperty("name")]
        public string Name { get; set; } = "";
        [JsonProperty("type")]
        public string Type { get; set; } = "";
        [JsonProperty("doorNumber")]
        public string DoorNumber { get; set; } = "";
        [JsonProperty("routeTripId")]
        public int RouteTripId { get; set; } = 0;
        [JsonProperty("routeTripLegId")]
        public int RouteTripLegId { get; set; } = 0;
        [JsonProperty("tripDirectionInd")]
        public string TripDirectionInd { get; set; } = "";
        [JsonProperty("legSiteName")]
        public string LegSiteName { get; set; } = "";
        [JsonProperty("legSiteId")]
        public string LegSiteId { get; set; } = "";
        [JsonProperty("status")]
        public string Status { get; set; } = "";
        [JsonProperty("tripMin")]
        public int TripMin { get; set; } = 0;
        [JsonProperty("containersNotLoaded")]
        public int ContainersNotLoaded { get; set; } = 0;
        [JsonProperty("scheduledDtm")]
        public DateTime ScheduledDtm { get; set; } = DateTime.MinValue;
        [JsonProperty("route")]
        public string Route { get; set; } = "";
        [JsonProperty("trip")]
        public string Trip { get; set; } = "";
        [JsonProperty("isTripAtDoor")]
        public bool IsTripAtDoor { get; set; }
        [JsonProperty("routeTrips")]
        public List<RouteTrips> RouteTrips { get; set; } = [];

       
    }
    public class RouteTrips
    {
        [JsonProperty("routeTripId")]
        public int RouteTripId { get; set; } = 0;
        [JsonProperty("routeTripLegId")]
        public int RouteTripLegId { get; set; } = 0;
        [JsonProperty("route")]
        public string Route { get; set; } = "";
        [JsonProperty("trip")]
        public string Trip { get; set; } = "";
        [JsonProperty("tripDirectionInd")]
        public string TripDirectionInd { get; set; } = "";
        [JsonProperty("legNumber")]
        public int LegNumber { get; set; }
        [JsonProperty("serviceTypeCode")]
        public string ServiceTypeCode { get; set; } = "";
        [JsonProperty("legSiteId")]
        public string LegSiteId { get; set; } = "";
        [JsonProperty("legSiteName")]
        public string LegSiteName { get; set; } = "";
        [JsonProperty("tripSiteId")]
        public string TripSiteId { get; set; } = "";
        [JsonProperty("tripSiteName")]
        public string TripSiteName { get; set; } = "";
        [JsonProperty("scheduledDtm")]
        public EventDtm ScheduledDtm { get; set; } = new EventDtm();
        [JsonProperty("scheduledDtmfmt")]
        public DateTime ScheduledDtmfmt
        {
            get
            {
                if (ActualDtm.Year == 0)
                {
                    return DateTime.MinValue;
                }
                return new DateTime(ScheduledDtm.Year, (ScheduledDtm.Month + 1), ScheduledDtm.DayOfMonth, ScheduledDtm.HourOfDay, ScheduledDtm.Minute, ScheduledDtm.Second);
            }
            set { return; }
        }
        [JsonProperty("legScheduledDtm")]
        public EventDtm LegScheduledDtm { get; set; } = new EventDtm();
        [JsonProperty("containers")]
        public IEnumerable<Container> Containers { get; set; } = new List<Container>();
        [JsonProperty("notloadedContainers")]
        public int NotloadedContainers { get; set; } = 0;
        [JsonProperty("form5397Ind")]
        public string Form5397Ind { get; set; } = "";
        [JsonProperty("originAreaName")]
        public string OriginAreaName { get; set; } = "";
        [JsonProperty("originDistrictName")]
        public string OriginDistrictName { get; set; } = "";
        [JsonProperty("originSiteName")]
        public string OriginSiteName { get; set; } = "";
        [JsonProperty("originSiteId")]
        public string OriginSiteId { get; set; } = "";
        [JsonProperty("destAreaName")]
        public string DestAreaName { get; set; } = "";
        [JsonProperty("destDistrictName")]
        public string DestDistrictName { get; set; } = "";
        [JsonProperty("destSiteName")]
        public string DestSiteName { get; set; } = "";
        [JsonProperty("destSiteId")]
        public string DestSiteId { get; set; } = "";
        [JsonProperty("tourNumber")]
        public int TourNumber { get; set; } = 0;
        [JsonProperty("supplier")]
        public string Supplier { get; set; } = "";
        [JsonProperty("notUnloadedInd")]
        public string NotUnloadedInd { get; set; } = "";
        [JsonProperty("operDate")]
        public EventDtm OperDate { get; set; } = new EventDtm();
        [JsonProperty("operDatefmt")]
        public DateTime OperDatefmt
        {
            get
            {
                if (ActualDtm.Year == 0)
                {
                    return DateTime.MinValue;
                }
                return new DateTime(OperDate.Year, (OperDate.Month + 1), OperDate.DayOfMonth, OperDate.HourOfDay, OperDate.Minute, OperDate.Second, DateTimeKind.Unspecified);
            }
            set { return; }
        }
        [JsonProperty("initialOriginSiteId")]
        public string InitialOriginSiteId { get; set; } = "";
        [JsonProperty("initialOriginSiteName")]
        public string InitialOriginSiteName { get; set; } = "";
        [JsonProperty("finalDestSiteId")]
        public string FinalDestSiteId { get; set; } = "";
        [JsonProperty("finalDestSiteName")]
        public string FinalDestSiteName { get; set; } = "";
        [JsonProperty("isAODU")]
        public string IsAODU { get; set; } = "";
        [JsonProperty("status")]
        public string Status { get; set; } = "ACTIVE";
        [JsonProperty("state")]
        public string State { get; set; } = "";
        [JsonProperty("notificationId")]
        public string NotificationId { get; set; } = "";
        [JsonProperty("legStatus")]
        public string LegStatus { get; set; } = "";
        [JsonProperty("trailerBarcode")]
        public string TrailerBarcode { get; set; } = "";
        [JsonProperty("actualDtm")]
        public EventDtm ActualDtm { get; set; } = new EventDtm();
        [JsonProperty("actualDtmfmt")]
        public DateTime ActualDtmfmt
        {
            get
            {
                if (ActualDtm.Year == 0)
                {
                    return DateTime.MinValue;
                }
                return new DateTime(ActualDtm.Year, (ActualDtm.Month + 1), ActualDtm.DayOfMonth, ActualDtm.HourOfDay, ActualDtm.Minute, ActualDtm.Second);
            }
            set { return; }
        }
        [JsonProperty("legActualDtm")]
        public EventDtm LegActualDtm { get; set; } = new EventDtm();
        [JsonProperty("driverFirstName")]
        public string DriverFirstName { get; set; } = "";
        [JsonProperty("driverLastName")]
        public string DriverLastName { get; set; } = "";
        [JsonProperty("driverPhoneNumber")]
        public string DriverPhoneNumber { get; set; } = "";
        [JsonProperty("driverBarcode")]
        public string DriverBarcode { get; set; } = "";
        [JsonProperty("driverId")]
        public int? DriverId { get; set; } = 0;
        [JsonProperty("doorId")]
        public string DoorId { get; set; } = "";
        [JsonProperty("doorNumber")]
        public string DoorNumber { get; set; } = "";
        [JsonProperty("atDoor")]
        public bool AtDoor { get; set; }
        [JsonProperty("vanNumber")]
        public string VanNumber { get; set; } = "";
        [JsonProperty("trailerLengthCode")]
        public string TrailerLengthCode { get; set; } = "";
        [JsonProperty("loadPercent")]
        public int? LoadPercent { get; set; } = 0;
        [JsonProperty("loadUnldStartDtm")]
        public EventDtm LoadUnldStartDtm { get; set; } = new EventDtm();
        [JsonProperty("loadUnldEndDtm")]
        public EventDtm LoadUnldEndDtm { get; set; } = new EventDtm();
        [JsonProperty("doorDtm")]
        public EventDtm DoorDtm { get; set; } = new EventDtm();
        [JsonProperty("legDoorDtm")]
        public EventDtm LegDoorDtm { get; set; } = new EventDtm();
        [JsonProperty("mspBarcode")]
        public string MspBarcode { get; set; } = "";
        [JsonProperty("destSites")]
        public string DestSites { get; set; } = "";
        [JsonProperty("rawData")]
        public string RawData { get; set; } = "";
        [JsonProperty("tripUpdate")]
        public bool TripUpdate { get; set; } = false;
        [JsonProperty("id")]
        public string Id
        {
            get
            {
                return string.Concat(RouteTripId.ToString(), RouteTripLegId.ToString(), TripDirectionInd);
            }
            set
            {
                return;
            }
        }
        [JsonProperty("tripSiteInboundStatus")]
        public string TripSiteInboundStatus { get; set; } = "";
        [JsonProperty("tripSiteOutboundStatus")]
        public string TripSiteOutboundStatus { get; set; } = "";
    }


    public class EventDtm
    {
        public int Year { get; set; } = 1;
        public int Month { get; set; } = 1;
        public int DayOfMonth { get; set; } = 1;
        public int HourOfDay { get; set; } = 1;
        public int Minute { get; set; } = 0;
        public int Second { get; set; } = 0;
    }
}
