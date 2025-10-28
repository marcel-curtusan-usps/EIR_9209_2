using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace EIR_9209_2.Service
{
    /// <summary>
    /// Base class for endpoint services that provides common functionality for managing endpoints.
    /// </summary>
    public abstract class BaseEndpointService : IDisposable
    {
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly IConfiguration _configuration;
        protected readonly IHubContext<HubServices> _hubContext;
        protected readonly IInMemoryConnectionRepository _connection;
        protected readonly ILoggerService _loggerService;
        protected Connection _endpointConfig;
        private CancellationTokenSource _cancellationTokenSource = new();
        private Task? _task = null;
        private PeriodicTimer? _timer = null;

        protected BaseEndpointService(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, ILoggerService loggerService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _hubContext = hubContext;
            _connection = connection;
            _endpointConfig = endpointConfig;
            _loggerService = loggerService;
        }

        public Task<bool> Start()
        {
            if (_task == null || _task.IsCompleted)
            {
                _cancellationTokenSource = new CancellationTokenSource();

                if (_endpointConfig.ActiveConnection)
                {
                    _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
                    _task = Task.Run(async () => await RunAsync(_cancellationTokenSource.Token).ConfigureAwait(false));
                }
                else
                {
                    _endpointConfig.Status = EWorkerServiceState.InActive;
                    _hubContext.Clients.Group("Connections").SendAsync("updateConnection", _endpointConfig).ConfigureAwait(false);
                }
                return Task.FromResult(true); // Service started successfully
            }
            return Task.FromResult(false); // Service could not be started
        }
        /// <summary>
        ///     Stops the endpoint service.
        /// </summary>
        /// <param name="restart"></param>
        /// <returns></returns>
        public async Task Stop(bool restart = false)
        {
            _cancellationTokenSource.Cancel();
            if (_task != null)
            {
                await _task.ContinueWith(async t =>
                {
                    if (restart)
                    {
                        await Start();
                    }
                });
            }
            else if (restart)
            {
                await Start();
            }
        }
        /// <summary>
        /// Removes the endpoint service.
        /// </summary>
        /// <param name="updateCon"></param>
        /// <returns></returns>
        public async Task<bool> Update(Connection updateCon)
        {
            try
            {
                await Stop(restart: false);
                _endpointConfig.MillisecondsInterval = updateCon.MillisecondsInterval;
                _endpointConfig.Hostname = updateCon.Hostname;
                _endpointConfig.IpAddress = updateCon.IpAddress;
                _endpointConfig.HoursBack = updateCon.HoursBack;
                _endpointConfig.HoursForward = updateCon.HoursForward;
                _endpointConfig.ActiveConnection = updateCon.ActiveConnection;
                _endpointConfig.DbType = updateCon.DbType;
                _endpointConfig.Url = updateCon.Url;
                _endpointConfig.ConnectionString = updateCon.ConnectionString;
                _endpointConfig.OAuthUrl = updateCon.OAuthUrl;
                _endpointConfig.OAuthClientId = updateCon.OAuthClientId;
                _endpointConfig.OAuthPassword = updateCon.OAuthPassword;
                _endpointConfig.OAuthUserName = updateCon.OAuthUserName;
                _endpointConfig.TenantId = updateCon.TenantId;
                _endpointConfig.MapId = updateCon.MapId;
                _endpointConfig.OutgoingApikey = updateCon.OutgoingApikey;
                _endpointConfig.MillisecondsTimeout = updateCon.MillisecondsTimeout;
                _endpointConfig.LogData = updateCon.LogData;
                _endpointConfig.ApiConnected = false;
                _endpointConfig.WebhookConnection = updateCon.WebhookConnection;
                _endpointConfig.WebhookUrl = updateCon.WebhookUrl;
                _endpointConfig.WebhookUserName = updateCon.WebhookUserName;
                _endpointConfig.WebhookPassword = updateCon.WebhookPassword;


                if (updateCon.ActiveConnection)
                {
                    await Start();
                    _endpointConfig.Status = EWorkerServiceState.Running;
                    await _connection.Update(_endpointConfig);
                    return true; // Successfully updated the connection
                }
                else
                {
                    _endpointConfig.Status = EWorkerServiceState.InActive;
                    await _connection.Update(_endpointConfig);
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", _endpointConfig, CancellationToken.None);
                    return false;
                }
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private async Task RunAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (await _timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
                {
                    _endpointConfig.Status = EWorkerServiceState.Running;
                    //_endpointConfig.LasttimeApiConnected = DateTime.MinValue;
                    _endpointConfig.ApiConnected = true;
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", _endpointConfig, CancellationToken.None).ConfigureAwait(false);

                    await FetchDataFromEndpoint(stoppingToken).ConfigureAwait(false);
                    if (_timer.Period.TotalMilliseconds != _endpointConfig.MillisecondsInterval)
                    {
                        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_endpointConfig.MillisecondsInterval));
                    }
                    _endpointConfig.LasttimeApiConnected = DateTime.Now;
                    //_endpointConfig.Status = EWorkerServiceState.Idel;
                    //await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", _endpointConfig, CancellationToken.None).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException ex)
            {
                await _loggerService.LogData(new JObject { ["message"] = $"Stopping data collection for {_endpointConfig.Url}" }, "Error", ex.Message, _endpointConfig.Url);
                _endpointConfig.Status = EWorkerServiceState.Stopped;
                _endpointConfig.ApiConnected = false;
                // Notify clients about the stopped status without terminating the connection
                await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", _endpointConfig, CancellationToken.None).ConfigureAwait(false);
            }
            finally
            {
                _timer?.Dispose();
            }
        }
        /// <summary>
        /// Fetches data from the endpoint.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected abstract Task FetchDataFromEndpoint(CancellationToken stoppingToken);
        internal static string BuildUrl(string template, Dictionary<string, string> parameters)
        {
            foreach (var kvp in parameters)
            {
                template = template.Replace($"{{{kvp.Key}}}", kvp.Value);
            }
            return template;
        }
        internal static JsonSerializerSettings jsonSettings = new()
        {
            Error = (sender, args) =>
            {
                Console.WriteLine($"Json Error: {args.ErrorContext.Error.Message} at path [{args.ErrorContext.Path}] " +
                    $"on original object:{Environment.NewLine}{JsonConvert.SerializeObject(args.CurrentObject)}");
                args.ErrorContext.Handled = true;
            },
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Formatting = Formatting.Indented

        };
        /// <summary>
        /// Disposes the resources used by the endpoint service.
        /// </summary>
        public async void Dispose()
        {
            // Cleanup resources
            await Stop();
            _timer?.Dispose();
            _cancellationTokenSource?.Dispose();
            _task = null;
        }
    }
}