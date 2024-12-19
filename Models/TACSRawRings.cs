namespace EIR_9209_2.Models
{
    public class RawRings
    {
        public required string rawRingSeqNo { get; set; }
        public required EmpInfo EmpInfo { get; set; }
        public required TranInfo TranInfo { get; set; }
        public RingInfo RingInfo { get; set; }
        public InputInfo InputInfo { get; set; }
        public DeviceInfo DeviceInfo { get; set; }
        public required string sourceId { get; set; }
        public required string sourceTranId { get; set; }
        public required string statusCode { get; set; }
        public string errorCode { get; set; }
    }

    public class EmpInfo
    {
        public required string EmpId { get; set; }
        public required string EmpIdType { get; set; }
    }

    public class TranInfo
    {
        public string TranCode { get; set; }
        public required string RingReasonCode { get; set; }
        public required string TranDate { get; set; }
        public required string TranTime { get; set; }
        public required string TimeZoneCode { get; set; }
        public string UtcOffset { get; set; }
        public bool? DstObserved { get; set; }
        public required string RingTypeCode { get; set; }
        public required string FinanceNo { get; set; }
    }

    public class RingInfo
    {
        public string FinanceNoUnitId { get; set; }
        public string Rsc { get; set; }
        public string RscSuffix { get; set; }
        public string OperationId { get; set; }
        public string LocalUnitNo { get; set; }
        public string RouteNo { get; set; }
        public string ActivityDurationQty { get; set; }
        public bool? ScheduledInd { get; set; }
        public string PositionLevelNo { get; set; }
        public string FacilityId { get; set; }
        public string VehicleId { get; set; }
    }

    public class InputInfo
    {
        public required string InputId { get; set; }
        public required string InputIdType { get; set; }
        public required string InputDate { get; set; }
        public required string InputTime { get; set; }
    }

    public class DeviceInfo
    {
        public required string DeviceId { get; set; }
        public required string DeviceType { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
