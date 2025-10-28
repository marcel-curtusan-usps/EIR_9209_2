using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    /// <summary>
    /// Service for logging data to files.
    /// </summary>
    public class LoggerService : ILoggerService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerService"/> class.
        /// </summary>
        private readonly ILogger<LoggerService> _logger;
        /// <summary>
        /// Service for file operations.
        /// </summary>
        private readonly IFileService _fileService;
        /// <summary>
        /// Directory where log files are stored.
        /// </summary>
        private string _logDirectory;
        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerService"/> class.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="fileService"></param>
        public LoggerService(ILogger<LoggerService> logger, IFileService fileService)
        {
            _logger = logger;
            _fileService = fileService;
        }
        /// <summary>
        /// Logs data to a file.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="messageType"></param>
        /// <param name="name"></param>
        /// <param name="formatUrl"></param>
        /// <returns></returns>
        public async Task LogData(JToken result, string messageType, string name, string formatUrl)
        {
            try
            {
                if (result != null)
                {
                    var logContent = new JObject
                    {
                        ["Datetime"] = DateTime.Now,
                        ["MessageType"] = messageType,
                        ["Name"] = name,
                        ["FormatUrl"] = formatUrl,
                        ["Data"] = result,
                    };

                    // Write log to file
                    string fileName = $"log_{name}_{messageType}_{DateTime.Now:yyyyMMdd}.txt";
                    await _fileService.WriteLogFile(fileName, JsonConvert.SerializeObject(logContent, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging data");
            }
        }
    }
}