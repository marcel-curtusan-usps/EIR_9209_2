
using Newtonsoft.Json;

public class Connection
{
    [JsonIgnore]
    private EWorkerServiceState _status;
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
    public bool LogData { get; set; } = false;
    public string AdminEmailRecepient { get; set; } = "";
    public bool ApiConnected { get; set; } = false;
    [JsonProperty("name")]
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
    public int Port { get; set; } = 0;
    public bool UdpConnection { get; set; } = false;
    public bool TcpIpConnection { get; set; } = false;
    public bool WsConnection { get; set; } = false;
    public bool ApiConnection { get; set; } = false;
    public string Url { get; set; } = "";
    public string DbType { get; set; } = "";
    public string ConnectionString { get; set; } = "";
    public string AuthType { get; set; } = "";
    public string OAuthUrl { get; set; } = "";
    public string OAuthUserName { get; set; } = "";
    public string OAuthPassword { get; set; } = "";
    public string OAuthClientId { get; set; } = "";
    /// <summary>
    /// The timeout in milliseconds for the connection.
    /// </summary>
    public int MillisecondsTimeout { get; set; } = 60000;
    /// <summary>
    /// The ID for the map connection.
    /// </summary>
    public string MapId { get; set; } = "";
    /// <summary>
    /// The tenant ID for the connection.
    /// </summary>
    public string TenantId { get; set; } = "";
    /// <summary>
    /// Indicates if the connection is a webhook connection.
    /// </summary>
    public bool WebhookConnection { get; set; } = false;
    /// <summary>
    /// The URL for the webhook connection.
    /// </summary>
    public string WebhookUrl { get; set; } = "";
    /// <summary>
    /// The username for the webhook connection, if required.
    /// </summary>
    public string WebhookUserName { get; set; } = "";
    /// <summary>
    /// The password for the webhook connection, if required.
    /// </summary>
    public string WebhookPassword { get; set; } = "";
}