using EIR_9209_2.Models;

public class ScheduleReport
{
    public string PayWeek { get; set; } = "";
    public string WeekDate1 { get; set; } = "";
    public string WeekDate2 { get; set; } = "";
    public string WeekDate3 { get; set; } = "";
    public string WeekDate4 { get; set; } = "";
    public string WeekDate5 { get; set; } = "";
    public string WeekDate6 { get; set; } = "";
    public string WeekDate7 { get; set; } = "";
    public List<SingleReport> ScheduleList { get; set; } = new List<SingleReport>();
}
public class SingleReport
{
    public string EIN { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string TourNumber { get; set; } = "";
    public string day1hr { get; set; } = "";
    public string day2hr { get; set; } = "";
    public string day3hr { get; set; } = "";
    public string day4hr { get; set; } = "";
    public string day5hr { get; set; } = "";
    public string day6hr { get; set; } = "";
    public string day7hr { get; set; } = "";
    public double totalhr { get; set; } = 0;
    public double day1selshr { get; set; } = 0;
    public double day2selshr { get; set; } = 0;
    public double day3selshr { get; set; } = 0;
    public double day4selshr { get; set; } = 0;
    public double day5selshr { get; set; } = 0;
    public double day6selshr { get; set; } = 0;
    public double day7selshr { get; set; } = 0;
    public double totalselshr { get; set; } = 0;
    public double day1tacshr { get; set; } = 0;
    public double day2tacshr { get; set; } = 0;
    public double day3tacshr { get; set; } = 0;
    public double day4tacshr { get; set; } = 0;
    public double day5tacshr { get; set; } = 0;
    public double day6tacshr { get; set; } = 0;
    public double day7tacshr { get; set; } = 0;
    public double totaltacshr { get; set; } = 0;
    public double percentage { get; set; } = 0;
}