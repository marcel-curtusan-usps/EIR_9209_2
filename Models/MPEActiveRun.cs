public class MPEActiveRun
{
    public string MpeType { get; set; } = "";
    public int MpeNumber { get; set; }
    public string CurSortplan { get; set; } = "";
    public int CurRunNumber { get; set; } = 0;
    public int CurThruputOphr { get; set; } = 0;
    public int TotSortplanVol { get; set; } = 0;
    public int RpgEstVol { get; set; } = 0;
    public int ActVolPlanVolNbr { get; set; } = 0;
    public bool ActiveRun { get; set; }
    public DateTime CurrentRunStart { get; set; } = DateTime.MinValue;
    public DateTime CurrentRunEnd { get; set; } = DateTime.MinValue;
    public int CurOperationId { get; set; } = 0;
    public int ExpectedThroughput { get; set; } = 0;
    public int RpgExpectedThruput { get; set; } = 0;
    public int ArsRecrej3 { get; set; } = 0;
    public int SweepRecrej3 { get; set; } = 0;
    public string MpeId { get; set; } = "";
    public string Type { get; set; } = "";
    public int Tour { get; set; } = 0;
    public List<Hours> Hourlydata { get; set; } = new List<Hours>();
}
public class Hours
{
    public string Hour { get; set; } = "";
    public int Count { get; set; } = 0;
    public int StaffCount { get; set; } = 0;
}