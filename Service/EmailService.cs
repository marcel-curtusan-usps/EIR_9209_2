using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Utils;

namespace EIR_9209_2.Service
{
    public class EmailService : IDisposable
    {
        private bool disposedValue;

        public async Task SendEmailAsync(string fromEmail, List<string> toEmail, string subject, string body, string base64Image)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Connected Facility ", fromEmail));
                //i want to a list of email address to be sent to
                foreach (var email in toEmail)
                {
                    message.Bcc.Add(new MailboxAddress("", email));
                }
                message.Subject = subject;

                var builder = new BodyBuilder();
                // Add the image as a linked resource and embed it in the email body
                //var image = builder.LinkedResources.Add("screenshot.png", screenshotStream);
                //image.ContentId = MimeUtils.GenerateMessageId();
                // Create the HTML body
                builder.HtmlBody = $@"
                <html>
                <body>
                    <p>{body}</p>
                    <img src='data:image/png;base64,{base64Image}' alt='Screenshot' />
                </body>
                </html>";

                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync("auth-mailrelay.usps.gov", 587, false);
                await client.AuthenticateAsync("SMTP_N_NGTC_4337", "NGTC!3i@M3)7u75!");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                //screenshotStream = null;
                //image = null;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                Dispose(true);
            }

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~EmailService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
