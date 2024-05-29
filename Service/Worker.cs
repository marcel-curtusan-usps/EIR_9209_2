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
        private readonly ConcurrentDictionary<string, QPEEndpointService> _QPEendpointServices;
        private readonly ConcurrentDictionary<string, MPEWatchEndpointService> _MPEWatchendpointServices;
        private readonly ConcurrentDictionary<string, IDSEndpointService> _IDSendpointServices;
        private readonly ConcurrentDictionary<string, EmailEndpointService> _EmailendpointServices;

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
            _hubServices = hubServices;
            if (_QPEendpointServices == null)
            {
                _QPEendpointServices = new();
            }

            _MPEWatchendpointServices = new();
            _IDSendpointServices = new();
            _EmailendpointServices = new();
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
            if (endpointConfig.Name == "QPE")
            {

                if (_QPEendpointServices.ContainsKey(endpointConfig.Id))
                {
                    _logger.LogWarning("Endpoint {Url} already exists.", endpointConfig.Id);
                    return false;
                }
                var _QPEendpointLogger = _loggerFactory.CreateLogger<QPEEndpointService>();
                var _QPEendpointService = new QPEEndpointService(_QPEendpointLogger, _httpClientFactory, endpointConfig, _connections, _tags, _hubServices);

                _QPEendpointServices[endpointConfig.Id] = _QPEendpointService;
                _QPEendpointService.Start();
                return true;
            }
            //MPE Watch Engine
            else if (endpointConfig.Name == "MPEWatch")
            {
                if (_MPEWatchendpointServices.ContainsKey(endpointConfig.Id))
                {
                    _logger.LogWarning("Endpoint {Url} already exists.", endpointConfig.Id);
                    return false;
                }
                var _MPEWatchendpointLogger = _loggerFactory.CreateLogger<MPEWatchEndpointService>();
                var _MPEWatchendpointService = new MPEWatchEndpointService(_MPEWatchendpointLogger, _httpClientFactory, endpointConfig, _connections, _geoZones, _hubServices);

                _MPEWatchendpointServices[endpointConfig.Id] = _MPEWatchendpointService;
                _MPEWatchendpointService.Start();
            }
            //MPE Watch Engine
            else if (endpointConfig.Name == "IDS")
            {
                if (_IDSendpointServices.ContainsKey(endpointConfig.Id))
                {
                    _logger.LogWarning("Endpoint {Url} already exists.", endpointConfig.Id);
                    return false;
                }
                var _IDSendpointLogger = _loggerFactory.CreateLogger<IDSEndpointService>();
                var _IDSendpointService = new IDSEndpointService(_IDSendpointLogger, _httpClientFactory, endpointConfig, _connections, _geoZones, _hubServices);

                _IDSendpointServices[endpointConfig.Id] = _IDSendpointService;
                _IDSendpointService.Start();
                return true;
            }
            //MPE Watch Engine
            else if (endpointConfig.Name == "Email")
            {
                if (_EmailendpointServices.ContainsKey(endpointConfig.Id))
                {
                    _logger.LogWarning("Endpoint {Url} already exists.", endpointConfig.Id);
                    return false;
                }
                var _EmailendpointLogger = _loggerFactory.CreateLogger<EmailEndpointService>();
                var _EmailendpointService = new EmailEndpointService(_EmailendpointLogger, _httpClientFactory, endpointConfig, _connections, _geoZones, _hubServices);

                _EmailendpointServices[endpointConfig.Id] = _EmailendpointService;
                _EmailendpointService.Start();
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
            if (endpointConfig != null)
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
            else
            {
                return false;
            }
        }

        public bool UpdateEndpoint(Connection updateConfig)
        {

            if (updateConfig != null)
            {
                if (updateConfig.Name == "QPE")
                {
                    _QPEendpointServices[updateConfig.Id].Update(updateConfig);
                    return true;

                }
                else if (updateConfig.Name == "IDS")
                {
                    _IDSendpointServices[updateConfig.Id].Update(updateConfig);
                    return true;

                }
                else if (updateConfig.Name == "MPEWatch")
                {
                    _MPEWatchendpointServices[updateConfig.Id].Update(updateConfig);
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