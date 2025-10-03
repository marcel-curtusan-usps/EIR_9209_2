using EIR_9209_2.DatabaseCalls.IDS;
using EIR_9209_2.DataStore;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace EIR_9209_2.Service
{
    /// <summary>
    /// Worker class that manages various endpoint services.
    /// It runs in the background and handles the lifecycle of endpoint services.
    /// </summary>
    /// <summary>
    /// Worker class that manages the lifecycle and operations of endpoint services in the background.
    /// </summary>
    public class Worker : BackgroundService, IWorker
    {
        // Logger for diagnostic and operational messages
        private readonly ILogger<Worker> _logger;
        // Factory for creating HttpClient instances
        private readonly IHttpClientFactory _httpClientFactory;
        // SignalR hub context for real-time communication
        private readonly IHubContext<HubServices> _hubServices;
        // Logger factory for creating loggers for endpoint services
        private readonly ILoggerFactory _loggerFactory;
        // Repository for managing connection data in memory
        private IInMemoryConnectionRepository _connections = null!;
        // Repository for geo zones
        private readonly IInMemoryGeoZonesRepository _geoZones = null!;
        // Repository for tags
        private readonly IInMemoryTagsRepository _tags = null!;
        // Repository for email data
        private readonly IInMemoryEmailRepository _email = null!;
        // Repository for site information
        private readonly IInMemorySiteInfoRepository _siteInfo = null!;
        // Repository for employee data
        private readonly IInMemoryEmployeesRepository _employees = null!;
        // Repository for employee schedules
        private readonly IInMemoryEmployeesSchedule _schedule = null!;
        // Repository for camera data
        private readonly IInMemoryCamerasRepository _cameras = null!;
        // Application configuration
        private readonly IConfiguration _configuration = null!;
        // Repository for background images
        private readonly IInMemoryBackgroundImageRepository _backgroundImage = null!;
        // Service for logging custom events
        private readonly ILoggerService _loggerService = null!;
        // IDS service for intrusion detection
        private readonly IIDS _ids = null!;
        // Service provider for dependency injection scopes
        private readonly IServiceProvider _serviceProvider = null!;
        // Thread-safe dictionary to manage endpoint services by ID
        private readonly ConcurrentDictionary<string, BaseEndpointService> _endPointServices = new();
        // Task completion source to signal when services are ready
        private readonly TaskCompletionSource<bool> _servicesReady = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Worker"/> class with all required dependencies.
        /// </summary>
        public Worker(
            ILogger<Worker> logger,
            ILoggerFactory loggerFactory,
            IHttpClientFactory httpClientFactory,
            IHubContext<HubServices> hubServices,
            IInMemoryGeoZonesRepository geoZones,
            IInMemoryTagsRepository tags,
            IInMemoryEmailRepository email,
            IInMemorySiteInfoRepository siteInfo,
            IInMemoryEmployeesRepository employees,
            IInMemoryCamerasRepository cameras,
            IConfiguration configuration,
            IInMemoryBackgroundImageRepository backgroundImage,
            ILoggerService loggerService,
            IIDS ids,
            IInMemoryEmployeesSchedule schedule,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _hubServices = hubServices;
            _loggerFactory = loggerFactory;
            _geoZones = geoZones;
            _tags = tags;
            _cameras = cameras;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _email = email;
            _siteInfo = siteInfo;
            _employees = employees;
            _backgroundImage = backgroundImage;
            _loggerService = loggerService;
            _ids = ids;
            _schedule = schedule;
            _serviceProvider = serviceProvider;
        }
        /// <summary>
        /// Signals that the worker information is ready by setting the result of the associated task.
        /// </summary>
        public void SignalWorkerReady()
        {
            _servicesReady.TrySetResult(true);
        }

        /// <summary>
        /// Initializes the worker and signals that services are ready.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _servicesReady.Task; // Wait for Services to be ready
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Create a new DI scope for each iteration
                    using var innerScope = _serviceProvider.CreateScope();
                    _connections = innerScope.ServiceProvider.GetRequiredService<IInMemoryConnectionRepository>();
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(2000, stoppingToken);
                    // Add all endpoints from the repository
                    foreach (var endpoint in await _connections.GetAll())
                    {
                        await AddEndpoint(endpoint);
                    }
                    // Wait indefinitely until cancellation is requested
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Worker task was canceled (application is shutting down).");
            }
        }

        /// <summary>
        /// Adds and starts a new endpoint service based on the provided configuration.
        /// </summary>
        /// <param name="endpointConfig">The configuration for the endpoint to add.</param>
        /// <returns>True if the endpoint was added and started; otherwise, false.</returns>
        public async Task<bool> AddEndpoint(Connection endpointConfig)
        {
            // Check if the endpoint already exists
            if (_endPointServices.ContainsKey(endpointConfig.Id))
            {
                _logger.LogWarning("Endpoint {Id} already exists.", endpointConfig.Id);
                return false;
            }
            BaseEndpointService endpointService;
            // Select the correct endpoint service implementation based on the endpoint name
            switch (endpointConfig.Name)
            {
                case "QPE":
                    endpointService = new QPEEndPointServices(_loggerFactory.CreateLogger<QPEEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _loggerService, _tags, _geoZones, _backgroundImage);
                    break;
                case "QRE":
                    endpointService = new QREEndPointServices(_loggerFactory.CreateLogger<QREEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _loggerService, _geoZones, _tags, _employees, _schedule, _siteInfo);
                    break;
                case "MPEWatch":
                    endpointService = new MPEWatchEndPointServices(_loggerFactory.CreateLogger<MPEWatchEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _loggerService, _geoZones);
                    break;
                case "IDS":
                    endpointService = new IDSEndPointServices(_loggerFactory.CreateLogger<IDSEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _loggerService, _geoZones, _ids, _siteInfo);
                    break;
                case "Email":
                    endpointService = new EmailEndPointServices(_loggerFactory.CreateLogger<EmailEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _loggerService, _email);
                    break;
                case "SV":
                    endpointService = new SVEndPointServices(_loggerFactory.CreateLogger<SVEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _loggerService, _geoZones, _siteInfo);
                    break;
                case "SMS_Wrapper":
                    endpointService = new SMSWrapperEndPointServices(_loggerFactory.CreateLogger<SMSWrapperEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _loggerService, _employees, _siteInfo);
                    break;
                case "HCES":
                    endpointService = new HCESEndPointServices(_loggerFactory.CreateLogger<HCESEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _loggerService, _siteInfo, _employees);
                    break;
                case "IVES":
                    endpointService = new IVESEndPointServices(_loggerFactory.CreateLogger<SMSWrapperEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _loggerService, _siteInfo, _employees, _schedule);
                    break;
                case "CiscoSpaces":
                    endpointService = new CiscoSpacesEndPointServices(_loggerFactory.CreateLogger<CiscoSpacesEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _loggerService, _tags, _backgroundImage);
                    break;
                case "Web_Camera":
                    endpointService = new CameraEndPointServices(_loggerFactory.CreateLogger<CiscoSpacesEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _loggerService, _siteInfo, _cameras);
                    break;
                default:
                    _logger.LogWarning("Unknown endpoint {Name}", endpointConfig.Name);
                    return false;
            }
            // Add the endpoint service to the dictionary and start it
            _endPointServices[endpointConfig.Id] = endpointService;
            await endpointService.Start();
            _logger.LogInformation("Started endpoint {Id}.", endpointConfig.Id);
            return true;
        }

        /// <summary>
        /// Removes and stops an endpoint service by its configuration.
        /// </summary>
        /// <param name="endpointConfig">The configuration of the endpoint to remove.</param>
        /// <returns>True if the endpoint was found and removed; otherwise, false.</returns>
        public Task<bool> RemoveEndpoint(Connection endpointConfig)
        {
            if (_endPointServices.TryRemove(endpointConfig.Id, out var endpointService))
            {
                // Optionally, call a Stop method on endpointService if available
                _logger.LogInformation("Stopped and removed endpoint {Id}.", endpointConfig.Id);
                return Task.FromResult(true);
            }
            else
            {
                _logger.LogWarning("Endpoint {Id} not found.", endpointConfig.Id);
                return Task.FromResult(false);
            }
        }
        /// <summary>
        /// Updates the configuration of an existing endpoint service.
        /// </summary>
        /// <param name="updateConfig">The new configuration for the endpoint.</param>
        /// <returns>True if the endpoint was found and updated; otherwise, false.</returns>
        public async Task<bool> UpdateEndpoint(Connection updateConfig)
        {
            if (_endPointServices.TryGetValue(updateConfig.Id, out var endpointService))
            {
                await endpointService.Update(updateConfig);
                _logger.LogInformation("Updated Configuration for endpoint {Id}", updateConfig.Id);
                return true;
            }
            else
            {
                _logger.LogWarning("Endpoint {Url} not found.", updateConfig.Id);
                return false;
            }
        }
        /// <summary>
        /// Deactivates all endpoints by updating their status and removing them from the active services.
        /// </summary>
        /// <returns>True if all endpoints were deactivated successfully; otherwise, false.</returns>
        public async Task<bool> DeactivateAllEndpoints()
        {
            try
            {
                // Iterate through all endpoints and deactivate each one
                foreach (var endpoint in await _connections.GetAll())
                {
                    if (_endPointServices.TryGetValue(endpoint.Id, out var endpointService))
                    {
                        endpoint.ActiveConnection = false;
                        if (await UpdateEndpoint(endpoint))
                        {
                            await RemoveEndpoint(endpoint);
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return false;
            }
        }
    }
}