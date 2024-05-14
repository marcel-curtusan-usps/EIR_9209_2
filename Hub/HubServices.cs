using EIR_9209_2.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.SignalR;
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

    private readonly IBackgroundImageRepository _backgroundImages;
    private readonly IConnectionRepository _connections;
    private readonly ITagsRepository _tags;
    private readonly ILogger<HubServices> _logger;
    public HubServices(ILogger<HubServices> logger, IBackgroundImageRepository backgroundImages, IConnectionRepository connectionList, ITagsRepository tags)
    {
        _logger = logger;
        _backgroundImages = backgroundImages;
        _connections = connectionList;
        _tags = tags;
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
    public async Task<List<BackgroundImage>> GetBackgroundImages()
    {
        return await _backgroundImages.GetAll();
    }
    public async Task<string> GetApplicationInfo()
    {
        return JsonConvert.SerializeObject(new JObject
        {
            ["name"] = "Connected Facilities",
            ["version"] = "1.0.0.1",
            ["description"] = "EIR-9209 is a web application for EIR-9209",
            ["siteName"] = "",
            ["user"] = await GetUserName(Context.User),
            ["role"] = "Admin"
        });
    }
    public async Task<List<TagGeoJson>> GetPersonTags()
    {
        return await _tags.GetAllPersonTag();
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
    public async Task<List<Connection>> GetConnectionList()
    {
        return await _connections.GetAll();
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