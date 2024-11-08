using Newtonsoft.Json;

namespace EIR_9209_2.Models
{
    public class MPERunPerformance
    {
        [JsonProperty("id")]
        public string ZoneId { get; set; } = "";

        [JsonProperty("mpeType")]
        public string MpeType { get; set; } = "";

        [JsonProperty("mpeNumber")]
        public string MpeNumber { get; set; } = "";

        [JsonProperty("bins")]
        public string Bins { get; set; } = "";

        [JsonProperty("curSortplan")]
        public string CurSortplan { get; set; } = "";

        [JsonProperty("curThruputOphr")]
        public string CurThruputOphr { get; set; } = "";

        [JsonProperty("totSortplanVol")]
        public string TotSortplanVol { get; set; } = "";

        [JsonProperty("rpgEstVol")]
        public string RpgEstVol { get; set; } = "";

        [JsonProperty("actVolPlanVolNbr")]
        public string ActVolPlanVolNbr { get; set; } = "";

        [JsonProperty("currentRunStart")]
        public string CurrentRunStart { get; set; } = "";

        [JsonProperty("currentRunEnd")]
        public string CurrentRunEnd { get; set; } = "";

        [JsonProperty("curOperationId")]
        public string CurOperationId { get; set; } = "";

        [JsonProperty("binFullStatus")]
        public string BinFullStatus { get; set; } = "";

        [JsonProperty("binFullBins")]
        public string BinFullBins { get; set; } = "";

        [JsonProperty("throughputStatus")]
        public string ThroughputStatus { get; set; } = "";

        [JsonProperty("unplanMaintSpStatus")]
        public string UnplanMaintSpStatus { get; set; } = "";

        [JsonProperty("opStartedLateStatus")]
        public string OpStartedLateStatus { get; set; } = "";

        [JsonProperty("opRunningLateStatus")]
        public string OpRunningLateStatus { get; set; } = "";

        [JsonProperty("sortplanWrongStatus")]
        public string SortplanWrongStatus { get; set; } = "";

        [JsonProperty("unplanMaintSpTimer")]
        public string UnplanMaintSpTimer { get; set; } = "";

        [JsonProperty("opStartedLateTimer")]
        public string OpStartedLateTimer { get; set; } = "";

        [JsonProperty("opRunningLateTimer")]
        public string OpRunningLateTimer { get; set; } = "";

        [JsonProperty("rPGStartDtm")]
        public string RPGStartDtm { get; set; } = "";

        [JsonProperty("rPGEndDtm")]
        public string RPGEndDtm { get; set; } = "";

        [JsonProperty("expectedThroughput")]
        public string ExpectedThroughput { get; set; } = "";

        [JsonProperty("sortplanWrongTimer")]
        public string SortplanWrongTimer { get; set; } = "";

        [JsonProperty("rpgEstCompTime")]
        public string RpgEstCompTime { get; set; } = "";
        [JsonProperty("rpgEstimatedCompletion")]
        public DateTime RpgEstimatedCompletion { get; set; } = DateTime.MinValue;
        [JsonProperty("hourlyData")]
        public List<HourlyData> HourlyData { get; set; } = new List<HourlyData>();
        [JsonProperty("rpgExpectedThruput")]
        public string RpgExpectedThruput { get; set; } = "";
        [JsonProperty("arsRecrej3")]
        public string ArsRecrej3 { get; set; } = "";
        [JsonProperty("sweepRecrej3")]
        public string SweepRecrej3 { get; set; } = "";
        [JsonProperty("mpeId")]
        public string MpeId { get; set; } = "";
        [JsonProperty("scheduledStaffing")]
        public Staff ScheduledStaffing { get; set; } = new();
        [JsonProperty("actualStaffing")]
        public Staff ActualStaffing { get; set; } = new();
        [JsonProperty("mPEGroup")]
        public string MPEGroup { get; set; } = "";
        [JsonProperty("dataSource")]
        public string DataSource { get; set; } = "";
    }
    public class HourlyData
    {
        [JsonProperty("hour")]
        public string Hour { get; set; } = "";
        [JsonProperty("count")]
        public int Count { get; set; } = 0;
        [JsonProperty("sorted")]
        public int Sorted { get; set; } = 0;
        [JsonProperty("rejected")]
        public int Rejected { get; set; } = 0;
    }
    public class TargetHourlyData
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "";
        [JsonProperty("mpeType")]
        public string MpeType { get; set; } = "";
        [JsonProperty("mpeNumber")]
        public string MpeNumber { get; set; } = "";
        [JsonProperty("mpeId")]
        public string MpeId { get; set; } = "";
        [JsonProperty("targetHour")]
        public string TargetHour { get; set; } = "";
        [JsonProperty("hourlyTargetVol")]
        public int HourlyTargetVol { get; set; } = 0;
        [JsonProperty("hourlyRejectRatePercent")]
        public double HourlyRejectRatePercent { get; set; } = 0;
    }
    public class MpeWatchRequestId
    {
        public string id { get; set; } = "";
    }
}
