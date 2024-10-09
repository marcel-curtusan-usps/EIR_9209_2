using EIR_9209_2.DataStore;
using EIR_9209_2.Models;
using EIR_9209_2.Utilities;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.RegularExpressions;

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
    private readonly IWebHostEnvironment _env;
    public HubServices(ILogger<HubServices> logger,
        IInMemoryBackgroundImageRepository backgroundImages,
        IInMemoryConnectionRepository connectionList,
        IInMemoryDacodeRepository dacodeList,
        IInMemoryTagsRepository tags,
        IInMemoryGeoZonesRepository zones,
        IInMemorySiteInfoRepository siteInfo,
        IInMemoryEmployeesRepository empSchedules,
        IInMemoryCamerasRepository cameraMarkers,
        IConfiguration configuration,
        IWebHostEnvironment env)    
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
        _env = env;
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
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId} UserName: {await GetUserName(Context.User)}, DateTime:{DateTime.Now.ToString()}" );

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        string userId = Context.ConnectionId;
        if (exception != null)
        {
            _logger.LogError(exception.ToString());
        }
       
        // await Groups.RemoveFromGroupAsync(userId, groupName);

        await base.OnDisconnectedAsync(exception);
    }
    public async Task<IEnumerable<OSLImage>> GetBackgroundImages()
    {
        return await Task.Run(_backgroundImages.GetAll);
    }
    public async Task<string> GetApplicationInfo()
    {
        var siteInfo = await _siteInfo.GetSiteInfo();
        if (_env.IsDevelopment())
        {
            return JsonConvert.SerializeObject(new JObject
            {
                ["ApplicationName"] = _configuration["ApplicationConfiguration:ApplicationName"],
                ["ApplicationVersion"] = Helper.GetCurrentVersion(),
                ["ApplicationDescription"] = _configuration["ApplicationConfiguration:ApplicationDescription"],
                ["SiteName"] = siteInfo?.DisplayName,
                ["User"] = Context.User.Identity.IsAuthenticated ? await GetUserName(Context.User) : "CF Admin",
                ["Role"] = Context.User.Identity.IsAuthenticated ? await GetUserRole(Context.User) : "Admin"
            });
        }
        else
        {
            return JsonConvert.SerializeObject(new JObject
            {
                ["ApplicationName"] = _configuration["ApplicationConfiguration:ApplicationName"],
                ["ApplicationVersion"] = Helper.GetCurrentVersion(),
                ["ApplicationDescription"] = _configuration["ApplicationConfiguration:ApplicationDescription"],
                ["SiteName"] = siteInfo?.DisplayName,
                ["User"] = Context.User.Identity.IsAuthenticated ? await GetUserName(Context.User) : "Operator",
                ["Role"] = Context.User.Identity.IsAuthenticated ? await GetUserRole(Context.User) : "Operator"
            });
        }
    }

    private Task<string> GetUserName(ClaimsPrincipal? user)
    {
        return Task.FromResult(user?.Identity?.Name ?? string.Empty);
    }
    private async Task<string> GetUserRole(ClaimsPrincipal? user)
    {
        try
        {
            var userGroups = await GetUserGroups(user).ConfigureAwait(false);
            return GetRoleFromGroups(userGroups);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return "Operator";
        }
    }
    private string GetRoleFromGroups(IEnumerable<string> userGroups)
    {
        var roles = new Dictionary<string, string>
        {
            { "AdminRole", "Admin" },
            { "PlantManager", "PM" },
            { "MaintenanceRole", "Maintenance" },
            { "OIE", "OIE" }
        };

        foreach (var role in roles)
        {
            var configRoles = _configuration[$"UserRole:{role.Key}"]?.Split(',').Select(r => r.Trim()).ToList() ?? new List<string>();
            if (configRoles.Intersect(userGroups, StringComparer.OrdinalIgnoreCase).Any())
            {
                return role.Value;
            }
        }

        return "Operator";
    }
    private Task<IEnumerable<string>> GetUserGroups(ClaimsPrincipal? user)
    {
        if (user == null)
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        if (user is not WindowsPrincipal windowsPrincipal)
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        if (windowsPrincipal.Identity is not WindowsIdentity windowsIdentity)
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        var groups = windowsIdentity.Groups
                                    .Select(g => g.Translate(typeof(NTAccount)).ToString().TrimStart(@"USA\".ToCharArray()))
                                    .ToList();

        return Task.FromResult<IEnumerable<string>>(groups);
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
    public async Task<MPERunPerformance> GetGeoZoneMPEData(string zoneName)
    {
        return await _geoZones.GetGeoZoneMPEPerformanceData(zoneName);
    }
    public async Task<IEnumerable<GeoZoneDockDoor>> GetDockDoorGeoZones()
    {
        return await _geoZones.GetDockDoor();
    }
    public async Task<IEnumerable<EmployeeInfo>> GetEmpSchedules()
    {
        return await Task.Run(_empSchedules.GetAll).ConfigureAwait(false);
    }
    public async Task<IEnumerable<CameraGeoMarker>> GetCameras()
    {
        return await Task.Run(_cameraMarkers.GetAll).ConfigureAwait(false);
    }

    //private Task string? GetUserName(ClaimsPrincipal? user) => user?.Identity?.Name;


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