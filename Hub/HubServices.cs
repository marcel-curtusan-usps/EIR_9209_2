using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Claims;

public class HubServices : Hub
{
    public static ConcurrentDictionary<string, string> _connectionIds = new ConcurrentDictionary<string, string>();

    private readonly IBackgroundImageRepository _backgroundImages;
    private ILogger<BackgroundImageRepository> _logger;
    public HubServices(ILogger<BackgroundImageRepository> logger, IBackgroundImageRepository backgroundImages)
    {
        _logger = logger;
        _backgroundImages = backgroundImages;
    }
    public async Task AddToGroup(string groupName)
    {
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
    //worker status
    public async Task ReceiveStatusUpdate(string status)
    {
        await Clients.All.SendAsync("WorkerStatusUpdate", status);
        Console.WriteLine("Worker Status : " + Context.ConnectionId);
    }
}