using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    public class QPEEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryTagsRepository _tags;

        public QPEEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, IInMemoryTagsRepository tags)
            : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection)
        {
            _tags = tags;
        }

        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                IQueryService queryService;

                // Process tag data
                string formatUrl = string.Format(_endpointConfig.Url, _endpointConfig.MessageType);
                queryService = new QueryService(_logger, _httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(formatUrl)));
                var result = await queryService.GetQPETagData(stoppingToken).ConfigureAwait(false);

                if (_endpointConfig.MessageType == "getTagData")
                {
                    // Process tag data in a separate thread
                    _ = Task.Run(() => ProcessQPETagData(result), stoppingToken).ConfigureAwait(false);
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
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, cancellationToken: stoppingToken).ConfigureAwait(false);
                }
            }
        }

        private async Task ProcessQPETagData(QuuppaTag result)
        {
            try
            {
                if (result?.Tags != null)
                {
                    await Task.Run(() => _tags.UpdateTagQPEInfo(result.Tags, result.ResponseTS)).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing QPE tag data");
            }
        }
    }
}
