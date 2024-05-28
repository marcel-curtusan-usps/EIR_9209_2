using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Net.Mail;
using System.Threading;

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
                string FormatUrl = "";
                //process tag data

                _endpointConfig.Status = EWorkerServiceState.Running;
                _endpointConfig.LasttimeApiConnected = DateTime.Now;
                _endpointConfig.ApiConnected = true;
                _connections.Update(_endpointConfig);
                FormatUrl = string.Format(_endpointConfig.Url, _endpointConfig.MessageType);

                _ = Task.Run(() => TakeScreenshotAndSendEmail(), stoppingToken);

                //queryService = new QueryService(_httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));
                // var result = (await queryService.GetIDSData(_endpointConfig.MessageType, _endpointConfig.HoursBack, _endpointConfig.HoursForward, stoppingToken));
                // Process tag data in a separate thread



            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
            }
        }

        private void TakeScreenshotAndSendEmail()
        {
            try
            {
                IWebDriver driver = new ChromeDriver();
                driver.Navigate().GoToUrl("http://www.example.com");

                Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                var screenshotStream = new MemoryStream(screenshot.AsByteArray);

                driver.Quit();

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("your_smtp_server");

                mail.From = new MailAddress("your_email@example.com");
                mail.To.Add("to_email@example.com");
                mail.Subject = "Test Mail - 1";
                mail.Body = "mail with attachment";

                Attachment attachment = new Attachment(screenshotStream, "screenshot.png");
                mail.Attachments.Add(attachment);

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("username", "password");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
            }
            catch (Exception e)
            {

                _logger.LogInformation("error", e.Message);
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
