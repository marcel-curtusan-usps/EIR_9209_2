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
    private readonly IInMemoryEmployeesRepository _empSchedules;
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
        IInMemoryEmployeesRepository empSchedules,
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
        if (exception != null)
        {
            _logger.LogError(exception.ToString());
        }
       
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
    public async Task<string> GetApplicationInfo()
    {
        var siteInfo = _siteInfo.GetSiteInfo();
        return JsonConvert.SerializeObject(new JObject
        {
            ["name"] = "Connected Facilities",
            ["version"] = $"{_assembly.GetName().Version}",
            ["description"] = "EIR-9209 is a web application for EIR-9209",
            ["siteName"] = siteInfo?.DisplayName,
            ["user"] = GetUserName(Context.User),
            ["role"] = "Admin"
        });
    }

    public async Task<IEnumerable<GeoMarker>> GetBadgeTags()
    {
        return await Task.Run(() => _tags.GetTagsType("Badge")).ConfigureAwait(false);
    }
    public async Task<IEnumerable<VehicleGeoMarker>> GetPIVTags()
    {
        return await Task.Run(() => _tags.GetAllPIV()).ConfigureAwait(false);
    }
    public async Task<IEnumerable<VehicleGeoMarker>> GetAGVTags()
    {
        return await Task.Run(() => _tags.GetAllAGV()).ConfigureAwait(false);
    }
    public async Task<IEnumerable<GeoMarker>> GetAccessPoints()
    {
        return await Task.Run(() => _tags.GetTagsType("AP")).ConfigureAwait(false);
    }
    public async Task<IEnumerable<GeoZone>> GetGeoZones(string zoneType)
    {
        return await Task.Run(() => _geoZones.GetGeoZone(zoneType)).ConfigureAwait(false);
    }
    public async Task<IEnumerable<GeoZoneDockDoor>> GetDockDoorGeoZones()
    {
        return await Task.Run(_geoZones.GetDockDoor).ConfigureAwait(false);
    }
    public async Task<IEnumerable<EmployeeInfo>> GetEmpSchedules()
    {
        return await Task.Run(_empSchedules.GetAll).ConfigureAwait(false);
    }
    public async Task<IEnumerable<CameraGeoMarker>> GetCameras()
    {
        return await Task.Run(_cameraMarkers.GetAll).ConfigureAwait(false);
    }

    private string? GetUserName(ClaimsPrincipal? user) => user?.Identity?.Name;


    // worker request for data of connection list
    public async Task<IEnumerable<Connection>> GetConnectionList()
    {
        return await Task.Run(_connections.GetAll).ConfigureAwait(false);
    }
    // worker request for data of connectionType list
    public async Task<IEnumerable<ConnectionType>> GetConnectionTypeList()
    {
        return await Task.Run(_connections.GetTypeAll).ConfigureAwait(false);
    }
    // worker request for data of connectionType list
    public async Task<IEnumerable<DesignationActivityToCraftType>> GetDacodeToCraftTypeList()
    {
        return await Task.Run(_dacodes.GetAll).ConfigureAwait(false);
    }
    // client get all zones
    public async Task<IEnumerable<GeoZone>> GetGeoZoneList()
    {
        return await Task.Run(_geoZones.GetAll).ConfigureAwait(false); 
    }
}