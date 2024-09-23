using EIR_9209_2.DataStore;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace EIR_9209_2.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHubContext<HubServices> _hubServices;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IInMemoryConnectionRepository _connections;
        private readonly IInMemoryGeoZonesRepository _geoZones;
        private readonly IInMemoryTagsRepository _tags;
        private readonly IInMemoryEmailRepository _email;
        private readonly IInMemorySiteInfoRepository _siteInfo;
        private readonly IInMemoryEmployeesRepository _employees;
        private readonly IInMemoryCamerasRepository _cameras;
        private readonly IConfiguration _configuration;
        private readonly IInMemoryBackgroundImageRepository _backgroundImage;
        private readonly ConcurrentDictionary<string, BaseEndpointService> _endPointServices = new();

        public Worker(ILogger<Worker> logger, ILoggerFactory loggerFactory,
            IHttpClientFactory httpClientFactory,
            IInMemoryConnectionRepository connections,
            IHubContext<HubServices> hubServices,
            IInMemoryGeoZonesRepository geoZones,
            IInMemoryTagsRepository tags,
            IInMemoryEmailRepository email,
            IInMemorySiteInfoRepository siteInfo,
            IInMemoryEmployeesRepository employees,
            IInMemoryCamerasRepository cameras,
            IConfiguration configuration, 
            IInMemoryBackgroundImageRepository backgroundImage)
        {
            _logger = logger;
            _hubServices = hubServices;
            _loggerFactory = loggerFactory;
            _geoZones = geoZones;
            _tags = tags;
            _cameras = cameras;
            _httpClientFactory = httpClientFactory;
            _connections = connections;
            _configuration = configuration;
            _email = email;
            _siteInfo = siteInfo;
            _employees = employees;
            _backgroundImage = backgroundImage;
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

        public bool AddEndpoint(Connection endpointConfig)
        {
            if (_endPointServices.ContainsKey(endpointConfig.Id))
            {
                _logger.LogWarning("Endpoint {Id} already exists.", endpointConfig.Id);
                return false;
            }
            BaseEndpointService endpointService;
            switch (endpointConfig.Name)
            {
                case "QPE":
                    endpointService = new QPEEndPointServices(_loggerFactory.CreateLogger<QPEEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _tags, _geoZones, _backgroundImage);
                    break;
                case "QRE":
                    endpointService = new QREEndPointServices(_loggerFactory.CreateLogger<QREEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _geoZones, _tags, _employees, _siteInfo);
                    break;
                case "MPEWatch":
                    endpointService = new MPEWatchEndPointServices(_loggerFactory.CreateLogger<MPEWatchEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _geoZones);
                    break;
                case "IDS":
                    endpointService = new IDSEndPointServices(_loggerFactory.CreateLogger<IDSEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _geoZones);
                    break;
                case "Email":
                    endpointService = new EmailEndPointServices(_loggerFactory.CreateLogger<EmailEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _email);
                    break;
                case "SV":
                    endpointService = new SVEndPointServices(_loggerFactory.CreateLogger<SVEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _geoZones, _siteInfo);
                    break;
                case "SMS_Wrapper":
                    endpointService = new SMSWrapperEndPointServices(_loggerFactory.CreateLogger<SMSWrapperEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _tags, _siteInfo);
                    break;
                case "IVES":
                    endpointService = new IVESEndPointServices(_loggerFactory.CreateLogger<SMSWrapperEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _siteInfo, _employees);
                    break;
                case "CiscoSpaces":
                    endpointService = new CiscoSpacesEndPointServices(_loggerFactory.CreateLogger<CiscoSpacesEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _tags);
                    break;
                case "Web_Camera":
                    endpointService = new CameraEndPointServices(_loggerFactory.CreateLogger<CiscoSpacesEndPointServices>(), _httpClientFactory, endpointConfig, _configuration, _hubServices, _connections, _siteInfo, _cameras);
                    break;
                default:
                    _logger.LogWarning("Unknown endpoint {Name}", endpointConfig.Name);
                    return false;

            }
            _endPointServices[endpointConfig.Id] = endpointService;
            endpointService.Start();
            _logger.LogInformation("Started endpoint {Id}.", endpointConfig.Id);
            return true;
        }

        public bool RemoveEndpoint(Connection endpointConfig)
        {
            if (_endPointServices.TryRemove(endpointConfig.Id, out var endpointService))
            {
                _logger.LogInformation("Stopped and removed endpoint {Id}.", endpointConfig.Id);
                return true;
            }
            else
            {
                _logger.LogWarning("Endpoint {I} not found.", endpointConfig.Id);
                return false;
            }
        }

        public bool UpdateEndpoint(Connection updateConfig)
        {
            if (_endPointServices.TryGetValue(updateConfig.Id, out var endpointService))
            {
                endpointService.Update(updateConfig);

                _logger.LogInformation("Updated Configuration for endpoint {Id}", updateConfig.Id);
                return true;
            }
            else
            {
                _logger.LogWarning("Endpoint {Url} not found.", updateConfig.Id);
                return false;
            }
        }
        public bool DeactivateAllEndpoints()
        {
            try
            {
                foreach (var endpoint in _connections.GetAll())
                {
                    if (_endPointServices.TryGetValue(endpoint.Id, out var endpointService))
                    {
                        endpoint.ActiveConnection = false;
                        if (UpdateEndpoint(endpoint))
                        {
                            RemoveEndpoint(endpoint);
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
}