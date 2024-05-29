using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Utils;
using System.IO;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EIR_9209_2.Service
{
    public class EmailService
    {
        public async Task SendEmailAsync(string fromEmail, string toEmail, string subject, string body, byte[] screenshotStream)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Connected Facility ", fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder();
            // Add the image as a linked resource and embed it in the email body
            var image = builder.LinkedResources.Add("screenshot.png", screenshotStream);
            image.ContentId = MimeUtils.GenerateMessageId();
            // Create the HTML body
            builder.HtmlBody = $@"
            <html>
            <body>
                <p>{body}</p>
                <p><img src=""cid:{image.ContentId}"" alt=""Screenshot"" /></p>
            </body>
            </html>";

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync("auth-mailrelay.usps.gov", 587, false);
            await client.AuthenticateAsync("SMTP_N_NGTC_4337", "NGTC!3i@M3)7u75!");
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            screenshotStream = null;
            image = null;
        }
    }
}
