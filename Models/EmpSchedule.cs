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
        public string EIN { get; set; } 
        public string PayWeek { get; set; } 
        public int Day { get; set; }
        public int HrCodeId { get; set; }
        public string GroupName { get; set; } 
        public string BeginTourDtm { get; set; } 
        public string EndTourDtm { get; set; }
        public string BeginLunchDtm { get; set; }
        public string EndLunchDtm { get; set; } 
        public string BeginMoveDtm { get; set; } 
        public string EndMoveDtm { get; set; } 
        public string Btour { get; set; } 
        public string Etour { get; set; } 
        public string Blunch { get; set; }
        public string Elunch { get; set; }
        public string Bmove { get; set; } 
        public string Emove { get; set; }
        public int SectionId { get; set; } 
        public string SectionName { get; set; }
        public string OpCode { get; set; }
        public int SortOrder { get; set; }
        public string SfasCode { get; set; } 
        public string RteZipCode { get; set; } 
        public string RteNbr { get; set; } 
        public string PvtInd { get; set; }
        public double HrLeave { get; set; } 
        public double HrSched { get; set; } 
        public double HrTour { get; set; } 
        public double HrMove { get; set; } 
        public double HrOt { get; set; } 
        public double DayErrCnt { get; set; } 
    }

    public class EmployeesData
    {
        public List<string> COLUMNS { get; set; }
        public List<List<object>> DATA { get; set; }
    }

    public class Data
    {
        public EmployeesData EMPLOYEES { get; set; }
    }

    public class Root
    {
        public string MESSAGE { get; set; }
        public string STATUS { get; set; }
        public Data DATA { get; set; }
        public List<object> ERROR { get; set; }
    }
}
