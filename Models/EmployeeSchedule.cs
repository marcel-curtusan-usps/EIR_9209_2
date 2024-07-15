namespace EIR_9209_2.Models
{
    public class EmployeeSchedule
    {
        public string Ein { get; set; } = "";
        public string Pay_Week { get; set; } = "";
        public int Day { get; set; } = 0;
        public int Hr_Code_Id { get; set; } = 0;
        public string Group_Name { get; set; } = "";
        public string Begin_Tour_Dtm { get; set; } = "";
        public string End_Tour_Dtm { get; set; } = "";
        public string Begin_Lunch_Dtm { get; set; } = "";
        public string End_Lunch_Dtm { get; set; } = "";
        public string Begin_Move_Dtm { get; set; } = "";
        public string End_Move_Dtm { get; set; } = "";
        public string Btour { get; set; } = "";
        public string Etour { get; set; } = "";
        public string Blunch { get; set; } = "";
        public string Elunch { get; set; } = "";
        public string Bmove { get; set; } = "";
        public string Emove { get; set; } = "";
        public int Section_Id { get; set; } = 0;
        public string Section_Name { get; set; } = "";
        public string Op_Code { get; set; } = "";
        public int Sort_Order { get; set; } = 0;
        public string Sfas_Code { get; set; } = "";
        public string Rte_Zip_Code { get; set; } = "";
        public string Rte_Nbr { get; set; } = "";
        public string Pvt_Ind { get; set; } = "";
        public int Hr_Leave { get; set; } = 0;
        public int Hr_Sched { get; set; } = 0;
        public int Hr_Tour { get; set; } = 0;
        public int Hr_Move { get; set; } = 0;
        public int Hr_Ot { get; set; } = 0;
        public int Day_Err_Cnt { get; set; } = 0;

    }
}
