using Newtonsoft.Json;

namespace EIR_9209_2.Models
{
    public class MPERunPerformance
    {
        [JsonProperty("mpe_type")]
        public string MpeType { get; set; } = "";

        [JsonProperty("mpe_number")]
        public string MpeNumber { get; set; } = "";

        [JsonProperty("bins")]
        public string Bins { get; set; } = "";

        [JsonProperty("cur_sortplan")]
        public string CurSortplan { get; set; } = "";

        [JsonProperty("cur_thruput_ophr")]
        public string CurThruputOphr { get; set; } = "";

        [JsonProperty("tot_sortplan_vol")]
        public string TotSortplanVol { get; set; } = "";

        [JsonProperty("rpg_est_vol")]
        public string RpgEstVol { get; set; } = "";

        [JsonProperty("act_vol_plan_vol_nbr")]
        public string ActVolPlanVolNbr { get; set; } = "";

        [JsonProperty("current_run_start")]
        public string CurrentRunStart { get; set; } = "";

        [JsonProperty("current_run_end")]
        public string CurrentRunEnd { get; set; } = "";

        [JsonProperty("cur_operation_id")]
        public string CurOperationId { get; set; } = "";

        [JsonProperty("bin_full_status")]
        public string BinFullStatus { get; set; } = "";

        [JsonProperty("bin_full_bins")]
        public string BinFullBins { get; set; } = "";

        [JsonProperty("throughput_status")]
        public string ThroughputStatus { get; set; } = "";

        [JsonProperty("unplan_maint_sp_status")]
        public string UnplanMaintSpStatus { get; set; } = "";

        [JsonProperty("op_started_late_status")]
        public string OpStartedLateStatus { get; set; } = "";

        [JsonProperty("op_running_late_status")]
        public string OpRunningLateStatus { get; set; } = "";

        [JsonProperty("sortplan_wrong_status")]
        public string SortplanWrongStatus { get; set; } = "";

        [JsonProperty("unplan_maint_sp_timer")]
        public string UnplanMaintSpTimer { get; set; } = "";

        [JsonProperty("op_started_late_timer")]
        public string OpStartedLateTimer { get; set; } = "";

        [JsonProperty("op_running_late_timer")]
        public string OpRunningLateTimer { get; set; } = "";

        [JsonProperty("rpg_start_dtm")]
        public string RPGStartDtm { get; set; } = "";

        [JsonProperty("rpg_end_dtm")]
        public string RPGEndDtm { get; set; } = "";

        [JsonProperty("expected_throughput")]
        public string ExpectedThroughput { get; set; } = "";

        [JsonProperty("sortplan_wrong_timer")]
        public string SortplanWrongTimer { get; set; } = "";

        [JsonProperty("rpg_est_comp_time")]
        public string RpgEstCompTime { get; set; } = "";

        [JsonProperty("hourly_data")]
        public List<HourlyData> HourlyData { get; set; } = new List<HourlyData>();

        [JsonProperty("rpg_expected_thruput")]
        public string RpgExpectedThruput { get; set; } = "";

        [JsonProperty("ars_recrej3")]
        public string ArsRecrej3 { get; set; } = "";

        [JsonProperty("sweep_recrej3")]
        public string SweepRecrej3 { get; set; } = "";

        public string MpeId { get; set; } = "";

        [JsonProperty("scheduled_staff")]
        public Staff ScheduledStaffing { get; set; } = new();

        [JsonProperty("actual_staff")]
        public Staff ActualStaffing { get; set; } = new();
        public string MPEGroup { get; set; } = "";

        [JsonProperty("dataSource")]
        public string DataSource { get; set; } = "";
    }
    public class HourlyData
    {
        [JsonProperty("hour")]
        public string Hour { get; set; } = "";

        [JsonProperty("count")]
        public string Count { get; set; } = "";
        public string Sorted { get; set; } = "";
        public string Rejected { get; set; } = "";
    }
}
