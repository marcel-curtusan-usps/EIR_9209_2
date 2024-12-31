namespace EIR_9209_2.Models
{
    public class TACSEmployeePayPeirod
    {
        //"YrPPWk","Hire Fin#.","Organization Name","Sub-Unit","Employee Id","Last Name","FI","MI","Pay Loc/Fin Unit","Var. EAS","Borrowed","Auto H/L","Annual Lv Bal","Sick Lv Bal","LWOP Lv Bal","FMLA Hrs","FMLA Used","SLDC Used",
        public string id = Guid.NewGuid().ToString();
        public string YrPPWk { get; set; } = "";
        public int HireFinanceNo { get; set; } = 0;
        public string Organization { get; set; } = "";
        public int SubUnit { get; set; } = 0;
        public string EIN { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FI { get; set; } = "";
        public string MI { get; set; } = "";
        public string PayLocFinUnit { get; set; } = "";
        public string VarEAS { get; set; } = "";
        public string Borrowed { get; set; } = "";
        public string AutoHL { get; set; } = "";
        public string AnnualLvBal { get; set; } = "";
        public string SickLvBal { get; set; } = "";
        public string LWOP { get; set; } = "";
        public string FMLAHrs { get; set; } = "";
        public string FMLAUsed { get; set; } = "";
        public string SLDCUsed { get; set; } = "";
        public string DefaultOPCode { get; set; } = "";
        public string TACSCode { get; set; } = "";
        public string TACSDate { get; set; } = "";
        public string TACSTime { get; set; } = "";
        public DateTime TACSDateTime { get; set; }
    }
}
