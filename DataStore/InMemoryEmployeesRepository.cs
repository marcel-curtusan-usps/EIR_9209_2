using EIR_9209_2.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Data;
using EIR_9209_2.DataStore;
using EIR_9209_2.Utilities;
/// <summary>
/// This class is used to manage the employee information in memory.
/// </summary>
public class InMemoryEmployeesRepository : IInMemoryEmployeesRepository
{
    private readonly ConcurrentDictionary<string, EmployeeInfo> _empList = new();
    private readonly ConcurrentDictionary<string, List<ScanTransaction>> _empScanList = new();
    private readonly IConfiguration _configuration;
    private readonly ILogger<InMemoryEmployeesRepository> _logger;
    private readonly IFileService _fileService;
    /// <summary>
    /// SignalR Hub context for sending messages to clients.
    /// </summary>
    protected readonly IHubContext<HubServices> _hubServices;
    private readonly string fileName = "Employees.json";
    private readonly string ePacsScanFileName = "EpacsScans";
    /// <summary>
    /// Constructor for InMemoryEmployeesRepository.
    /// Initializes the repository with the provided logger, configuration, file service, and hub context.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="configuration"></param>
    /// <param name="fileService"></param>
    /// <param name="hubServices"></param>
    public InMemoryEmployeesRepository(ILogger<InMemoryEmployeesRepository> logger, IConfiguration configuration, IFileService fileService, IHubContext<HubServices> hubServices)
    {
        _fileService = fileService;
        _logger = logger;
        _configuration = configuration;

        // Load data from the first file into the first collection
        LoadDataFromFile().Wait();
        LoadEpacsDataFromFile().Wait();

    }
    /// <summary>
    /// Get Employees List
    /// </summary>
    /// <returns></returns>
    public async Task<object> GetEmployeesList()
    {
        try
        {
            var temp = _empList.Where(r => r.Value.EmployeeId != null).Select(r => new
            {
                id = r.Value.EmployeeId,
                name = r.Value.FirstName + " " + r.Value.LastName
            }).ToList();
            return temp;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return new List<EmployeeInfo>();
        }
    }
    /// <summary>
    /// Get Employee by BLEId
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public Task<EmployeeInfo> GetEmployeeByBLE(string code)
    {
        try
        {
            if (_empList != null && !_empList.IsEmpty)
            {
                var result = _empList.Values
                    .FirstOrDefault(r => r.BleId?.Trim().Equals(code.Trim(), StringComparison.CurrentCultureIgnoreCase) == true);

                return Task.FromResult(result);
            }

            _logger.LogInformation($"BLE ID {code} not found in the employee list.");
            return Task.FromResult<EmployeeInfo?>(null);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error finding BLE ID {code}: {e.Message}");
            return Task.FromResult<EmployeeInfo?>(null);
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
                // Here's an example if your file was in JSON format and contained an array of T objects:
                List<EmployeeInfo>? data = JsonConvert.DeserializeObject<List<EmployeeInfo>>(fileContent);

                // Insert the data into the MongoDB collection
                if (data != null && data.Count > 0)
                {
                    foreach (EmployeeInfo item in data.Select(r => r).ToList())
                    {
                        if (!string.IsNullOrEmpty(item.EmployeeId))
                        {
                            _empList.TryAdd(item.EmployeeId, item);
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
    private async Task LoadEpacsDataFromFile()
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
                string fileName = $"EpacsScans_{date}.json";
                var fileContent = await _fileService.ReadFile(fileName);
                if (!string.IsNullOrEmpty(fileContent))
                {
                    // Parse the file content to get the data. This depends on the format of your file.
                    List<ScanTransaction>? data = JsonConvert.DeserializeObject<List<ScanTransaction>>(fileContent);

                    // Insert the data into the MongoDB collection
                    if (data != null && data.Count != 0)
                    {
                        foreach (ScanTransaction item in data.Select(r => r).ToList())
                        {
                            if (item.EIN != null && !string.IsNullOrEmpty(item.EIN) && _empScanList.ContainsKey(item.EIN))
                            {
                                _empScanList[item.EIN].Add(item);
                            }
                            else
                            {
                                if (item.EIN != null && !string.IsNullOrEmpty(item.EIN))
                                {
                                    _empScanList[item.EIN] = [item];
                                }
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
    public async Task<IEnumerable<EmployeeInfo>> GetAll() => _empList.Values;
    public async Task LoadEmployees(JToken data)
    {
        bool savetoFile = false;
        try
        {
            foreach (var item in data["DATA"]!["EMPLOYEES"]!["DATA"]!)
            {
                var employeeInfo = new EmployeeInfo
                {
                    EmployeeId = item[0]!.ToString(),
                    PayWeek = item[1]!.ToString(),
                    PayLocation = item[2]!.ToString(),
                    LastName = item[3]!.ToString(),
                    FirstName = item[4]!.ToString(),
                    MiddleInit = item[5]!.ToString(),
                    DesActCode = item[6]!.ToString(),
                    Title = item[7]!.ToString(),
                    BaseOp = item[8]!.ToString(),
                    TourNumber = item[9]!.ToString()
                };
                _empList.AddOrUpdate(
                    employeeInfo.EmployeeId,
                    employeeInfo,
                    (existingKey, existingValue) =>
                    {
                        // Update the existing value with the new value
                        existingValue.LastName = employeeInfo.LastName;
                        existingValue.MiddleInit = employeeInfo.MiddleInit;
                        existingValue.FirstName = employeeInfo.FirstName;
                        existingValue.DesActCode = employeeInfo.DesActCode;
                        existingValue.Title = employeeInfo.Title;
                        existingValue.BaseOp = employeeInfo.BaseOp;
                        existingValue.TourNumber = employeeInfo.TourNumber;
                        existingValue.PayLocation = employeeInfo.PayLocation;
                        // Add other properties as needed
                        return existingValue;
                    }
                );
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"Error loading Employee data {e.Message}");
        }
        finally
        {
            if (savetoFile)
            {
                await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_empList.Values, Formatting.Indented));
            }
        }
    }

    public Task<bool> Reset()
    {
        try
        {
            _empList.Clear();
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
    /// <summary>
    /// Load HECSEmployees
    /// </summary>
    /// <param name="result"></param>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    public async Task<bool> LoadHECSEmployees(Hces result, CancellationToken stoppingToken)
    {
        bool savetoFile = false;

        try
        {
            if (result.row.Count > 0)
            {
                //loop through the result rows
                foreach (var row in result.row)
                {
                    //check if cancellationToken has been called
                    if (stoppingToken.IsCancellationRequested)
                    {
                        return false;
                    }
                    EmployeeInfo employeeInfo = new();
                    //employeeGroup
                    string employeeGroup = "";
                    //employeeSubGroup
                    string employeeSubGroup = "";
                    //loop through the field name and value
                    row.field.ForEach(f =>
                    {
                        //get the field name and value
                        var fieldName = f.name;
                        var fieldValue = f.value;
                        if (fieldName == "EIN")
                        {
                            employeeInfo.EmployeeId = fieldValue;
                        }
                        if (fieldName == "lastName")
                        {
                            employeeInfo.LastName = string.IsNullOrEmpty(fieldValue) ? "" : Helper.ConvertToTitleCase(fieldValue);
                        }
                        if (fieldName == "firstName")
                        {
                            employeeInfo.FirstName = string.IsNullOrEmpty(fieldValue) ? "" : Helper.ConvertToTitleCase(fieldValue);
                        }
                        if (fieldName == "employeeGroup")
                        {
                            employeeGroup = fieldValue;
                        }
                        if (fieldName == "employeeSubGroup")
                        {
                            employeeSubGroup = fieldValue;
                        }
                        if (fieldName == "facilityID")
                        {
                            employeeInfo.FacilityID = fieldValue;
                        }
                        if (fieldName == "position")
                        {
                            employeeInfo.Position = fieldValue;
                        }
                        if (fieldName == "dutyStationFINNBR")
                        {
                            employeeInfo.DutyStationFINNBR = fieldValue;
                        }
                        if (fieldName == "employeeStatus")
                        {
                            employeeInfo.EmployeeStatus = fieldValue;
                        }
                    });
                    employeeInfo.DesActCode = $"{employeeGroup}{employeeSubGroup}";
                    if (!_empList.ContainsKey(employeeInfo.EmployeeId) && _empList.TryAdd(employeeInfo.EmployeeId, employeeInfo))
                    {
                        savetoFile = true;
                    }
                    else
                    {
                        if (_empList.TryGetValue(employeeInfo.EmployeeId, out EmployeeInfo currentEmp))
                        {
                            if (currentEmp.FirstName != employeeInfo.FirstName)
                            {
                                currentEmp.FirstName = employeeInfo.FirstName;
                                savetoFile = true;
                            }
                            if (currentEmp.LastName != employeeInfo.LastName)
                            {
                                currentEmp.LastName = employeeInfo.LastName;
                                savetoFile = true;
                            }
                            if (currentEmp.PayLocation != employeeInfo.PayLocation)
                            {
                                currentEmp.PayLocation = employeeInfo.PayLocation;
                                savetoFile = true;
                            }
                            if (currentEmp.DesActCode != employeeInfo.DesActCode)
                            {
                                currentEmp.DesActCode = employeeInfo.DesActCode;
                                savetoFile = true;
                            }
                            if (currentEmp.DutyStationFINNBR != employeeInfo.DutyStationFINNBR)
                            {
                                currentEmp.DutyStationFINNBR = employeeInfo.DutyStationFINNBR;
                                savetoFile = true;
                            }
                            if (currentEmp.Position != employeeInfo.Position)
                            {
                                currentEmp.Position = employeeInfo.Position;
                                savetoFile = true;
                            }
                            if (currentEmp.FacilityID != employeeInfo.FacilityID)
                            {
                                currentEmp.FacilityID = employeeInfo.FacilityID;
                                savetoFile = true;
                            }
                            if (currentEmp.EmployeeStatus != employeeInfo.EmployeeStatus)
                            {
                                currentEmp.EmployeeStatus = employeeInfo.EmployeeStatus;
                                savetoFile = true;
                            }
                        }
                    }
                }
            }

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return true;
        }
        finally
        {
            if (savetoFile)
            {
                await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_empList.Values, Formatting.Indented));
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    public async Task<bool> LoadSMSEmployeeInfo(List<SMSWrapperEmployeeInfo> result, CancellationToken stoppingToken)
    {
        bool savetoFile = false;

        try
        {
            foreach (var empData in result)
            {
                //check if cancellationToken has been called
                if (stoppingToken.IsCancellationRequested)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(empData.Ein))
                {
                    if (_empList.ContainsKey(empData.Ein) && _empList.TryGetValue(empData.Ein, out EmployeeInfo currentEmp))
                    {
                        var firstName = string.IsNullOrEmpty(empData.FirstName) ? "" : Helper.ConvertToTitleCase(empData.FirstName);
                        var lastName = string.IsNullOrEmpty(empData.LastName) ? "" : Helper.ConvertToTitleCase(empData.LastName);
                        if (currentEmp.FirstName != firstName)
                        {
                            currentEmp.FirstName = firstName;
                            savetoFile = true;
                        }
                        if (currentEmp.LastName != lastName)
                        {
                            currentEmp.LastName = lastName;
                            savetoFile = true;
                        }
                        if (currentEmp.PayLocation != empData.PayLocation)
                        {
                            currentEmp.PayLocation = empData.PayLocation;
                            savetoFile = true;
                        }
                        if (currentEmp.CardholderId != empData.CardholderId)
                        {
                            currentEmp.CardholderId = empData.CardholderId;
                            savetoFile = true;
                        }
                        if (currentEmp.BleId != empData.TagId)
                        {
                            currentEmp.BleId = empData.TagId;
                            savetoFile = true;
                        }
                        if (currentEmp.EncodedId != empData.EncodedId)
                        {
                            currentEmp.EncodedId = empData.EncodedId;
                            savetoFile = true;
                        }
                        if (currentEmp.DesActCode != empData.DesignationActivity)
                        {
                            currentEmp.DesActCode = empData.DesignationActivity;
                            savetoFile = true;
                        }
                        if (currentEmp.Title != empData.Title)
                        {
                            currentEmp.Title = empData.Title;
                            savetoFile = true;
                        }
                    }
                    else
                    {
                        if (_empList.TryAdd(empData.Ein, new EmployeeInfo
                        {
                            FirstName = empData.FirstName,
                            LastName = empData.LastName,
                            EmployeeId = empData.Ein,
                            PayLocation = empData.PayLocation,
                            Title = empData.Title,
                            DesActCode = empData.DesignationActivity,
                            BleId = empData.TagId,
                            EncodedId = empData.EncodedId,
                        }))
                        {
                            savetoFile = true;

                        }
                    }
                }
            }
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return true;
        }
        finally
        {
            if (savetoFile)
            {
                await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_empList.Values, Formatting.Indented));
            }
        }
    }

    private async Task AddEpacsScan(ScanInfo scan)
    {
        bool savetoFile = false;
        DateTime fileDate = DateTime.MinValue;
        try
        {
            var transaction = scan.Data.Transactions.FirstOrDefault();
            fileDate = transaction?.TransactionDateTime.Date ?? DateTime.Now;
            var cardholderData = transaction?.CardholderData;
            ScanTransaction scanTransaction = new ScanTransaction
            {

                DeviceID = transaction.DeviceID,
                EIN = cardholderData.EIN,
                ScanDateTime = transaction.TransactionDateTime

            };
            if (_empScanList.ContainsKey(cardholderData.EIN))
            {
                _empScanList[cardholderData.EIN].Add(scanTransaction);
                savetoFile = true;
            }
            else
            {
                _empScanList[cardholderData.EIN] = [scanTransaction];
                savetoFile = true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
        finally
        {
            if (savetoFile)
            {
                var EpacsScanData = await GetEpacsScanByDate(fileDate);
                if (EpacsScanData != null)
                {
                    await _fileService.WriteConfigurationFile($"{ePacsScanFileName}_{fileDate.ToString("yyyy-MM-dd")}.json", EpacsScanData);
                }
            }
        }
    }
    private async Task<string> GetEpacsScanByDate(DateTime fileDate)
    {
        try
        {
            // Find all raw rings that match the input date
            var epacsScan = await Task.Run(() => _empScanList.Values
                .SelectMany(list => list) // Flatten the lists of RawRings
                .Where(r => r.ScanDateTime.Date == fileDate.Date)
                .ToList());

            return JsonConvert.SerializeObject(epacsScan, Formatting.Indented);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return "";
        }
    }
    /// <summary>
    /// Updates the employee information from the EPAC scan data.
    /// </summary>
    /// <param name="scan"></param>
    public void UpdateEmployeeInfoFromEPAC(ScanInfo scan)
    {
        bool savetoFile = false;

        try
        {
            _ = Task.Run(() => AddEpacsScan(scan)).ConfigureAwait(false);
            if (scan.Data.Transactions != null)
            {
                var transaction = scan.Data.Transactions.FirstOrDefault();
                var cardholderData = transaction?.CardholderData;
                if (!string.IsNullOrEmpty(cardholderData?.EIN))
                {
                    ///check if the employee exists in the list then update
                    if (_empList.ContainsKey(cardholderData.EIN) && _empList.TryGetValue(cardholderData.EIN, out EmployeeInfo? currentEmp))
                    {
                        var firstName = string.IsNullOrEmpty(cardholderData.FirstName) ? "" : Helper.ConvertToTitleCase(cardholderData.FirstName);
                        var lastName = string.IsNullOrEmpty(cardholderData.FirstName) ? "" : Helper.ConvertToTitleCase(cardholderData.LastName);
                        if (currentEmp.FirstName.Equals(firstName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            currentEmp.FirstName = firstName;
                            savetoFile = true;
                        }
                        if (currentEmp.LastName.Equals(lastName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            currentEmp.LastName = lastName;
                            savetoFile = true;
                        }
                        if (currentEmp.BleId != cardholderData.ImportField)
                        {
                            currentEmp.BleId = cardholderData.ImportField;
                            savetoFile = true;
                        }
                        if (currentEmp.EncodedId != transaction?.EncodedID)
                        {
                            currentEmp.EncodedId = transaction?.EncodedID;
                            savetoFile = true;
                        }
                        if (currentEmp.CardholderId != transaction?.CardholderID)
                        {
                            currentEmp.CardholderId = transaction.CardholderID;
                            savetoFile = true;
                        }

                        if (currentEmp.CurrentStatus != cardholderData?.CurrentStatus)
                        {
                            currentEmp.CurrentStatus = cardholderData?.CurrentStatus;
                            savetoFile = true;
                        }
                        if (currentEmp.Activation != cardholderData.Activation)
                        {
                            currentEmp.Activation = cardholderData.Activation;
                            savetoFile = true;
                        }
                        if (currentEmp.Expiration != cardholderData.Expiration)
                        {
                            currentEmp.Expiration = cardholderData.Expiration;
                            savetoFile = true;
                        }
                    }
                    else
                    {
                        //add to the employee list
                        if (_empList.TryAdd(cardholderData?.EIN, new EmployeeInfo
                        {
                            FirstName = cardholderData?.FirstName,
                            LastName = cardholderData?.LastName,
                            EmployeeId = cardholderData?.EIN,
                            CurrentStatus = cardholderData?.CurrentStatus,
                            Title = cardholderData?.Title,
                            DesActCode = cardholderData?.DesignationActivity,
                            BleId = cardholderData?.ImportField,
                            EncodedId = transaction?.EncodedID,
                            CardholderId = transaction?.CardholderID ?? 0,
                            EmployeeStatus = cardholderData?.CurrentStatus,
                            Activation = cardholderData.Activation,
                            Expiration = cardholderData.Expiration,
                        }))
                        {

                            savetoFile = true;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
        finally
        {
            if (savetoFile)
            {
                _ = Task.Run(() => _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_empList.Values, Formatting.Indented)));
            }
        }
    }
    /// <summary>
    /// Get Employee by Code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public async Task<object?> GetEmployeeByCode(string code)
    {
        try
        {
            var empData = _empList.Where(r => r.Value.EmployeeId == code || r.Value.EncodedId == code).Select(r => r.Value).FirstOrDefault();
            if (empData != null)
            {
                return empData;
            }
            else
            {
                _logger.LogInformation($"Employee {code} not Found in Employee list ");
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Task<EmployeeInfo> GetEmployeeByEIN(string code)
    {
        try
        {
            if (_empList.ContainsKey(code))
            {
                var employee = _empList.Where(r => r.Value.EmployeeId == code || r.Value.EncodedId == code).Select(r => r.Value).ToList().FirstOrDefault();
                return Task.FromResult(employee);
            }
            else
            {
                return null;
            }

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }
    /// <summary>
    /// Search Employee by EmployeeId, FirstName, LastName, BLEId, EncodedId, CardholderId
    /// </summary>
    /// <param name="searchValue"></param>
    /// <returns></returns>
    public Task<List<JObject>> SearchEmployee(string searchValue)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchValue))
            {
                _logger.LogWarning("Search value is null or empty.");
                return Task.FromResult(new List<JObject>());
            }

            var trimmedSearchValue = searchValue.Trim();
            var regexPattern = Regex.Escape(trimmedSearchValue); // Escape special characters in the search value
            var regexTimeout = TimeSpan.FromMilliseconds(10); // Set a timeout for regex operations

            var empQuery = _empList.Where(sl =>
                sl.Value != null &&
                (
                    IsMatchWithTimeout(sl.Value.Title ?? "", regexPattern, regexTimeout) ||
                    IsMatchWithTimeout(sl.Value.EmployeeId ?? "", regexPattern, regexTimeout) ||
                    IsMatchWithTimeout(sl.Value.FirstName ?? "", regexPattern, regexTimeout) ||
                    IsMatchWithTimeout(sl.Value.LastName ?? "", regexPattern, regexTimeout) ||
                    IsMatchWithTimeout(sl.Value.BleId ?? "", regexPattern, regexTimeout) ||
                    IsMatchWithTimeout(sl.Value.EncodedId ?? "", regexPattern, regexTimeout) ||
                    IsMatchWithTimeout(sl.Value.CardholderId.ToString(), regexPattern, regexTimeout)
                )
            ).Select(r => r.Value).ToList();

            var empSearchResult = empQuery.Select(sr => new JObject
            {
                ["id"] = sr.CardholderId,
                ["tagid"] = sr.BleId,
                ["ein"] = sr.EmployeeId,
                ["yype"] = "Badge",
                ["name"] = sr.FirstName,
                ["encodedId"] = sr.EncodedId,
                ["empFirstName"] = sr.FirstName,
                ["empLastName"] = sr.LastName,
                ["craftName"] = sr.Title,
                ["payLocation"] = sr.PayLocation,
                ["presence"] = sr.EmployeeStatus,
                ["designationActivity"] = sr.DesActCode,
                ["color"] = ""
            }).ToList();

            return Task.FromResult(empSearchResult);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error in SearchEmployee: {e.Message}");
            return Task.FromResult(new List<JObject>());
        }
    }
    /// Helper method for regex matching with timeout
    private bool IsMatchWithTimeout(string input, string pattern, TimeSpan timeout)
    {
        try
        {
            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase, timeout);
        }
        catch (RegexMatchTimeoutException)
        {
            // Log timeout exception if needed
            _logger.LogWarning($"Regex match timed out for input: {input}");
            return false;
        }
    }
    /// <summary>
    /// Get Distinct Employee Id List
    /// </summary>
    /// <returns></returns>
    public async Task<List<string?>> GetDistinctEmployeeIdList()
    {
        try
        {
            return _empList.Where(r => !string.IsNullOrEmpty(r.Value.EmployeeId)).Select(item => item.Value.EmployeeId).Distinct().ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }
}