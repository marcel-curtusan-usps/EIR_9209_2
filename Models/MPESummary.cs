public class MPESummary
{
    public Dictionary<string, double> laborHrs = new Dictionary<string, double>();
    public double totalDwellTime => laborHrs.Values.Sum();
    public Dictionary<string, int> laborCounts = new Dictionary<string, int>();
    public int totalPresent => laborCounts.Values.Sum();
    public double clerkDwellTime = 0;
    public double mhDwellTime = 0;
    public double maintDwellTime = 0;
    public double supervisorDwellTime = 0;
    public double otherDwellTime = 0;
    public string mpeName = "";
    public string mpeNumber = "";
    public double actualYield = 0;
    public int piecesFeed = 0;
    public int piecesSorted = 0;
    public int piecesRejected = 0;
    public string hour = "";
    public int clerkPresent = 0;
    public int mhPresent = 0;
    public int maintPresent = 0;
    public int supervisorPresent = 0;
    public int otherPresent = 0;
    public int standardPiecseFeed = 0;
    public int standardStaffHrs = 0;
}