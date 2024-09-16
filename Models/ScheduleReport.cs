using System.Globalization;

public class ScheduleReport
{
    public string PayWeek { get; set; } = ""; 
    public string EIN { get; set; } = "";
    private string _empLastName = "";
    public string LastName
    {
        get => _empLastName;
        set => _empLastName = ConvertToTitleCase(value);
    }
    private string _empFirstName = "";
    public string FirstName
    {
        get => _empFirstName;
        set => _empFirstName = ConvertToTitleCase(value);
    }
    public string TourNumber { get; set; } = "";
    public double DailyQREhr { get; set; } = 0;
    public double DailyTACShr { get; set; } = 0;
    public double Percentage { get; set; } = 0;
    public int Day { get; set; } = 0;
    public string Date { get; set; } = "";
    public string BeginTourHour { get; set; } = "";
    public string EndTourHour { get; set; } = "";
    public string OpCode { get; set; } = "";
    public string WorkStatus { get;  set; } = "";
    public double HrsLeave { get;  set; } = 0;
    public double HrsSchedule { get;  set; } = 0;
    public double HrsMove { get;  set; } = 0.0;
    public DayOfWeek DayName { get; set; }
    public string SectionName { get;  set; } = "";

    private string ConvertToTitleCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
    }
}