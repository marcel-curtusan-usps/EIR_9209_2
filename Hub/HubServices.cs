using EIR_9209_2.DataStore;
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
    private readonly IInMemoryBackgroundImageRepository _backgroundImages;
    private readonly IInMemoryConnectionRepository _connections;
    private readonly IInMemoryTagsRepository _tags;
    private readonly IInMemoryGeoZonesRepository _geoZones;
    private readonly IInMemorySiteInfoRepository _siteInfo;
    private readonly ILogger<HubServices> _logger;
    private readonly IConfiguration _configuration;
    public HubServices(ILogger<HubServices> logger,
        IInMemoryBackgroundImageRepository backgroundImages,
        IInMemoryConnectionRepository connectionList,
        IInMemoryTagsRepository tags,
        IInMemoryGeoZonesRepository zones,
        IInMemorySiteInfoRepository siteInfo,
        IConfiguration configuration)
    {
        _logger = logger;
        _backgroundImages = backgroundImages;
        _connections = connectionList;
        _tags = tags;
        _geoZones = zones;
        _siteInfo = siteInfo;
        _configuration = configuration;
    }
    public async Task JoinGroup(string groupName)
    {
        _logger.LogInformation($"{Context.ConnectionId} Joined the {groupName} group");

        string userId = Context.ConnectionId;
        await Groups.AddToGroupAsync(userId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        _logger.LogInformation($"{Context.ConnectionId} is Leaving the Group: {groupName}");

        string userId = Context.ConnectionId;

        await Groups.RemoveFromGroupAsync(userId, groupName);

    }
    public async Task CallerMessage(string user, object message, string method)
    {
        await Clients.Caller.SendAsync(method, user, message.ToString());
    }
    //public override async Task OnConnectedAsync()
    //{
    //    Console.WriteLine("Client connected: " + Context.ConnectionId);
    //    _connectionIds.TryAdd(Context.ConnectionId, Context.ConnectionId);
    //    await base.OnConnectedAsync();
    //}
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        //string removedConnectionId;
        //_connectionIds.TryRemove(Context.ConnectionId, out removedConnectionId);
        //// Remove the connection from the group in the _groups dictionary
        //foreach (var group in _groups)
        //{
        //    if (group.Value.Contains(Context.ConnectionId))
        //    {
        //        group.Value.Remove(Context.ConnectionId);
        //        break;
        //    }
        //}
        string userId = Context.ConnectionId;

        // await Groups.RemoveFromGroupAsync(userId, groupName);

        await base.OnDisconnectedAsync(exception);
    }
    //public async Task<List<string>> GetConnectedClientsInGroup(string groupName)
    //{
    //    if (_groups.TryGetValue(groupName, out var connections))
    //    {
    //        return connections.ToList();
    //    }

    //    return new List<string>();
    //}

    //public async Task<List<string>> GetAllGroups()
    //{
    //    return Clients.All;
    //}
    public async Task<IEnumerable<BackgroundImage>> GetBackgroundImages()
    {
        return _backgroundImages.GetAll();
    }
    public async Task<string> GetApplicationInfo()
    {
        var siteInfo = _siteInfo.GetByNASSCode(_configuration[key: "SiteIdentity:NassCode"].ToString());
        return JsonConvert.SerializeObject(new JObject
        {
            ["name"] = "Connected Facilities",
            ["version"] = "1.0.0.1",
            ["description"] = "EIR-9209 is a web application for EIR-9209",
            ["siteName"] = siteInfo?.DisplayName,
            ["user"] = await GetUserName(Context.User),
            ["role"] = "Admin"
        });
    }
    public async Task<List<GeoMarker>> GetPersonTags()
    {
        return _tags.GetAll().ToList();
    }
    public async Task<List<GeoZone>> GetGeoZones()
    {
        return _geoZones.GetAll().ToList();
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
        // await SendMessageToGroup("Tags", workerData, "tags");
        // _logger.LogInformation($"Worker Data for QPE :  {Context.ConnectionId}  <-->  {workerData}");
    }
}