using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    public class EmailEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryEmailRepository _email;
        public EmailEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, ILoggerService loggerService, IInMemoryEmailRepository email)
              : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection , loggerService)
        {
            _email = email;
        }
        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                // Dictionary to hold categorized emails
                // Key: Tuple of report type and Mpe name, Value: List of recipient email addresses
                var categorizedEmails = new Dictionary<(string ReportType, string MpeName), List<string>>();
                IEnumerable<Email> emails = _email.GetAll();
                // Iterate through all emails
                foreach (var email in emails)
                {
                    // Assuming each email object has ReportType, MpeName, and RecipientEmailAddress properties
                    var key = (email.ReportName, email.MPEName);
                    if (_endpointConfig.MessageType.StartsWith(email.ReportName) && email.Enabled)
                    {
                        if (!categorizedEmails.ContainsKey(key))
                        {
                            categorizedEmails[key] = [];
                        }
                        categorizedEmails[key].Add(email.EmailAddress);

                    }
                }
                // Now, categorizedEmails dictionary holds the categorized list of email recipients
                // You can iterate through this dictionary to send emails to each category
                foreach (var category in categorizedEmails)
                {
                    var reportType = category.Key.ReportType;
                    var mpeName = category.Key.MpeName;
                    var recipients = category.Value;
                    string FormatUrl = "";

                    FormatUrl = string.Format(_endpointConfig.Url, mpeName);

                    var screenshotStream = await new ScreenshotService().CaptureScreenshotAsync(FormatUrl);
                    var body = "";
                    if (screenshotStream.Length > 0)
                    {
                        // Construct the email content
                        body = $"Dear recipients,\n\nThis email is for the report type '{reportType}' and Zone name '{mpeName}'.\n\nClick on this link to navigate to Report<p>Click on this <a href='{FormatUrl}'>link</a> to navigate to Report.</p>";

                    }
                    else
                    {
                        body = $"Dear recipients,\n\nThis email is for the report type '{reportType}' and Zone name '{mpeName}'.\n\nClick on this link to navigate to Report<p>Click on this <a href='{FormatUrl}'>link</a> to navigate to Report.</p>";

                    }
                    await new EmailService().SendEmailAsync(_configuration["ApplicationConfiguration:SupportEmail"], recipients, "MPE Screen shot", body, screenshotStream);
                }
            }
            catch (Exception ex)
            {
                await _loggerService.LogData(JToken.FromObject(ex.Message), "Error", "FetchDataFromEndpoint", _endpointConfig.Url);
                _endpointConfig.ApiConnected = false;
                _endpointConfig.Status = EWorkerServiceState.ErrorPullingData;
                var updateCon = _connection.Update(_endpointConfig).Result;
                if (updateCon != null)
                {
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, CancellationToken.None);
                }
            }
        }
    }
}