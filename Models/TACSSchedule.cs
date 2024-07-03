namespace EIR_9209_2.Models
{
    public class TACSSchedule
    {
        //"FinanceNo","Organization","SubUnit","ScheduleNo","EmployeeID","LastName","FI","MI","RSC","D/A","LDC","Oper/LU","Route","LtdTour","BeginTour","EndTour","Wk","Lunch","AssignmentType","EffectiveStartDt"
        public string id = Guid.NewGuid().ToString();
        public int FinanceNo { get; set; } = 0;
        public string Organization { get; set; } = "";
        public int SubUnit { get; set; } = 0;
        public int ScheduleNo { get; set; } = 0;
        public string EIN { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FI { get; set; } = "";
        public string MI { get; set; } = "";
        public string RSC { get; set; } = "";
        public string DA { get; set; } = "";
        public string LDC { get; set; } = "";
        public string OperLU { get; set; } = "";
        public string Route { get; set; } = "";
        public string LtdTour { get; set; } = "";
        public string BeginTour { get; set; } = "";
        public string EndTour { get; set; } = "";
        public string Wk { get; set; } = "";
        public string Lunch { get; set; } = "";
        public string AssignmentType { get; set; } = "";
        public string EffectiveStartDt { get; set; } = "";
        public string EffectiveEndDt { get; set; } = "";

    }
}
