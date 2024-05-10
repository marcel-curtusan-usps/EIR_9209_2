
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;

public class ConnectionBackgroundService : BackgroundService, IHostedService
{
    private ILogger<ConnectionBackgroundService> _logger;
    private HttpClient _httpClient;
    private Connection _serviceConfig;
    private PeriodicTimer _timer;
    private readonly IHubContext<HubServices> _hubContext;

    public ConnectionBackgroundService(ILogger<ConnectionBackgroundService> logger, HttpClient httpClient, Connection serviceConfig, IHubContext<HubServices> hubContext, BackgroundServiceManager serviceManager, string id)
    {
        _logger = logger;
        _httpClient = httpClient;
        _serviceConfig = serviceConfig;
        _hubContext = hubContext;
        // Add this instance to the service manager
        serviceManager.AddService(id, this);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(_serviceConfig.Interval));

        while (!stoppingToken.IsCancellationRequested && await _timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                if (_serviceConfig.ActiveConnection)
                {
                    // Fetch data from the specified URL
                    var response = await _httpClient.GetAsync(_serviceConfig.Url, stoppingToken);
                    response.EnsureSuccessStatusCode();

                    // Process the fetched data
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation(JsonConvert.SerializeObject(responseBody));

                    if (_serviceConfig.MessageType == "getTagData")
                    {
                        await _hubContext.Clients.Group(_serviceConfig.MessageType).SendAsync(_serviceConfig.MessageType, responseBody);

                    }
                    if (_serviceConfig.MessageType == "siteInfo")
                    {
                        await _hubContext.Clients.Group(_serviceConfig.MessageType).SendAsync(_serviceConfig.MessageType, responseBody);
                    }


                }
                else
                {
                    _logger.LogInformation($"Service is Active: {_serviceConfig.Name} Message Type: {_serviceConfig.MessageType}");
                    _logger.LogInformation($"Service: {_serviceConfig.Name} Will restart in {_serviceConfig.Interval} Seconds");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching data for {_serviceConfig.Name}.");
            }
        }
    }
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Add a delay before starting the service
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            // Start the timer
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(_serviceConfig.Interval));
            await base.StartAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while starting the service.");
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Add a delay before stopping the service
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            // Clean up any resources used by the background service
            _httpClient.Dispose();
            await base.StopAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while stopping the service.");
        }

    }
    public override void Dispose()
    {
        // Clean up any resources used by the background service
        _httpClient.Dispose();
        base.Dispose();
    }

}