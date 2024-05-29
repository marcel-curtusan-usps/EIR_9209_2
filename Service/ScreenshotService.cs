using PuppeteerSharp;
using System.IO;
using System.Net;
using System.Threading.Tasks;
namespace EIR_9209_2.Service
{
    public class ScreenshotService
    {
        public async Task<byte[]> CaptureScreenshotAsync(string url)
        {
            try
            {
                string edgePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
                string chromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";

                string? browserPath = null;

                if (File.Exists(edgePath))
                {
                    browserPath = edgePath;
                }
                else if (File.Exists(chromePath))
                {
                    browserPath = chromePath;
                }

                await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    ExecutablePath = browserPath
                });
                await using var page = await browser.NewPageAsync();
                // Provide the credentials for HTTP authentication
                await page.AuthenticateAsync(new Credentials
                {
                    Username = "username",
                    Password = "password"
                });
                await page.GoToAsync(url, WaitUntilNavigation.Load);
                byte[] screenshotData = await page.ScreenshotDataAsync(new ScreenshotOptions { FullPage = true });

                await browser.CloseAsync();

                return screenshotData;
            }
            catch (Exception e)
            {
                return System.Text.Encoding.UTF8.GetBytes(e.Message);
            }
        }
    }
}
