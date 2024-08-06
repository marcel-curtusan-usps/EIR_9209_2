namespace EIR_9209_2.Models
{
    public class TagTimeline
    {
        public string EmployeeName { get; set; } = "";
        public string Ein { get; set; } = "";
        public string AreaName { get; set; } = "";
        public DateTime Start_txt { get; set; } = DateTime.MinValue;
        public long Start { get; set; } = 0;
        public DateTime End_txt { get; set; } = DateTime.MinValue;
        public long End { get; set; } = 0;
        public TimeSpan Duration { get; set; }
        public string Type { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
    }
}