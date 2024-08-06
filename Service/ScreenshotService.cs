using PuppeteerSharp;
namespace EIR_9209_2.Service
{
    public class ScreenshotService : IDisposable
    {
        private bool disposedValue;

        public async Task<string> CaptureScreenshotAsync(string url)
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
                    ExecutablePath = browserPath,
                    DefaultViewport = {
                        Width= 1920,
                        Height = 1080
                    }

                });
                await using var page = await browser.NewPageAsync();
                // Set the default timeout for all operations to 60 seconds
                page.DefaultTimeout = 6000;
                // set the nav default timeout
                page.DefaultNavigationTimeout = 6000;
                // Provide the credentials for HTTP authentication
                await page.AuthenticateAsync(new Credentials
                {
                    Username = "username",
                    Password = "password"
                });
                await page.GoToAsync(url, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
                await Task.Delay(6000);
                var screenshotData = await page.ScreenshotDataAsync(new ScreenshotOptions
                {
                    FullPage = true,
                    Type = ScreenshotType.Png
                });

                await browser.CloseAsync();
                var base64ScreenshotData = Convert.ToBase64String(screenshotData);
                return base64ScreenshotData;
            }
            catch (Exception e)
            {
                return e.Message;
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
        // ~ScreenshotService()
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
