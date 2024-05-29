using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EIR_9209_2.Service
{
    public class EmailEndpointService
    {
        private readonly ILogger<EmailEndpointService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IInMemoryConnectionRepository _connections;
        private readonly IInMemoryGeoZonesRepository _geoZones;
        private readonly IHubContext<HubServices> _hubServices;
        private readonly Connection _endpointConfig;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;

        public EmailEndpointService(ILogger<EmailEndpointService> logger,
          IHttpClientFactory httpClientFactory,
          Connection endpointConfig,
          IInMemoryConnectionRepository connections,
          IInMemoryGeoZonesRepository geoZones,
          IHubContext<HubServices> hubServices)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _endpointConfig = endpointConfig;
            _connections = connections;
            _hubServices = hubServices;
            _geoZones = geoZones;
            _cancellationTokenSource = new CancellationTokenSource();
        }
        public void Start()
        {
            if (_task == null || _task.IsCompleted)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _task = Task.Run(async () => await RunAsync(_cancellationTokenSource.Token));
            }
        }

        public void Stop()
        {
            if (_task != null && !_task.IsCompleted)
            {
                _cancellationTokenSource.Cancel();
            }
        }
        private async Task RunAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await FetchDataFromEndpoint(stoppingToken);
                    if (timer.Period.TotalMilliseconds != _endpointConfig.MillisecondsInterval)
                    {
                        timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_endpointConfig.MillisecondsInterval));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Stopping data collection for {Url}", _endpointConfig.Url);
            }
            finally
            {
                timer.Dispose();
            }
        }
        private async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                IQueryService queryService;
                _endpointConfig.Status = EWorkerServiceState.Running;
                _endpointConfig.LasttimeApiConnected = DateTime.Now;
                _endpointConfig.ApiConnected = true;

                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(_endpointConfig.Url, stoppingToken);
                //loop thought geozone and check if the email is in the geozone
                foreach (var email in _geoZones.GetAll().Where(r => !string.IsNullOrEmpty(r.Properties.Emails)).Select(y => y.Properties).ToList())
                {

                    string FormatUrl = "";
                    //send email

                    FormatUrl = string.Format(_endpointConfig.Url, email.MpeType);
                    queryService = new QueryService(_httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
                    var result = (await queryService.SendEmail(stoppingToken));
                }



            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
            }
        }



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
