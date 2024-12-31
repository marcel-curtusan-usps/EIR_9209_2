namespace EIR_9209_2.Models
{
    public class ConnectionType
    {

        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }
        public List<Messagetype>? MessageTypes { get; set; } = [];
    }

    public class Messagetype
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? BaseURL { get; set; }
    }
}
