using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using NuGet.Protocol;

namespace EIR_9209_2.Service
{
    /// <summary>
    /// This class is responsible for fetching data from the QPE endpoint and processing it.
    /// </summary>
    public class QPEEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryTagsRepository _tags;
        private readonly IInMemoryGeoZonesRepository _zones;
        private readonly IInMemoryBackgroundImageRepository _backgroundImage;
        /// <summary>
        /// This class is responsible for fetching data from the QPE endpoint and processing it.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="endpointConfig"></param>
        /// <param name="configuration"></param>
        /// <param name="hubContext"></param>
        /// <param name="connection"></param>
        /// <param name="loggerService"></param>
        /// <param name="tags"></param>
        /// <param name="zones"></param>
        /// <param name="backgroundImage"></param>
        public QPEEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, ILoggerService loggerService, IInMemoryTagsRepository tags, IInMemoryGeoZonesRepository zones, IInMemoryBackgroundImageRepository backgroundImage)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection, loggerService)
        {
            _tags = tags;
            _zones = zones;
            _backgroundImage = backgroundImage;
        }
        /// <summary>
        /// Fetches data from the endpoint based on the message type specified in the configuration.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
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
                        await _loggerService.LogData(result.ToJson(),
                            _endpointConfig.MessageType,
                            _endpointConfig.Name,
                            formatUrl);
                    }
                    _endpointConfig.Status = EWorkerServiceState.Idel;
                    var updateCon = _connection.Update(_endpointConfig).Result;
                    if (updateCon != null)
                    {
                        await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                    }
                    await ProcessQPETagData(result, stoppingToken);
                }
                if (_endpointConfig.MessageType.Equals("getProjectInfo", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Process tag data in a separate thread
                    var result = await queryService.GetQPEProjectInfo(stoppingToken);
                    // Start a new thread to handle the logging
                    if (_endpointConfig.LogData)
                    {
                        await _loggerService.LogData(result.ToJson(),
                        _endpointConfig.MessageType,
                        _endpointConfig.Name,
                        formatUrl);
                    }
                    _endpointConfig.Status = EWorkerServiceState.Idel;
                    var updateCon = _connection.Update(_endpointConfig).Result;
                    if (updateCon != null)
                    {
                        await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                    }
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
