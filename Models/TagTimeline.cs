// Ignore Spelling: Timeline

namespace EIR_9209_2.Models
{
    public class TagTimeline
    {
        public required string EmployeeName { get; set; } 
        public required string Ein { get; set; }
        public required string AreaName { get; set; }
        public required DateTime Hour { get; set; }
        public required DateTime Start { get; set; }
        public required DateTime End { get; set; }
        public required TimeSpan Duration { get; set; }
        public required string Type { get; set; }
        public required string FirstName { get; set; } 
        public required string LastName { get; set; }
    }
}