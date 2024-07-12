using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EIR_9209_2.Service
{
    public abstract class BaseEndpointService
    {
        protected readonly ILogger<BaseEndpointService> Logger;
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly IConfiguration _configuration;
        protected readonly IHubContext<HubServices> _hubContext;
        protected readonly IInMemoryConnectionRepository _connection;
        protected Connection _endpointConfig;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;
        private PeriodicTimer _timer;
        protected BaseEndpointService(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection)
        {
            Logger = logger;
            _httpClientFactory = httpClientFactory;
            _hubContext = hubContext;
            _endpointConfig = endpointConfig;
            _cancellationTokenSource = new CancellationTokenSource();
            _configuration = configuration;
            _connection = connection;

        }
        public async void Start()
        {
            if (_task == null || _task.IsCompleted)
            {
                _cancellationTokenSource = new CancellationTokenSource();

                if (_endpointConfig.ActiveConnection)
                {
                    _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
                    _task = Task.Run(async () => await RunAsync(_cancellationTokenSource.Token));
                    _endpointConfig.Status = EWorkerServiceState.Idel;
                    var updateCon = _connection.Update(_endpointConfig).Result;
                    if (updateCon != null)
                    {
                        await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon);
                    }

                }
                else
                {
                    _endpointConfig.Status = EWorkerServiceState.InActive;

                    var updateCon = _connection.Update(_endpointConfig).Result;
                    if (updateCon != null)
                    {
                        await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon);
                    }
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
                updateCon = _connection.Update(_endpointConfig).Result;
                if (updateCon != null)
                {
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon);
                }
            }
            else
            {
                _endpointConfig.Status = EWorkerServiceState.Idel;
                updateCon = _connection.Update(_endpointConfig).Result;
                if (updateCon != null)
                {
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon);
                }
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
                }

            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation("Stopping data collection for {Url}", _endpointConfig.Url);
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
    }
}