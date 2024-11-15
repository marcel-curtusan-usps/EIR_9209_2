using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;

namespace EIR_9209_2.Service
{
    public class QPEEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryTagsRepository _tags;
        private readonly IInMemoryGeoZonesRepository _zones;
        private readonly IInMemoryBackgroundImageRepository _backgroundImage;

        public QPEEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, ILoggerService loggerService, IInMemoryTagsRepository tags, IInMemoryGeoZonesRepository zones, IInMemoryBackgroundImageRepository backgroundImage)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection, loggerService)
        {
            _tags = tags;
            _zones = zones;
            _backgroundImage = backgroundImage;
        }

        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                IQueryService queryService;
                string server = string.IsNullOrEmpty(_endpointConfig.IpAddress) ? _endpointConfig.Hostname : _endpointConfig.IpAddress;
                string formatUrl = string.Format(_endpointConfig.Url, server, _endpointConfig.MessageType);
                queryService = new QueryService(_logger, _httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(formatUrl), new TimeSpan(0, 0, 0, 0, _endpointConfig.MillisecondsTimeout)));
              
                if (_endpointConfig.MessageType.Equals("getTagData", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Process tag data in a separate thread
                    var result = await queryService.GetQPETagData(stoppingToken);
                    if (_endpointConfig.LogData)
                    {
                        // Start a new thread to handle the logging
                        Task.Run(() => _loggerService.LogData(result.ToJson(),
                            _endpointConfig.MessageType,
                            _endpointConfig.Name,
                            formatUrl), stoppingToken);
                    }
                    await ProcessQPETagData(result, stoppingToken);
                }
                if (_endpointConfig.MessageType.Equals("getProjectInfo", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Process tag data in a separate thread
                    var result = await queryService.GetQPEProjectInfo(stoppingToken);
                    // Start a new thread to handle the logging
                    Task.Run(() => _loggerService.LogData(result.ToJson(),
                        _endpointConfig.MessageType,
                        _endpointConfig.Name,
                        formatUrl), stoppingToken);
                    await ProcessQPEProjectInfo(result, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
                _endpointConfig.ApiConnected = false;
                _endpointConfig.Status = EWorkerServiceState.ErrorPullingData;
                var updateCon = await _connection.Update(_endpointConfig).ConfigureAwait(false);
                if (updateCon != null)
                {
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None).ConfigureAwait(false);
                }
            }
        }

        private async Task ProcessQPEProjectInfo(QPEProjectInfo result, CancellationToken stoppingToken)
        {
            try
            {
                await _zones.ProcessQPEGeoZone(result.coordinateSystems, stoppingToken);
                await _backgroundImage.ProcessQPEBackgroundImage(result.coordinateSystems, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing QPE tag data");
            }
        }

        private async Task ProcessQPETagData(QuuppaTag result, CancellationToken stoppingToken)
        {
            try
            {
                if (result?.Tags != null)
                {
                  await _tags.UpdateTagQPEInfo(result.Tags, result.ResponseTS, stoppingToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing QPE tag data");
            }
        }
    }
}
