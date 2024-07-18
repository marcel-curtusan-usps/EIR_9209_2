using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http;

namespace EIR_9209_2.Service
{
    public abstract class BaseEndpointService(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection) : IDisposable
    {
        protected readonly ILogger<BaseEndpointService> _logger = logger;
        protected readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        protected readonly IConfiguration _configuration = configuration;
        protected readonly IHubContext<HubServices> _hubContext = hubContext;
        protected readonly IInMemoryConnectionRepository _connection = connection;
        protected Connection _endpointConfig = endpointConfig;
        private CancellationTokenSource _cancellationTokenSource = new();
        private Task? _task = null;
        private PeriodicTimer? _timer = null;

        public async void Start()
        {
            if (_task == null || _task.IsCompleted)
            {
                _cancellationTokenSource = new CancellationTokenSource();

                if (_endpointConfig.ActiveConnection)
                {
                    _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
                    _task = Task.Run(async () => await RunAsync(_cancellationTokenSource.Token));
                }
                else
                {
                    _endpointConfig.Status = EWorkerServiceState.InActive;
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", _endpointConfig);

                }
            }
        }

        public void Stop()
        {
            if (_task != null && !_task.IsCompleted)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        public async void Update(Connection updateCon)
        {
            Stop();
            _endpointConfig.MillisecondsInterval = updateCon.MillisecondsInterval;
            _endpointConfig.HoursBack = updateCon.HoursBack;
            _endpointConfig.HoursForward = updateCon.HoursForward;
            _endpointConfig.ActiveConnection = updateCon.ActiveConnection;
            _endpointConfig.Url = updateCon.Url;
            _endpointConfig.OAuthUrl = updateCon.OAuthUrl;
            _endpointConfig.OAuthClientId = updateCon.OAuthClientId;
            _endpointConfig.OAuthPassword = updateCon.OAuthPassword;
            _endpointConfig.OAuthUserName = updateCon.OAuthUserName;

            if (updateCon.ActiveConnection)
            {
                Start();
                _endpointConfig.Status = EWorkerServiceState.Running;
                _ = _connection.Update(_endpointConfig).Result;

            }
            else
            {
                _endpointConfig.Status = EWorkerServiceState.InActive;
                _ = _connection.Update(_endpointConfig).Result;
                await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", _endpointConfig);

            }

        }

        private async Task RunAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (await _timer.WaitForNextTickAsync(stoppingToken))
                {
                    await FetchDataFromEndpoint(stoppingToken);
                    if (_timer.Period.TotalMilliseconds != _endpointConfig.MillisecondsInterval)
                    {
                        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_endpointConfig.MillisecondsInterval));
                    }
                    _endpointConfig.Status = EWorkerServiceState.Idel;
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", _endpointConfig);
                }

            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Stopping data collection for {Url}", _endpointConfig.Url);
            }
            finally
            {
                _timer.Dispose();
            }
        }

        protected abstract Task FetchDataFromEndpoint(CancellationToken stoppingToken);
        internal static JsonSerializerSettings jsonSettings = new()
        {
            //keep this
            Error = (sender, args) =>
            {
                //in fotf this should log to an actual log file for diagnostics
                Console.WriteLine($"Json Error: {args.ErrorContext.Error.Message} at path [{args.ErrorContext.Path}] " +
                    $"on original object:{Environment.NewLine}{JsonConvert.SerializeObject(args.CurrentObject)}");
                //keep this
                args.ErrorContext.Handled = true;
            },
            //keep this
            ContractResolver = new DefaultContractResolver
            {
                //keep this
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };
        /// <summary>
        /// fix this later to use the correct http client
        /// </summary>
        /// <returns></returns>
        protected HttpClient CreateHttpClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(_endpointConfig.TimeoutSeconds);
            return client;
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}