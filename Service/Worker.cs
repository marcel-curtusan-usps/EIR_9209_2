using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using NuGet.Configuration;
using System.Collections.Concurrent;

public class Worker : BackgroundService, IHostedService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHubContext<HubServices> _hubServices;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IInMemoryConnectionRepository _connections;
    private readonly IInMemoryGeoZonesRepository _geoZones;
    private readonly IInMemoryTagsRepository _tags;
    private readonly ConcurrentDictionary<string, QPEEndpointService> _QPEendpointServices = new();
    private readonly ConcurrentDictionary<string, MPEWatchEndpointService> _MPEWatchendpointServices = new();
    private readonly ConcurrentDictionary<string, IDSEndpointService> _IDSendpointServices = new();

    public Worker(ILogger<Worker> logger, ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory,
        IInMemoryConnectionRepository connections,
        IInMemoryGeoZonesRepository geoZones,
         IInMemoryTagsRepository tags,
        IHubContext<HubServices> hubServices)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _geoZones = geoZones;
        _tags = tags;
        _httpClientFactory = httpClientFactory;
        _connections = connections;
        _loggerFactory = loggerFactory;
        _hubServices = hubServices;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        await Task.Delay(2000, stoppingToken);
        foreach (var endpoint in _connections.GetAll())
        {
            AddEndpoint(endpoint);
        }

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public void AddEndpoint(Connection endpointConfig)
    {
        //Quuppa Position Engine (QPE)
        if (endpointConfig.Name == "QPE")
        {

            if (_QPEendpointServices.ContainsKey(endpointConfig.Id))
            {
                _logger.LogWarning("Endpoint {Url} already exists.", endpointConfig.Id);
                return;
            }
            var _QPEendpointLogger = _loggerFactory.CreateLogger<QPEEndpointService>();
            var _QPEendpointService = new QPEEndpointService(_QPEendpointLogger, _httpClientFactory, endpointConfig, _connections, _tags, _hubServices);
            endpointConfig.Status = EWorkerServiceState.Starting;
            endpointConfig.LasttimeApiConnected = DateTime.Now;
            _QPEendpointServices[endpointConfig.Id] = _QPEendpointService;
            _QPEendpointService.Start();
        }
        //MPE Watch Engine
        if (endpointConfig.Name == "MPEWatch")
        {
            if (_MPEWatchendpointServices.ContainsKey(endpointConfig.Id))
            {
                _logger.LogWarning("Endpoint {Url} already exists.", endpointConfig.Id);
                return;
            }
            var _MPEWatchendpointLogger = _loggerFactory.CreateLogger<MPEWatchEndpointService>();
            var _MPEWatchendpointService = new MPEWatchEndpointService(_MPEWatchendpointLogger, _httpClientFactory, endpointConfig, _connections, _geoZones, _hubServices);
            endpointConfig.Status = EWorkerServiceState.Starting;
            endpointConfig.LasttimeApiConnected = DateTime.Now;
            _MPEWatchendpointServices[endpointConfig.Id] = _MPEWatchendpointService;
            _MPEWatchendpointService.Start();
        }
        //MPE Watch Engine
        if (endpointConfig.Name == "IDS")
        {
            if (_MPEWatchendpointServices.ContainsKey(endpointConfig.Id))
            {
                _logger.LogWarning("Endpoint {Url} already exists.", endpointConfig.Id);
                return;
            }
            var _IDSendpointLogger = _loggerFactory.CreateLogger<IDSEndpointService>();
            var _IDSendpointService = new IDSEndpointService(_IDSendpointLogger, _httpClientFactory, endpointConfig, _connections, _geoZones, _hubServices);
            endpointConfig.Status = EWorkerServiceState.Starting;
            endpointConfig.LasttimeApiConnected = DateTime.Now;
            _IDSendpointServices[endpointConfig.Id] = _IDSendpointService;
            _IDSendpointService.Start();
        }

        endpointConfig.Status = EWorkerServiceState.Starting;
        endpointConfig.LasttimeApiConnected = DateTime.Now;
        _logger.LogInformation("Started endpoint {Url}.", endpointConfig.Id);

        // Add or update in the repository
        _connections.Update(endpointConfig);
    }

    public void RemoveEndpoint(string id)
    {
        //if (_endpointServices.TryRemove(id, out var endpointService))
        //{
        //    endpointService.Stop();
        //    _logger.LogInformation("Stopped and removed endpoint {Url}.", id);

        //    // Remove from the repository
        //    _connections.Remove(id);
        //}
        //else
        //{
        //    _logger.LogWarning("Endpoint {Url} not found.", id);
        //}
    }

    public void UpdateEndpointInterval(Connection updateConfig)
    {
        //if (_endpointServices.TryGetValue(updateConfig.Id, out var endpointService))
        //{
        //    endpointService.UpdateInterval(updateConfig.MillisecondsInterval);

        //    _logger.LogInformation("Updated interval for endpoint {id} to {IntervalMilliseconds} Milliseconds.", updateConfig.Id, updateConfig.MillisecondsInterval);
        //}
        //else
        //{
        //    _logger.LogWarning("Endpoint {id} not found.", updateConfig.Id);
        //}
    }

    public void UpdateEndpointActive(Connection updateConfig)
    {
        //if (_endpointServices.TryGetValue(updateConfig.Id, out var endpointService))
        //{
        //    endpointService.UpdateActive(updateConfig.ActiveConnection);

        //    _logger.LogInformation("Updated active status for endpoint {id} to {active}.", updateConfig.Id, updateConfig.ActiveConnection);
        //}
        //else
        //{
        //    _logger.LogWarning("Endpoint {id} not found.", updateConfig.Id);
        //}
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker is stopping.");

        //foreach (var service in _endpointServices.Values)
        //{
        //    service.Stop();
        //}

        await base.StopAsync(stoppingToken);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        return base.StartAsync(cancellationToken);
    }
}