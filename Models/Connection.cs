using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Newtonsoft.Json;

public class Connection
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string _id { get; set; }
    [JsonIgnore]
    [BsonIgnore]
    private EWorkerServiceState _status;
    [BsonIgnore]
    public EWorkerServiceState Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                // Update status in database or notify listeners of status change
            }
        }
    }
    public bool ActiveConnection { get; set; } = false;
    public string AdminEmailRecepient { get; set; } = "";
    public bool ApiConnected { get; set; } = false;
    public string Name { get; set; } = "";
    public string CreatedByUsername { get; set; } = "";
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public long MillisecondsInterval { get; set; } = 1000;
    public string DeactivatedByUsername { get; set; } = "";
    public DateTime DeactivatedDate { get; set; }
    public string Hostname { get; set; } = "";
    public int HoursBack { get; set; } = 0;
    public int HoursForward { get; set; } = 0;
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string IpAddress { get; set; } = "";
    public DateTime LasttimeApiConnected { get; set; }
    public DateTime LastupDate { get; set; }
    public string LastupdateByUsername { get; set; } = "";
    public string MessageType { get; set; } = "";
    public string NassCode { get; set; } = "";
    public string OutgoingApikey { get; set; } = "";
    public Int32 Port { get; set; } = 0;
    public bool UdpConnection { get; set; } = false;
    public bool TcpIpConnection { get; set; } = false;
    public bool WsConnection { get; set; } = false;
    public bool ApiConnection { get; set; } = false;
    public string Url { get; set; } = "";
    public string OAuthUrl { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string ClientId { get; set; } = "";
}