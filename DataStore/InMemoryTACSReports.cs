using EIR_9209_2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace EIR_9209_2.DataStore
{
    public class InMemoryTACSReports : IInMemoryTACSReports
    {
        private readonly ConcurrentDictionary<string, TACSEmployeePayPeirod> _employeePayPeirod = new();
        private readonly ConcurrentDictionary<DateTime, TACSReportSummary> _reportSummary = new();
        private readonly ConcurrentDictionary<string, RawRings> _tacsRawRings = new();
        private readonly ILogger<InMemoryTACSReports> _logger;
        private readonly IConfiguration _configuration;
        private readonly IFileService _fileService;
        private readonly string fileName = "TACSReportSummary.json";
        private readonly string tacsFileName = "TacsRawRings.json";

        public InMemoryTACSReports(ILogger<InMemoryTACSReports> logger, IConfiguration configuration, IFileService fileService)
        {
            _fileService = fileService;
            _logger = logger;
            _configuration = configuration;
            LoadDataFromFile().Wait();
            LoadTacsDataFromFile().Wait();
        }


   
        public void AddEmployeePayPeirods(List<TACSEmployeePayPeirod> employeePayPeirods)
        {
            foreach (TACSEmployeePayPeirod item in employeePayPeirods.Select(r => r).ToList())
            {
                _employeePayPeirod.TryAdd(item.id, item);
            }
        }
        public async Task<List<RawRings>> GetTACSRawRings(string code)
        {
            try
            {
                //find raw Ring with code and EmpInfo.EmpId
                DateTime threeDaysAgo = DateTime.Now.AddDays(-3);

                // Find all raw rings for the last 3 days that match the code and EmpInfo.EmpId
                var rawRings = _tacsRawRings.Values
                    .Where(r => r.EmpInfo.EmpId == code && DateTime.Parse(r.TranInfo.TranDate) >= threeDaysAgo)
                    .ToList();
                return rawRings;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }
        public async Task<bool?> AddTacsRawRings(RawRings rawRings)
        {
            bool saveToFile = false;
            try
            {
                DateTime currenttime = DateTime.Now;
                if (rawRings == null)
                {
                    return null;
                }
                rawRings.InputInfo.InputDate = currenttime.ToString("yyyy-MM-dd");
                rawRings.InputInfo.InputTime = await PostalTime(currenttime);

                if (_tacsRawRings.TryAdd(string.Concat(rawRings.EmpInfo.EmpId, '_', rawRings.TranInfo.TranDate), rawRings))
                {
                    saveToFile = true;
                    return true;
                }
                else { 
                    return false;
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return false;
            }
            finally
            {
                if (saveToFile)
                {
                    await _fileService.WriteConfigurationFile(tacsFileName, _tacsRawRings.Select(y => y.Value).ToString());
                }
            }
        }

        private async Task LoadDataFromFile()
        {
            try
            {
                // Read data from file
            
                var fileContent = await _fileService.ReadFile(fileName);
                if (!string.IsNullOrEmpty(fileContent))
                {
                    // Parse the file content to get the data. This depends on the format of your file.
                    List<TACSReportSummary>? data = JsonConvert.DeserializeObject<List<TACSReportSummary>>(fileContent);

                    // Insert the data into the MongoDB collection
                    if (data != null && data.Count != 0)
                    {
                        foreach (TACSReportSummary item in data.Select(r => r).ToList())
                        {
                            _reportSummary.TryAdd(item.DateTime, item);
                        }
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
        private async Task LoadTacsDataFromFile()
        {
            try
            {
                // Read data from file

                var fileContent = await _fileService.ReadFile(tacsFileName);
                if (!string.IsNullOrEmpty(fileContent))
                {
                    // Parse the file content to get the data. This depends on the format of your file.
                    List<TACSReportSummary>? data = JsonConvert.DeserializeObject<List<TACSReportSummary>>(fileContent);

                    // Insert the data into the MongoDB collection
                    if (data != null && data.Count != 0)
                    {
                        foreach (TACSReportSummary item in data.Select(r => r).ToList())
                        {
                            _reportSummary.TryAdd(item.DateTime, item);
                        }
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
        private async Task<string> PostalTime(DateTime currenttime)
        {
            try
            {
                // ℎ𝑜𝑢𝑟𝑠 + (⌊(𝑚𝑖𝑛𝑢𝑡𝑒𝑠 × 60 + 𝑠𝑒𝑐𝑜𝑛𝑑𝑠) ÷ 36⌋) ÷ 100
                int hours = currenttime.Hour;
                int minutes = currenttime.Minute;
                int seconds = currenttime.Second;

                double postalTime = hours + Math.Floor((minutes * 60 + seconds) / 36.0) / 100;

                return postalTime.ToString("F2");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return "";
            }
        }
        public Task<bool> Reset()
        {
            try
            {
                _employeePayPeirod.Clear();
                _reportSummary.Clear();
                _tacsRawRings.Clear();
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Task.FromResult(true);
            }
        }

        public Task<bool> Setup()
        {
            try
            {
                // Load data from the first file into the first collection
                LoadDataFromFile().Wait();
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Task.FromResult(true);
            }
        }

      
    }
}
