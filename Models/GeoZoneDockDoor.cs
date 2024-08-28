using Microsoft.AspNetCore.Mvc.Diagnostics;
using Newtonsoft.Json;
using Serilog;
using System.ComponentModel;

namespace EIR_9209_2.Models
{
    public class GeoZoneDockDoor
    {
        public string Type { get; set; } = "Feature";

        public Geometry Geometry { get; set; } = new Geometry();

        public DockDoorProperties Properties { get; set; } = new DockDoorProperties();
    }
    public class DockDoorProperties
    {
        public string Id { get; set; } = "";
        public string FloorId { get; set; } = "";
        public bool Visible { get; set; } = false;
        public string Color { get; set; } = "";
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string DoorNumber { get; set; } = "";
        public List<RouteTrips> RouteTrips { get; set; } = [];

       
    }
    public class RouteTrips
    {
        public int RouteTripId { get; set; } = 0;
        public int RouteTripLegId { get; set; } = 0;
        public string Route { get; set; } = "";
        public string Trip { get; set; } = "";
        public string TripDirectionInd { get; set; } = "";
        public int LegNumber { get; set; }
        public string ServiceTypeCode { get; set; } = "";
        public string LegSiteId { get; set; } = "";
        public string LegSiteName { get; set; } = "";
        public string TripSiteId { get; set; } = "";
        public string TripSiteName { get; set; } = "";
        public EventDtm ScheduledDtm { get; set; } = new EventDtm();
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
        public EventDtm LegScheduledDtm { get; set; } = new EventDtm();
        public IEnumerable<Container> Containers { get; set; } = new List<Container>();
        public int NotloadedContainers { get; set; } = 0;
        public string Form5397Ind { get; set; } = "";
        public string OriginAreaName { get; set; } = "";
        public string OriginDistrictName { get; set; } = "";
        public string OriginSiteName { get; set; } = "";
        public string OriginSiteId { get; set; } = "";
        public string DestAreaName { get; set; } = "";
        public string DestDistrictName { get; set; } = "";
        public string DestSiteName { get; set; } = "";
        public string DestSiteId { get; set; } = "";
        public int TourNumber { get; set; } = 0;
        public string Supplier { get; set; } = "";
        public string NotUnloadedInd { get; set; } = "";
        public EventDtm OperDate { get; set; } = new EventDtm();
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

        public string InitialOriginSiteId { get; set; } = "";
        public string InitialOriginSiteName { get; set; } = "";
        public string FinalDestSiteId { get; set; } = "";
        public string FinalDestSiteName { get; set; } = "";
        public string IsAODU { get; set; } = "";
        public string Status { get; set; } = "ACTIVE";
        public string State { get; set; } = "";
        public string NotificationId { get; set; } = "";
        public string LegStatus { get; set; } = "";
        public string TrailerBarcode { get; set; } = "";
        public EventDtm ActualDtm { get; set; } = new EventDtm();
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
        public EventDtm LegActualDtm { get; set; } = new EventDtm();
        public string DriverFirstName { get; set; } = "";
        public string DriverLastName { get; set; } = "";
        public string DriverPhoneNumber { get; set; } = "";
        public string DriverBarcode { get; set; } = "";
        public int? DriverId { get; set; } = 0;
        public string DoorId { get; set; } = "";
        public string DoorNumber { get; set; } = "";
        public bool AtDoor { get; set; }
        public string VanNumber { get; set; } = "";
        public string TrailerLengthCode { get; set; } = "";
        public int? LoadPercent { get; set; } = 0;
        public EventDtm LoadUnldStartDtm { get; set; } = new EventDtm();
        public EventDtm LoadUnldEndDtm { get; set; } = new EventDtm();
        public EventDtm DoorDtm { get; set; } = new EventDtm();
        public EventDtm LegDoorDtm { get; set; } = new EventDtm();
        public string MspBarcode { get; set; } = "";
        public string DestSites { get; set; } = "";
        public string RawData { get; set; } = "";
        public bool TripUpdate { get; set; } = false;
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

        public string TripSiteInboundStatus { get; set; } = "";
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
