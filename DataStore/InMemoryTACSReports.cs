using EIR_9209_2.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Globalization;

namespace EIR_9209_2.DataStore
{
    public class InMemoryTACSReports : IInMemoryTACSReports
    {
        private readonly ConcurrentDictionary<string, TACSDailyHours> _dailyHours = new();
        private readonly ConcurrentDictionary<string, TACSEmployeePayPeirod> _employeePayPeirod = new();
        private readonly ConcurrentDictionary<string, TACSSchedule> _schedule = new();
        private readonly ConcurrentDictionary<DateTime, TACSReportSummary> _reportSummary = new();
        private readonly ILogger<InMemoryTACSReports> _logger;
        private readonly IConfiguration _configuration;
        private readonly IFileService _fileService;
        private readonly string filePath = "";
        private readonly string fileName = "";

        public InMemoryTACSReports(ILogger<InMemoryTACSReports> logger, IConfiguration configuration, IFileService fileService)
        {
            _fileService = fileService;
            _logger = logger;
            _configuration = configuration;
            fileName = $"TACSReportSummary.json";
            filePath = Path.Combine(Directory.GetCurrentDirectory(), _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"], $"{fileName}");

            _ = LoadDataFromFile(filePath);
        }

        public void AddEmployeePayPeirods(List<TACSEmployeePayPeirod> employeePayPeirods)
        {
            foreach (TACSEmployeePayPeirod item in employeePayPeirods.Select(r => r).ToList())
            {
                _employeePayPeirod.TryAdd(item.id, item);
            }
        }

        public void AddTACSDailyHours(List<TACSDailyHours> tACSDailyHours)
        {
            //foreach tacsdailyhours in list add to _dailyHours

            foreach (TACSDailyHours item in tACSDailyHours.Select(r => r).ToList())
            {
                _dailyHours.TryAdd(item.id, item);
            }

        }

        public void AddTACSSchedule(List<TACSSchedule> tACSSchedules)
        {
            foreach (TACSSchedule item in tACSSchedules.Select(r => r).ToList())
            {
                _schedule.TryAdd(item.id, item);
            }
        }

        private async Task LoadDataFromFile(string filePath)
        {
            try
            {
                // Read data from file
                var fileContent = await _fileService.ReadFile(filePath);

                // Parse the file content to get the data. This depends on the format of your file.
                // Here's an example if your file was in JSON format and contained an array of T objects:
                List<TACSReportSummary> data = JsonConvert.DeserializeObject<List<TACSReportSummary>>(fileContent);

                // Insert the data into the MongoDB collection
                if (data.Count != 0)
                {
                    foreach (TACSReportSummary item in data.Select(r => r).ToList())
                    {
                        _reportSummary.TryAdd(item.DateTime, item);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                // Handle the FileNotFoundException here
                _logger.LogError($"File not found: {ex.FileName}");
                // You can choose to throw an exception or take any other appropriate action
            }
            catch (IOException ex)
            {
                // Handle errors when reading the file
                _logger.LogError($"An error occurred when reading the file: {ex.Message}");
            }
            catch (JsonException ex)
            {
                // Handle errors when parsing the JSON
                _logger.LogError($"An error occurred when parsing the JSON: {ex.Message}");
            }
        }
    }
}
