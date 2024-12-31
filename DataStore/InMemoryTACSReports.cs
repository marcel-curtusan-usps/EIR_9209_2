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
        private readonly ConcurrentDictionary<string, List<RawRings>> _tacsRawRings = new();
        private readonly ConcurrentDictionary<string, List<TopOpnCode>> _topOpnCodes = new();
        private readonly ILogger<InMemoryTACSReports> _logger;
        private readonly IConfiguration _configuration;
        private readonly IFileService _fileService;
        private readonly string tacsReportSummaryfileName = "TacsReportSummary.json";
        private readonly string tacsRawRingsFileName = "TacsRawRings";
        private readonly string tacsTopOpnCodesFileName = "TacsTopOpnCodes.json";

        public InMemoryTACSReports(ILogger<InMemoryTACSReports> logger, IConfiguration configuration, IFileService fileService)
        {
            _fileService = fileService;
            _logger = logger;
            _configuration = configuration;
            LoadDataFromFile().Wait();
            LoadTacsDataFromFile().Wait();
            LoadTacsTopOpnDataFromFile().Wait();
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
                ////find raw Ring with code and EmpInfo.EmpId
                DateTime twoDaysAgo = DateTime.Now.AddDays(-2);

                // Find all raw rings for the last 2 days that match the code
                var rawRings = _tacsRawRings.Values
                    .SelectMany(list => list) // Flatten the lists of RawRings
                    .Where(r => r.EmpInfo.EmpId == code && DateTime.Parse(r.TranInfo.TranDate) >= twoDaysAgo)
                    .ToList();

                return rawRings;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }
        public async Task<List<string?>> GetTopOpnCodes(string code)
        {
            try
            {
                return _topOpnCodes.Values
                    .SelectMany(list => list) // Flatten the lists of TopOpnCode
                    .Where(r => r.EmpId == code) // Filter by EmpId
                    .OrderByDescending(r => r.OperationIdCount) // Order by OperationIdCount in descending order
                    .Take(8) // Take the top 8
                    .Select(r => r.OperationId) // Select the OperationId
                    .ToList(); // Convert to a list
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
            string fileDate = "";
            try
            {
                
                if (rawRings == null)
                {
                    return null;
                }
                string empId = rawRings.EmpInfo.EmpId;
                DateTime currenttime = DateTime.Now;
                rawRings.InputInfo.InputDate = currenttime.ToString("yyyy-MM-dd");
                fileDate = rawRings.InputInfo.InputDate;
                rawRings.InputInfo.InputTime = await PostalTime(currenttime);
                rawRings.TranInfo.TranDate = rawRings.InputInfo.InputDate;
                rawRings.TranInfo.TranTime = rawRings.InputInfo.InputTime;

                // add topOpnCode to the list
                if (rawRings.TranInfo.TranCode == "011")
                {
                    _ = Task.Run(async () => await AddTopOpnCode(empId, rawRings.RingInfo.OperationId));
                }
                if (_tacsRawRings.ContainsKey(empId))
                {
                    _tacsRawRings[empId].Add(rawRings);
                    saveToFile = true;
                    return true;
                }
                else
                {
                    _tacsRawRings[empId] = [rawRings];
                    saveToFile = true;
                    return true;
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
                    await _fileService.WriteConfigurationFile($"{tacsRawRingsFileName}_{fileDate}.json", await GetTacsRawRingsByDate(fileDate));
                }
            }
        }
        public async Task<string> GetTacsRawRingsByDate(string fileDate)
        {
            try
            {
                // Parse the fileDate to DateTime
                DateTime targetDate = DateTime.Parse(fileDate);

                // Find all raw rings that match the input date
                var rawRings = _tacsRawRings.Values
                    .SelectMany(list => list) // Flatten the lists of RawRings
                    .Where(r => DateTime.Parse(r.InputInfo.InputDate) == targetDate) // Filter by InputDate
                    .ToList();

                return JsonConvert.SerializeObject(rawRings, Formatting.Indented);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }
        private async Task AddTopOpnCode(string? empId, string? operationId)
        {
            bool saveToFile = false;
            try
            {
                if (_topOpnCodes.ContainsKey(empId))
                {
                    var topOpnCode = _topOpnCodes[empId].FirstOrDefault(r => r.OperationId == operationId);
                    if (topOpnCode != null)
                    {
                        topOpnCode.OperationIdCount++;
                        saveToFile = true;
                    }
                    else
                    {
                        _topOpnCodes[empId].Add(new TopOpnCode { EmpId = empId, OperationId = operationId, OperationIdCount = 1 });
                        saveToFile = true;
                    }
                }
                else
                {
                    _topOpnCodes[empId] = [new TopOpnCode { EmpId = empId, OperationId = operationId, OperationIdCount = 1 }];
                    saveToFile = true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
             
            }
            finally
            {
                if (saveToFile)
                {
                    await _fileService.WriteConfigurationFile(tacsTopOpnCodesFileName, JsonConvert.SerializeObject(_topOpnCodes.Values, Formatting.Indented));
                }
            }
        }

        private async Task LoadDataFromFile()
        {
            try
            {
                // Read data from file
            
                var fileContent = await _fileService.ReadFile(tacsReportSummaryfileName);
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
                // Get the current date
                DateTime currentDate = DateTime.Now;

                // Generate the list of dates for the last 5 days
                List<string> lastFiveDays = Enumerable.Range(0, 5)
                    .Select(offset => currentDate.AddDays(-offset).ToString("yyyy-MM-dd"))
                    .ToList();
                // Iterate over the last 5 days and load the corresponding files
                foreach (var date in lastFiveDays)
                {
                    string fileName = $"TacsRawRings_{date}.json";
                    var fileContent = await _fileService.ReadFile(fileName);
                    if (!string.IsNullOrEmpty(fileContent))
                    {
                        // Parse the file content to get the data. This depends on the format of your file.
                        List<RawRings>? data = JsonConvert.DeserializeObject<List<RawRings>>(fileContent);

                        // Insert the data into the MongoDB collection
                        if (data != null && data.Count != 0)
                        {
                            foreach (RawRings item in data.Select(r => r).ToList())
                            {
                                if (_tacsRawRings.ContainsKey(item.EmpInfo.EmpId))
                                {
                                    _tacsRawRings[item.EmpInfo.EmpId].Add(item);
                                }
                                else
                                {
                                    _tacsRawRings[item.EmpInfo.EmpId] = [item];
                                }
                            }
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
        private async Task LoadTacsTopOpnDataFromFile()
        {
            try
            {
                // Read data from file

                var fileContent = await _fileService.ReadFile(tacsTopOpnCodesFileName);
                if (!string.IsNullOrEmpty(fileContent))
                {
                    // Parse the file content to get the data. This depends on the format of your file.
                    List<TopOpnCode>? data = JsonConvert.DeserializeObject<List<TopOpnCode>>(fileContent);

                    // Insert the data into the MongoDB collection
                    if (data != null && data.Count != 0)
                    {
                        foreach (TopOpnCode item in data.Select(r => r).ToList())
                        {
                            if (_topOpnCodes.ContainsKey(item.EmpId))
                            {
                                _topOpnCodes[item.EmpId].Add(item);
                            }
                            else
                            {
                                _topOpnCodes[item.EmpId] = [item];
                            }
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
                _topOpnCodes.Clear();
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
                // Load data from the second file into the second collection
                LoadTacsDataFromFile().Wait();
                // Load data from the third file into the third collection
                LoadTacsTopOpnDataFromFile().Wait();

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
