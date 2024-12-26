namespace EIR_9209_2.Models
{
    public class RawRings
    {
        public string rawRingSeqNo { get; set; } = "";
        public EmpInfo EmpInfo { get; set; } = new EmpInfo();
        public TranInfo TranInfo { get; set; } = new TranInfo();
        public RingInfo RingInfo { get; set; } = new RingInfo();
        public InputInfo InputInfo { get; set; } = new InputInfo();
        public DeviceInfo DeviceInfo { get; set; } = new DeviceInfo();
        public string sourceId { get; set; } = "";
        public string sourceTranId { get; set; } = "";
        public string statusCode { get; set; } = "";
        public string errorCode { get; set; } = "";
    }

    public class EmpInfo
    {
        public string EmpId { get; set; } = "";
        public string EmpIdType { get; set; } = "";
    }

    public class TranInfo
    {
        public string TranCode { get; set; } = "";
        public string RingReasonCode { get; set; } = "";
        public string TranDate { get; set; } = "";
        public string TranTime { get; set; } = "";
        public string TimeZoneCode { get; set; } = "";
        public string UtcOffset { get; set; } = "";
        public bool? DstObserved { get; set; } 
        public string RingTypeCode { get; set; } = "";
        public string FinanceNo { get; set; } = "";
    }

    public class RingInfo
    {
        public string FinanceNoUnitId { get; set; } = "";
        public string Rsc { get; set; } = "";
        public string RscSuffix { get; set; } = "";
        public string OperationId { get; set; } = "";
        public string LocalUnitNo { get; set; } = "";
        public string RouteNo { get; set; } = "";
        public string ActivityDurationQty { get; set; } = "";
        public bool? ScheduledInd { get; set; }
        public string PositionLevelNo { get; set; } = "";
        public string FacilityId { get; set; } = "";
        public string VehicleId { get; set; } = "";
    }

    public class InputInfo
    {
        public string InputId { get; set; } = "";
        public string InputIdType { get; set; } = "";
        public string InputDate { get; set; } = "";
        public string InputTime { get; set; } = "";
    }

    public class DeviceInfo
    {
        public string DeviceId { get; set; } = "";
        public string DeviceType { get; set; } = "";
        public string Latitude { get; set; } = "";
        public string Longitude { get; set; } = "";
    }
}
