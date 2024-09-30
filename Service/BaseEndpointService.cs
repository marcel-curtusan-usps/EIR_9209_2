using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EIR_9209_2.Service
{
    public abstract class BaseEndpointService : IDisposable
    {
        protected readonly ILogger<BaseEndpointService> _logger;
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly IConfiguration _configuration;
        protected readonly IHubContext<HubServices> _hubContext;
        protected readonly IInMemoryConnectionRepository _connection;
        protected Connection _endpointConfig;
        private CancellationTokenSource _cancellationTokenSource = new();
        private Task? _task = null;
        private PeriodicTimer? _timer = null;

        protected BaseEndpointService(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _hubContext = hubContext;
            _connection = connection;
            _endpointConfig = endpointConfig;
        }

        public void Start()
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
            }
        }

        public void Stop(bool restart = false)
        {
            if (_task != null && !_task.IsCompleted)
            {
                _cancellationTokenSource.Cancel();
                _task.ContinueWith(t =>
                {
                    if (restart)
                    {
                        Start();
                    }
                });
            }
        }

        public async Task Update(Connection updateCon)
        {
            Stop(restart: false);
            _endpointConfig.MillisecondsInterval = updateCon.MillisecondsInterval;
            _endpointConfig.Hostname = updateCon.Hostname;
            _endpointConfig.IpAddress = updateCon.IpAddress;
            _endpointConfig.HoursBack = updateCon.HoursBack;
            _endpointConfig.HoursForward = updateCon.HoursForward;
            _endpointConfig.ActiveConnection = updateCon.ActiveConnection;
            _endpointConfig.Url = updateCon.Url;
            _endpointConfig.OAuthUrl = updateCon.OAuthUrl;
            _endpointConfig.OAuthClientId = updateCon.OAuthClientId;
            _endpointConfig.OAuthPassword = updateCon.OAuthPassword;
            _endpointConfig.OAuthUserName = updateCon.OAuthUserName;
            _endpointConfig.TenantId = updateCon.TenantId;
            _endpointConfig.MapId = updateCon.MapId;
            _endpointConfig.OutgoingApikey = updateCon.OutgoingApikey;
            _endpointConfig.MillisecondsTimeout = updateCon.MillisecondsTimeout;
            _endpointConfig.ApiConnected = false;

            if (updateCon.ActiveConnection)
            {
                Start();
                _endpointConfig.Status = EWorkerServiceState.Running;
                await _connection.Update(_endpointConfig).ConfigureAwait(false);
            }
            else
            {
                _endpointConfig.Status = EWorkerServiceState.InActive;
                await _connection.Update(_endpointConfig).ConfigureAwait(false);
                await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", _endpointConfig, CancellationToken.None).ConfigureAwait(false);
            }
        }

        private async Task RunAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (await _timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
                {
                    _endpointConfig.Status = EWorkerServiceState.Running;
                    _endpointConfig.LasttimeApiConnected = DateTime.Now;
                    _endpointConfig.ApiConnected = true;
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", _endpointConfig, CancellationToken.None).ConfigureAwait(false);

                    await FetchDataFromEndpoint(stoppingToken).ConfigureAwait(false);
                    if (_timer.Period.TotalMilliseconds != _endpointConfig.MillisecondsInterval)
                    {
                        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_endpointConfig.MillisecondsInterval));
                    }
                    _endpointConfig.Status = EWorkerServiceState.Idel;
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", _endpointConfig, CancellationToken.None).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Stopping data collection for {Url}", _endpointConfig.Url);
                _endpointConfig.Status = EWorkerServiceState.Stopped;
                _endpointConfig.ApiConnected = false;
                // Notify clients about the stopped status without terminating the connection
                await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", _endpointConfig,CancellationToken.None).ConfigureAwait(false);
            }
            finally
            {
                _timer?.Dispose();
            }
        }

        protected abstract Task FetchDataFromEndpoint(CancellationToken stoppingToken);

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
            }
        };
        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _task?.Wait();
            _cancellationTokenSource.Dispose();
            _timer?.Dispose();
        }
    }
}