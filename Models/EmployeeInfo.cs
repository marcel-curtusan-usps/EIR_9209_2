namespace EIR_9209_2.Models
{
    public class EmployeeInfo
    {
        public string? EmployeeId { get; set; }
        public string? PayWeek { get; set; }
        public string? PayLocation { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleInit { get; set; }
        public string? DesActCode { get; set; }
        public string? Title { get; set; }
        public string? BaseOp { get; set; }
        public string? TourNumber { get; set; }
        public string? DutyStationFINNBR { get; set; }
        public string? Position { get; set; }
        public string? FacilityID { get; set; }
        public string? EmployeeStatus { get; set; }
        public string? BleId { get; set; }
        public string? EncodedId { get; set; }
        public int CardholderId { get; set; } = 0;
    }
}
