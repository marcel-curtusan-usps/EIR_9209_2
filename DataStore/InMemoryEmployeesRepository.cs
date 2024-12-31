using EIR_9209_2.Models;
using Humanizer;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto;
using PuppeteerSharp.Input;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using static NuGet.Packaging.PackagingConstants;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class InMemoryEmployeesRepository : IInMemoryEmployeesRepository
{
    private readonly ConcurrentDictionary<string, EmployeeInfo> _empList = new();
    private readonly IConfiguration _configuration;
    private readonly ILogger<InMemoryEmployeesRepository> _logger;
    private readonly IFileService _fileService;
    protected readonly IHubContext<HubServices> _hubServices;
    private readonly string fileName = "Employees.json";
    public InMemoryEmployeesRepository(ILogger<InMemoryEmployeesRepository> logger, IConfiguration configuration, IFileService fileService, IHubContext<HubServices> hubServices)
    {
        _fileService = fileService;
        _logger = logger;
        _configuration = configuration;

        // Load data from the first file into the first collection
        LoadDataFromFile().Wait();

    }
    public async Task<EmployeeInfo?> GetEmployeeByBLE(string id)
    {
        try
        {
            return _empList.Where(r => r.Value.BleId == id).Select(r => r.Value).FirstOrDefault();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
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
                            employeeInfo.LastName = fieldValue;
                        }
                        if (fieldName == "firstName")
                        {
                            employeeInfo.FirstName = fieldValue;
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
                        if (currentEmp.FirstName != empData.FirstName)
                        {
                            currentEmp.FirstName = empData.FirstName;
                            savetoFile = true;
                        }
                        if (currentEmp.LastName != empData.LastName)
                        {
                            currentEmp.LastName = empData.LastName;
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
            

                //if (empData.ContainsKey("ein") && !string.IsNullOrEmpty(empData["ein"].ToString()))
                //{
                //    TagData = _tagList.Where(r => r.Value.Properties.EIN == empData["ein"].ToString()).Select(y => y.Value).FirstOrDefault();
                //}

                //if (TagData != null && _tagList.TryGetValue(TagData.Properties.Id, out GeoMarker currentTag))
                //{
                //    //check if tag type is not null and update the tag type

                //    currentTag.Properties.TagType = "Badge";
                //    savetoFile = true;


                //    //check EIN value is not null and update the EIN value
                //    if (empData.ContainsKey("ein"))
                //    {
                //        if (!string.IsNullOrEmpty(empData["ein"].ToString()) && currentTag.Properties.EIN != empData["ein"].ToString())
                //        {
                //            currentTag.Properties.EIN = empData["ein"].ToString();

                //            savetoFile = true;
                //        }
                //    }
                //    //check FirstName value is not null and update the FirstName value
                //    if (empData.ContainsKey("firstName"))
                //    {
                //        if (!Regex.IsMatch(currentTag.Properties.EmpFirstName, $"^{Regex.Escape(empData["firstName"].ToString())}$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)))
                //        {
                //            currentTag.Properties.EmpFirstName = empData["firstName"].ToString();
                //            savetoFile = true;
                //        }
                //    }
                //    //check LastName value is not null and update the LastName value
                //    if (empData.ContainsKey("lastName"))
                //    {
                //        if (!Regex.IsMatch(currentTag.Properties.EmpLastName, $"^{Regex.Escape(empData["lastName"].ToString())}$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)))
                //        {
                //            currentTag.Properties.EmpLastName = empData["lastName"].ToString();
                //            savetoFile = true;
                //        }
                //    }
                //    //check title value is not null and update the title value
                //    if (empData.ContainsKey("title"))
                //    {
                //        if (!Regex.IsMatch(currentTag.Properties.Title, $"^{Regex.Escape(empData["title"].ToString())}$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10)))
                //        {
                //            currentTag.Properties.Title = empData["title"].ToString();
                //            savetoFile = true;
                //        }
                //    }
                //    //check encodedId value is not null and update the title value
                //    if (empData.ContainsKey("encodedId"))
                //    {
                //        if (!string.IsNullOrEmpty(empData["encodedId"].ToString()) && currentTag.Properties.EncodedId != empData["encodedId"].ToString())
                //        {
                //            currentTag.Properties.EncodedId = empData["encodedId"].ToString();
                //            savetoFile = true;
                //        }
                //    }

                //    //check designationActivity value is not null and update the designationActivity value
                //    if (empData.ContainsKey("designationActivity"))
                //    {
                //        if (!string.IsNullOrEmpty(empData["designationActivity"].ToString()) && currentTag.Properties.DesignationActivity != empData["designationActivity"].ToString())
                //        {
                //            if (string.IsNullOrEmpty(currentTag.Properties.DesignationActivity))
                //            {
                //                currentTag.Properties.DesignationActivity = empData["designationActivity"].ToString();
                //                savetoFile = true;
                //            }

                //            var daCode = _dacode.Get(empData["designationActivity"].ToString());
                //            if (daCode != null)
                //            {
                //                currentTag.Properties.CraftName = daCode.CraftType;
                //                savetoFile = true;
                //            }


                //        }


                //    }
                //    //check paylocation value is not null and update the paylocation value
                //    if (empData.ContainsKey("paylocation"))
                //    {
                //        if (!string.IsNullOrEmpty(empData["paylocation"].ToString()) && currentTag.Properties.EmpPayLocation != empData["paylocation"].ToString())
                //        {
                //            currentTag.Properties.EmpPayLocation = empData["paylocation"].ToString();
                //            savetoFile = true;
                //        }
                //    }
                //}
            }
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return true;
        }
        finally {
            if (savetoFile)
            {
                await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_empList.Values, Formatting.Indented));
            }
        }
    }



    public async Task<object?> GetEmployeeByCode(string code)
    {
        try
        {
            var empData =  _empList.Where(r => r.Value.EmployeeId == code || r.Value.EncodedId == code).Select(r => r.Value).FirstOrDefault();
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
    public async Task<EmployeeInfo?> GetEmployeeByEIN(string code)
    {
        try
        {
            return _empList.Where(r => r.Value.EmployeeId == code || r.Value.EncodedId == code).Select(r => r.Value).FirstOrDefault();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }

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