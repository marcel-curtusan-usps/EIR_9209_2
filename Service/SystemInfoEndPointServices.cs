using EIR_9209_2.DataStore;
using EIR_9209_2.Utilities;
using Newtonsoft.Json.Linq;
using System.Text;

namespace EIR_9209_2.Service
{
    public class SystemInfoEndPointServices : IDisposable
    {
        private bool disposedValue;

        public async Task<bool> SendSystemInfoToEndpoint(string url, CancellationToken stoppingToken)
        {
            try
            {
 
                using (HttpClient client = new HttpClient())
                {
                    // Get the serial number using SystemInformation
                    string _serialNumber = SystemInformation.SerialNumber;
                    string ipAddress = SystemInformation.GetIPAddress();
                    // Create the data to send
                    var data = new
                    {
                        SerialNumber = _serialNumber,
                        IPAddress = ipAddress,
                        VersionNumber = Helper.GetCurrentVersion(),
                        ApplicationName = Helper.GetAppName()
                    };

                    // Serialize the data to JSON
                    string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                    StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    // Send the POST request
                    HttpResponseMessage response = await client.PostAsync(url, content, stoppingToken);

                    // Check the response status code
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        // Log the response status code and reason
                        string errorMessage = $"Error: {response.StatusCode} - {response.ReasonPhrase}";
                        // Log the error message (optional)
                        return false;
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                // Log the HTTP request exception details
                string errorMessage = $"HttpRequestException: {httpEx.Message}";
                // Log the error message (optional)
                return false;
            }
            catch (TaskCanceledException taskEx)
            {
                // Log the task canceled exception details (e.g., request timeout)
                string errorMessage = $"TaskCanceledException: {taskEx.Message}";
                // Log the error message (optional)
                return false;
            }
            catch (Exception ex)
            {
                // Log the general exception details
                string errorMessage = $"Exception: {ex.Message}";
                // Log the error message (optional)
                return false;
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
        // ~SystemInfoEndPointServices()
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
