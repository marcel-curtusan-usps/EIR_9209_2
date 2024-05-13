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
    public readonly static ConcurrentDictionary<string, string> _connectionIds = new ConcurrentDictionary<string, string>();

    private readonly IBackgroundImageRepository _backgroundImages;
    private readonly IConnectionRepository _connections;
    private ILogger<HubServices> _logger;
    public HubServices(ILogger<HubServices> logger, IBackgroundImageRepository backgroundImages, IConnectionRepository connectionList)
    {
        _logger = logger;
        _backgroundImages = backgroundImages;
        _connections = connectionList;
    }
    public async Task AddToGroup(string groupName)
    {
        _logger.LogInformation($"{Context.ConnectionId} Joined the {groupName} group");
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task RemoveFromGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SendMessageToGroup(string groupName, string message, string method)
    {
        await Clients.Group(groupName).SendAsync(method, message);
    }
    public async Task CallerMessage(string user, object message, string method)
    {
        await Clients.Caller.SendAsync(method, user, message.ToString());
    }


    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("Client connected: " + Context.ConnectionId);
        _connectionIds.TryAdd(Context.ConnectionId, Context.ConnectionId);
        await CallerMessage(Context.ConnectionId, new JObject
        {
            ["name"] = "Connected Facilities",
            ["version"] = typeof(Program).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version,
            ["description"] = "EIR-9209 is a web application for EIR-9209",
            ["siteName"] = "",
            ["user"] = await GetUserName(Context.User),
            ["role"] = "Admin"
        }, "applicationInfo");

        var images = await _backgroundImages.GetAll();
        await CallerMessage(Context.ConnectionId, JsonConvert.SerializeObject(images), "backgroundImages");


        await base.OnConnectedAsync();
    }

    private Task<string> GetUserName(ClaimsPrincipal? user)
    {
        return Task.FromResult(user?.Identity?.Name);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        string removedConnectionId;
        _connectionIds.TryRemove(Context.ConnectionId, out removedConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
    // worker request for data of connection list
    public async Task GetConnectionList()
    {
        var conList = await _connections.GetAll();
        await CallerMessage(Context.ConnectionId, JsonConvert.SerializeObject(conList), "WorkerConnectionList");
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
        await SendMessageToGroup("Tags", workerData, "tags");
        // _logger.LogInformation($"Worker Data for QPE :  {Context.ConnectionId}  <-->  {workerData}");
    }
}