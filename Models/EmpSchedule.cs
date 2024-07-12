namespace EIR_9209_2.Models
{
    public class EmpSchedule
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
        public List<Schedule> WeekSchedule { get; set; } = new List<Schedule>();
    }

    public class Schedule
    {
        public string PayWeek { get; set; } = "";
        public string Day { get; set; } = "";
        //public string HrCodeId { get; set; } = "";
        public string GroupName { get; set; } = "";
        public string BeginTourDtm { get; set; } = "";
        public string EndTourDtm { get; set; } = "";
        //public string BeginLunchDtm { get; set; } = "";
        //public string EndLunchDtm { get; set; } = "";
        //public string BeginMoveDtm { get; set; } = "";
        //public string EndMoveDtm { get; set; } = "";
        public string Btour { get; set; } = "";
        public string Etour { get; set; } = "";
        //public string Blunch { get; set; } = "";
        //public string Elunch { get; set; } = "";
        //public string Bmove { get; set; } = "";
        //public string Emove { get; set; } = "";
        //public string SectionId { get; set; } = "";
        //public string SectionName { get; set; } = "";
        //public string OpCode { get; set; } = "";
        //public string SortOrder { get; set; } = "";
        //public string SfasCode { get; set; } = "";
        //public string RteZipCode { get; set; } = "";
        //public string RteNbr { get; set; } = "";
        //public string PvtInd { get; set; } = "";
        public string HrLeave { get; set; } = "";
        public string HrSched { get; set; } = "";
        public string HrTour { get; set; } = "";
        public string HrMove { get; set; } = "";
        public string HrOt { get; set; } = "";
        public string DayErrCnt { get; set; } = "";
    }
}
