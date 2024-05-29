using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using NuGet.Configuration;
using System.Collections.Concurrent;

namespace EIR_9209_2.Service
{
    public class Worker : BackgroundService, IWorker
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

        public bool AddEndpoint(Connection endpointConfig)
        {
            //Quuppa Position Engine (QPE)
            if (endpointConfig.Name == "QPE" && endpointConfig.ActiveConnection)
            {

                if (_QPEendpointServices.ContainsKey(endpointConfig.Id))
                {
                    _logger.LogWarning("Endpoint {Url} already exists.", endpointConfig.Id);
                    return false;
                }
                var _QPEendpointLogger = _loggerFactory.CreateLogger<QPEEndpointService>();
                var _QPEendpointService = new QPEEndpointService(_QPEendpointLogger, _httpClientFactory, endpointConfig, _connections, _tags, _hubServices);
                endpointConfig.Status = EWorkerServiceState.Starting;
                endpointConfig.LasttimeApiConnected = DateTime.Now;
                _QPEendpointServices[endpointConfig.Id] = _QPEendpointService;
                _QPEendpointService.Start();
                return true;
            }
            //MPE Watch Engine
            else if (endpointConfig.Name == "MPEWatch" && endpointConfig.ActiveConnection)
            {
                if (_MPEWatchendpointServices.ContainsKey(endpointConfig.Id))
                {
                    _logger.LogWarning("Endpoint {Url} already exists.", endpointConfig.Id);
                    return false;
                }
                var _MPEWatchendpointLogger = _loggerFactory.CreateLogger<MPEWatchEndpointService>();
                var _MPEWatchendpointService = new MPEWatchEndpointService(_MPEWatchendpointLogger, _httpClientFactory, endpointConfig, _connections, _geoZones, _hubServices);
                endpointConfig.Status = EWorkerServiceState.Starting;
                endpointConfig.LasttimeApiConnected = DateTime.Now;
                _MPEWatchendpointServices[endpointConfig.Id] = _MPEWatchendpointService;
                _MPEWatchendpointService.Start();
            }
            //MPE Watch Engine
            else if (endpointConfig.Name == "IDS" && endpointConfig.ActiveConnection)
            {
                if (_MPEWatchendpointServices.ContainsKey(endpointConfig.Id))
                {
                    _logger.LogWarning("Endpoint {Url} already exists.", endpointConfig.Id);
                    return false; 
                }
                var _IDSendpointLogger = _loggerFactory.CreateLogger<IDSEndpointService>();
                var _IDSendpointService = new IDSEndpointService(_IDSendpointLogger, _httpClientFactory, endpointConfig, _connections, _geoZones, _hubServices);
                endpointConfig.Status = EWorkerServiceState.Starting;
                endpointConfig.LasttimeApiConnected = DateTime.Now;
                _IDSendpointServices[endpointConfig.Id] = _IDSendpointService;
                _IDSendpointService.Start();
                return true;
            }
            else
            {
                return false;
                //endpointConfig.Status = EWorkerServiceState.InActive;
                //endpointConfig.LasttimeApiConnected = DateTime.Now;
                //_logger.LogInformation("Endpoint {ID} is {status}.", endpointConfig.Id, endpointConfig.Status);

                //// Add or update in the repository
                //_connections.Update(endpointConfig);
            }
            return false;
        }

        public bool RemoveEndpoint(Connection endpointConfig)
        {
            if (endpointConfig.Name == "QPE")
            {
                _QPEendpointServices[endpointConfig.Id].Stop();
                if (_QPEendpointServices.TryRemove(endpointConfig.Id, out var endpointService))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (endpointConfig.Name == "IDS")
            {
                _IDSendpointServices[endpointConfig.Id].Stop();
                if (_IDSendpointServices.TryRemove(endpointConfig.Id, out var endpointService))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (endpointConfig.Name == "MPEWatch")
            {
                _MPEWatchendpointServices[endpointConfig.Id].Stop();
                if (_MPEWatchendpointServices.TryRemove(endpointConfig.Id, out var endpointService))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
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
}