using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using System;

namespace EIR_9209_2.Service
{
    public class EmailEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryEmailRepository _email;
        public EmailEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IInMemoryConnectionRepository connection, IInMemoryEmailRepository email)
              : base(logger, httpClientFactory, endpointConfig, configuration, connection)
        {
            _email = email;
        }
        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                IQueryService queryService;
                _endpointConfig.Status = EWorkerServiceState.Running;
                _endpointConfig.LasttimeApiConnected = DateTime.Now;
                _endpointConfig.ApiConnected = true;
                await _connection.Update(_endpointConfig);
                //get list of Mpe name from email list and send email

                // Dictionary to hold categorized emails
                // Key: Tuple of report type and Mpe name, Value: List of recipient email addresses
                var categorizedEmails = new Dictionary<(string ReportType, string MpeName), List<string>>();
                IEnumerable<Email> emails = _email.GetAll();
                // Iterate through all emails
                foreach (var email in emails)
                {
                    // Assuming each email object has ReportType, MpeName, and RecipientEmailAddress properties
                    var key = (email.ReportName, email.MPEName);
                    if (_endpointConfig.MessageType.StartsWith(email.ReportName))
                    {
                        if (!categorizedEmails.ContainsKey(key))
                        {
                            categorizedEmails[key] = new List<string>();
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
                    // Construct the email content
                    var body = $"Dear recipients,\n\nThis email is for the report type '{reportType}' and Zone name '{mpeName}'.\n\nClick on this link to navigate to Report<p>Click on this <a href='{FormatUrl}'>link</a> to navigate to Report.</p>";
                    //how do i add a link to the email body

                    // Assuming you have a method to send emails that takes the subject and the message
                    // Note: You might need to adjust the method to accept multiple recipients or handle it accordingly
                    await new EmailService().SendEmailAsync(_configuration["ApplicationConfiguration:SupportEmail"], recipients, "MPE Screen shot", body, screenshotStream);

                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
            }
        }
    }
}