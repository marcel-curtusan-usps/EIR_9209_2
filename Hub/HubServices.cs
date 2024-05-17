using EIR_9209_2.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Claims;

public class HubServices : Hub
{
    public readonly static ConcurrentDictionary<string, string> _connectionIds = new();
    private readonly static ConcurrentDictionary<string, List<string>> _groups = new();

    private readonly IInMemoryTagsBackgroundImageRepository _backgroundImages;
    private readonly IInMemoryConnectionRepository _connections;
    private readonly IInMemoryTagsRepository _tags;
    private readonly IInMemoryGeoZonesRepository _geoZones;
    private readonly IOptions<SiteIdentitySettings> _siteSettings;
    private readonly ILogger<HubServices> _logger;
    public HubServices(ILogger<HubServices> logger, IInMemoryTagsBackgroundImageRepository backgroundImages, IInMemoryConnectionRepository connectionList, IInMemoryTagsRepository tags, IInMemoryGeoZonesRepository zones, IOptions<SiteIdentitySettings> siteSettings)
    {
        _logger = logger;
        _backgroundImages = backgroundImages;
        _connections = connectionList;
        _tags = tags;
        _geoZones = zones;
        _siteSettings = siteSettings;
    }
    public async Task AddToGroup(string groupName)
    {
        _logger.LogInformation($"{Context.ConnectionId} Joined the {groupName} group");
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        // Add the connection to the group in the _groups dictionary
        _groups.AddOrUpdate(groupName, new List<string> { Context.ConnectionId }, (_, connections) =>
        {
            connections.Add(Context.ConnectionId);
            return connections;
        });
    }

    public async Task RemoveFromGroup(string groupName)
    {
        _logger.LogInformation($"{Context.ConnectionId} Has been Removed from the {groupName} group");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SendMessageToGroup(string groupName, string message, string method)
    {
        if (Clients != null)
        {
            await Clients.Group(groupName).SendAsync(method, message);
        }
    }
    public async Task CallerMessage(string user, object message, string method)
    {
        await Clients.Caller.SendAsync(method, user, message.ToString());
    }
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("Client connected: " + Context.ConnectionId);
        _connectionIds.TryAdd(Context.ConnectionId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        string removedConnectionId;
        _connectionIds.TryRemove(Context.ConnectionId, out removedConnectionId);
        // Remove the connection from the group in the _groups dictionary
        foreach (var group in _groups)
        {
            if (group.Value.Contains(Context.ConnectionId))
            {
                group.Value.Remove(Context.ConnectionId);
                break;
            }
        }
        await base.OnDisconnectedAsync(exception);
    }
    public async Task<List<string>> GetConnectedClientsInGroup(string groupName)
    {
        if (_groups.TryGetValue(groupName, out var connections))
        {
            return connections;
        }

        return new List<string>();
    }

    public async Task<List<string>> GetAllGroups()
    {
        return _groups.Keys.ToList();
    }
    public async Task<IEnumerable<BackgroundImage>> GetBackgroundImages()
    {
        return _backgroundImages.GetAll();
    }
    public async Task<string> GetApplicationInfo()
    {
        return JsonConvert.SerializeObject(new JObject
        {
            ["name"] = "Connected Facilities",
            ["version"] = "1.0.0.1",
            ["description"] = "EIR-9209 is a web application for EIR-9209",
            ["siteName"] = _siteSettings.Value.DisplayName,
            ["user"] = await GetUserName(Context.User),
            ["role"] = "Admin"
        });
    }
    public async Task<IEnumerable<GeoMarker>> GetPersonTags()
    {
        return _tags.GetAll();
    }
    public async Task<List<GeoZone>> GetGeoZoneMPE()
    {
        return new List<GeoZone>();
    }

    private Task<string> GetUserName(ClaimsPrincipal? user)
    {
        return Task.FromResult(user?.Identity?.Name);
    }


    // worker request for data of connection list
    public async Task<IEnumerable<Connection>> GetConnectionList()
    {
        return _connections.GetAll();
    }
    // client get all zones
    public async Task<IEnumerable<GeoZone>> GetGeoZoneList()
    {
        return _geoZones.GetAll();
    }
    public async Task WorkerStatusUpdate(string status)
    {
        //await Clients.All.SendAsync("WorkerStatusUpdate", status);
        // _logger.LogInformation($"Worker Status :  {Context.ConnectionId}  <-->  {status}");
    }
    public async Task WorkerData(byte[] data)
    {
        //await Clients.All.SendAsync("WorkerStatusUpdate", status);
        string workerData = System.Text.Encoding.UTF8.GetString(data);
        //await SendMessageToGroup("Tags", workerData, "tags");
        // _logger.LogInformation($"Worker Data for QPE :  {Context.ConnectionId}  <-->  {workerData}");
    }
    public async Task WorkerPositionData(byte[] data)
    {
        //await Clients.All.SendAsync("WorkerStatusUpdate", status);
        string workerData = System.Text.Encoding.UTF8.GetString(data);
        await SendMessageToGroup("Tags", workerData, "tags");
        // _logger.LogInformation($"Worker Data for QPE :  {Context.ConnectionId}  <-->  {workerData}");
    }
}