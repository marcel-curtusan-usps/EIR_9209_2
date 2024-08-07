namespace EIR_9209_2.Models
{
    public class TagTimeline
    {
        public string EmployeeName { get; set; } = "";
        public string Ein { get; set; } = "";
        public string AreaName { get; set; } = "";
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan Duration { get; set; }
        public string Type { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
    }
}