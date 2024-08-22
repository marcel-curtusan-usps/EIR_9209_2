using EIR_9209_2.DataStore;
using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Security.Claims;

public class HubServices : Hub
{
    private readonly IInMemoryBackgroundImageRepository _backgroundImages;
    private readonly IInMemoryConnectionRepository _connections;
    private readonly IInMemoryDacodeRepository _dacodes;
    private readonly IInMemoryTagsRepository _tags;
    private readonly IInMemoryGeoZonesRepository _geoZones;
    private readonly IInMemorySiteInfoRepository _siteInfo;
    private readonly IInMemoryEmpSchedulesRepository _empSchedules;
    private readonly IInMemoryCamerasRepository _cameraMarkers;
    private readonly ILogger<HubServices> _logger;
    private readonly IConfiguration _configuration;
    private readonly Assembly _assembly;
    public HubServices(ILogger<HubServices> logger,
        IInMemoryBackgroundImageRepository backgroundImages,
        IInMemoryConnectionRepository connectionList,
        IInMemoryDacodeRepository dacodeList,
        IInMemoryTagsRepository tags,
        IInMemoryGeoZonesRepository zones,
        IInMemorySiteInfoRepository siteInfo,
        IInMemoryEmpSchedulesRepository empSchedules,
        IInMemoryCamerasRepository cameraMarkers,
        IConfiguration configuration)
    {
        _logger = logger;
        _backgroundImages = backgroundImages;
        _connections = connectionList;
        _dacodes = dacodeList;
        _tags = tags;
        _geoZones = zones;
        _siteInfo = siteInfo;
        _empSchedules = empSchedules;
        _cameraMarkers = cameraMarkers;
        _configuration = configuration;
        _assembly = Assembly.GetExecutingAssembly();
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
        return await Task.Run(_backgroundImages.GetAll);
    }
    public SiteInformation GetSiteInformation()
    {
        string nassCode = _configuration["ApplicationConfiguration:NassCode"]?.ToString();
        return _siteInfo.GetByNASSCode(nassCode);
    }
    public async Task<string> GetApplicationInfo()
    {
        var siteInfo = _siteInfo.GetByNASSCode(_configuration["ApplicationConfiguration:NassCode"].ToString());
        return JsonConvert.SerializeObject(new JObject
        {
            ["name"] = "Connected Facilities",
            ["version"] = $"{_assembly.GetName().Version}",
            ["description"] = "EIR-9209 is a web application for EIR-9209",
            ["siteName"] = siteInfo?.DisplayName,
            ["user"] = await GetUserName(Context.User),
            ["role"] = "Admin"
        });
    }

    public async Task<IEnumerable<GeoMarker>> GetBadgeTags()
    {
        return await Task.Run(() => _tags.GetTagsType("Badge"));
    }
    public async Task<IEnumerable<GeoMarker>> GetPIVTags()
    {
        return await Task.Run(() => _tags.GetTagsType("PIVVehicle"));
    }
    public async Task<IEnumerable<GeoMarker>> GetAGVTags()
    {
        return await Task.Run(() => _tags.GetTagsType("AutonomousVehicle"));
    }
    public async Task<IEnumerable<GeoMarker>> GetAccessPoints()
    {
        return await Task.Run(() => _tags.GetTagsType("AP"));
    }
    public async Task<IEnumerable<GeoZone>> GetGeoZones()
    {
        return await Task.Run(_geoZones.GetAll);
    }
    public async Task<IEnumerable<EmployeeInfo>> GetEmpSchedules()
    {
        return await Task.Run(_empSchedules.GetAll);
    }
    public async Task<IEnumerable<CameraMarker>> GetCameras()
    {
        return await Task.Run(_cameraMarkers.GetAll);
    }

    private Task<string> GetUserName(ClaimsPrincipal? user)
    {
        return Task.FromResult(user?.Identity?.Name);
    }


    // worker request for data of connection list
    public async Task<IEnumerable<Connection>> GetConnectionList()
    {
        return await Task.Run(_connections.GetAll);
    }
    // worker request for data of connectionType list
    public async Task<IEnumerable<ConnectionType>> GetConnectionTypeList()
    {
        return await Task.Run(_connections.GetTypeAll);
    }
    // worker request for data of connectionType list
    public async Task<IEnumerable<DesignationActivityToCraftType>> GetDacodeToCraftTypeList()
    {
        return await Task.Run(_dacodes.GetAll);
    }
    // client get all zones
    public async Task<IEnumerable<GeoZone>> GetGeoZoneList()
    {
        return await Task.Run(_geoZones.GetAll);
    }
}