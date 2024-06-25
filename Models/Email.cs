namespace EIR_9209_2.Models
{
    public class Email
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string EmailAddress { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string ACE { get; set; } = "";
        public string ReportName { get; set; } = "";
        public string MPEName { get; set; } = "";
        public bool Enabled { get; set; } = false;

    }
}
