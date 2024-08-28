namespace EIR_9209_2.Models
{
    public class AreaDwell
    {
        public required string EmployeeName { get; set; }
        public required string Ein { get; set; }
        public required string AreaName { get; set; }
        public TimeSpan DwellTimeDurationInArea { get; set; }
        public required string Type { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
    }
}