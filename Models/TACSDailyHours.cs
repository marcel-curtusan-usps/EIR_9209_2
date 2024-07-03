namespace EIR_9209_2.Models
{
    public class TACSDailyHours
    {
        //"Start Dt","Finance No","Organization","Sub-Unit","Employee Id","Last Name","FI","MI","HL Cd1","RSC","D/A","LDC","Oper/LU","Level","Exempt","HL Cd2","Code Value","Day","Hours Code","Hours",
        public string id = Guid.NewGuid().ToString();
        public int StartDt { get; set; } = 0;
        public int FinanceNo { get; set; } = 0;
        public string Organization { get; set; } = "";
        public int SubUnit { get; set; } = 0;
        public string EIN { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FI { get; set; } = "";
        public string MI { get; set; } = "";
        public string HLCd1 { get; set; } = "";
        public string RSC { get; set; } = "";
        public string DA { get; set; } = "";
        public string LDC { get; set; } = "";
        public string OperLU { get; set; } = "";
        public string Level { get; set; } = "";
        public string Exempt { get; set; } = "";
        public string HLCd2 { get; set; } = "";
        public string CodeValue { get; set; } = "";
        public string Day { get; set; } = "";
        public string HoursCode { get; set; } = "";
        public double Hours { get; set; } = 0.0;

    }
}
