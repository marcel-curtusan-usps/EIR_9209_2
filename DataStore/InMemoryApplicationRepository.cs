using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.DataStore
{
    /// <summary>
    /// InMemoryApplicationRepository constructor
    /// </summary>
    public class InMemoryApplicationRepository : IInMemoryApplicationRepository
    {
        private readonly ILogger<InMemoryApplicationRepository> _logger;
        private readonly IConfiguration _configuration;
        private readonly IFileService _fileService;
        private string fileName = "appsettings.json";
        /// <summary>
        /// InMemoryApplicationRepository constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        /// <param name="fileService"></param>
        public InMemoryApplicationRepository(ILogger<InMemoryApplicationRepository> logger, IConfiguration configuration, IFileService fileService)
        {
            _fileService = fileService;
            _logger = logger;
            _configuration = configuration;
#if DEBUG
            fileName = $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? ""}.json";
#endif
        }
        public async Task<bool> Update(string key, string value, string section)
        {
            try
            {

                // Read data from file
                //check if the file exists
                if (!File.Exists(fileName))
                {
                    fileName = "appsettings.json";
                }
                var fileContent = await _fileService.ReadFileFromRoot(fileName, "");
                if (!string.IsNullOrEmpty(fileContent))
                {
                    var jsonObj = JObject.Parse(fileContent);
                    var appSettingsSection = jsonObj[section];
                    appSettingsSection[key] = value;
#if DEBUG
                    fileName = $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? ""}.json";
#endif
                    await _fileService.WriteFileInRoot(fileName, "", JsonConvert.SerializeObject(jsonObj, Formatting.Indented));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (FileNotFoundException ex)
            {
                // Handle the FileNotFoundException here
                _logger.LogError($"File not found: {ex.FileName}");
                return false;
                // You can choose to throw an exception or take any other appropriate action
            }
            catch (IOException ex)
            {
                // Handle errors when reading the file
                _logger.LogError($"An error occurred when reading the file: {ex.Message}");
                return false;
            }
            catch (JsonException ex)
            {
                // Handle errors when parsing the JSON
                _logger.LogError($"An error occurred when parsing the JSON: {ex.Message}");
                return false;
            }



        }
    }
}
