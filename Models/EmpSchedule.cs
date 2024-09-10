// Ignore Spelling: EIN

namespace EIR_9209_2.Models
{
    public class EmployeeInfo
    {
        public required string EIN { get; set; } 
        public required string PayWeek { get; set; } 
        public required string PayLocation { get; set; }
        public required string LastName { get; set; } 
        public required string FirstName { get; set; } 
        public required string MiddleInit { get; set; } 
        public required string DesActCode { get; set; } 
        public required string Title { get; set; } 
        public required string BaseOp { get; set; } 
        public required string TourNumber { get; set; } 
    }

    public class Schedule
    {
        public string EIN { get; set; } = "";
        public string PayWeek { get; set; } = "";
        public string Day { get; set; } = "";
        public string HrCodeId { get; set; } = "";
        public string GroupName { get; set; } = "";
        public string BeginTourDtm { get; set; } = "";
        public string EndTourDtm { get; set; } = "";
        public string BeginLunchDtm { get; set; } = "";
        public string EndLunchDtm { get; set; } = "";
        public string BeginMoveDtm { get; set; } = "";
        public string EndMoveDtm { get; set; } = "";
        public string Btour { get; set; } = "";
        public string Etour { get; set; } = "";
        public string Blunch { get; set; } = "";
        public string Elunch { get; set; } = "";
        public string Bmove { get; set; } = "";
        public string Emove { get; set; } = "";
        public string SectionId { get; set; } = "";
        public string SectionName { get; set; } = "";
        public string OpCode { get; set; } = "";
        public string SortOrder { get; set; } = "";
        public string SfasCode { get; set; } = "";
        public string RteZipCode { get; set; } = "";
        public string RteNbr { get; set; } = "";
        public string PvtInd { get; set; } = "";
        public string HrLeave { get; set; } = "";
        public string HrSched { get; set; } = "";
        public string HrTour { get; set; } = "";
        public string HrMove { get; set; } = "";
        public string HrOt { get; set; } = "";
        public string DayErrCnt { get; set; } = "";
    }
}
