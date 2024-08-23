using EIR_9209_2.Models;

public class EmployeeScheduleSummary
{
    public string EIN { get; set; } = "";
    public string PayWeek { get; set; } = "";
    public string PayLoc { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string MiddleInit { get; set; } = "";
    public string DesActCode { get; set; } = "";
    public string Title { get; set; } = "";
    public string BaseOp { get; set; } = "";
    public string TourNumber { get; set; } = "";
    public List<Schedule> Schedule { get; set; } = [];
    public Dictionary<string, double> SelsLaborHrs = [];
    public double totalSelsDwellTime => SelsLaborHrs.Values.Sum();
    public Dictionary<string, double> TACSLaborHrs = [];
    public double totalTacsDwellTime => TACSLaborHrs.Values.Sum();

}