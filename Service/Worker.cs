using EIR_9209_2.Service;
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
    private readonly IInMemoryConnectionRepository _repository;
    private readonly ConcurrentDictionary<string, EndpointService> _endpointServices = new();

    public Worker(ILogger<Worker> logger, ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory, IInMemoryConnectionRepository repository, IHubContext<HubServices> hubServices)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _repository = repository;
        _loggerFactory = loggerFactory;
        _hubServices = hubServices;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        await Task.Delay(2000, stoppingToken);
        foreach (var endpoint in _repository.GetAll())
        {
            AddEndpoint(endpoint);
        }

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public void AddEndpoint(Connection endpointConfig)
    {
        if (_endpointServices.ContainsKey(endpointConfig.Id))
        {
            _logger.LogWarning("Endpoint {Url} already exists.", endpointConfig.Id);
            return;
        }

        var endpointLogger = _loggerFactory.CreateLogger<EndpointService>();
        var endpointService = new EndpointService(endpointLogger, _httpClientFactory, endpointConfig, _hubServices);
        _endpointServices[endpointConfig.Id] = endpointService;
        endpointService.Start();
        _logger.LogInformation("Started endpoint {Url}.", endpointConfig.Id);

        // Add or update in the repository
        _repository.Update(endpointConfig);
    }

    public void RemoveEndpoint(string id)
    {
        if (_endpointServices.TryRemove(id, out var endpointService))
        {
            endpointService.Stop();
            _logger.LogInformation("Stopped and removed endpoint {Url}.", id);

            // Remove from the repository
            _repository.Remove(id);
        }
        else
        {
            _logger.LogWarning("Endpoint {Url} not found.", id);
        }
    }

    public void UpdateEndpointInterval(Connection updateConfig)
    {
        if (_endpointServices.TryGetValue(updateConfig.Id, out var endpointService))
        {
            endpointService.UpdateInterval(updateConfig.MillisecondsInterval);

            _logger.LogInformation("Updated interval for endpoint {id} to {IntervalMilliseconds} Milliseconds.", updateConfig.Id, updateConfig.MillisecondsInterval);
        }
        else
        {
            _logger.LogWarning("Endpoint {id} not found.", updateConfig.Id);
        }
    }

    public void UpdateEndpointActive(Connection updateConfig)
    {
        if (_endpointServices.TryGetValue(updateConfig.Id, out var endpointService))
        {
            endpointService.UpdateActive(updateConfig.ActiveConnection);

            _logger.LogInformation("Updated active status for endpoint {id} to {active}.", updateConfig.Id, updateConfig.ActiveConnection);
        }
        else
        {
            _logger.LogWarning("Endpoint {id} not found.", updateConfig.Id);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker is stopping.");

        foreach (var service in _endpointServices.Values)
        {
            service.Stop();
        }

        await base.StopAsync(stoppingToken);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        return base.StartAsync(cancellationToken);
    }
}