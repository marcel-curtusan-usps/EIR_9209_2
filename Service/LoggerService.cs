using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    public class LoggerService : ILoggerService
    {
        private readonly ILogger<LoggerService> _logger;
        private readonly IFileService _fileService;
        private string _logDirectory;

        public LoggerService(ILogger<LoggerService> logger, IFileService fileService)
        {
            _logger = logger;
            _fileService = fileService;
        }

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