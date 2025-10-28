using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    /// <summary>
    /// Service for logging data.
    /// </summary>
    public interface ILoggerService
    {
        /// <summary>
        /// Logs data asynchronously.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="messageType"></param>
        /// <param name="name"></param>
        /// <param name="formatUrl"></param>
        /// <returns></returns>
        Task LogData(JToken result, string messageType, string name, string formatUrl);
    }
}