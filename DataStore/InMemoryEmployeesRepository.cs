using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PuppeteerSharp.Input;
using System.Collections.Concurrent;
using System.Data;


public class InMemoryEmployeesRepository : IInMemoryEmployeesRepository
{
    private readonly ConcurrentDictionary<string, EmployeeInfo> _empList = new();
    private readonly ConcurrentDictionary<string, Schedule> _empsch = new();
    private readonly ConcurrentDictionary<string, ScheduleReport> _schReport = new();
    private readonly ConcurrentDictionary<string, Dictionary<DateTime, EmployeeScheduleSummary>> _employeeScheduleDailySummary = new();
    private readonly IConfiguration _configuration;
    private readonly ILogger<InMemoryEmployeesRepository> _logger;
    private readonly IFileService _fileService;
    private readonly IInMemoryTagsRepository _tags;
    protected readonly IHubContext<HubServices> _hubServices;

    private readonly string filePath = "";
    private readonly string fileName = "Employees.json";
    public InMemoryEmployeesRepository(ILogger<InMemoryEmployeesRepository> logger, IConfiguration configuration, IFileService fileService, IHubContext<HubServices> hubServices, IInMemoryTagsRepository tags)
    {
        _fileService = fileService;
        _logger = logger;
        _configuration = configuration;
        _hubServices = hubServices;
        _tags = tags;
        filePath = Path.Combine(_configuration[key: "ApplicationConfiguration:BaseDrive"]!,
            _configuration[key: "ApplicationConfiguration:BaseDirectory"]!,
            _configuration[key: "ApplicationConfiguration:NassCode"]!,
            _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"]!,
            $"{fileName}");
        // Load data from the first file into the first collection
        _ = LoadDataFromFile(filePath);

    }
    private async Task LoadDataFromFile(string filePath)
    {
        try
        {
            // Read data from file
            var fileContent = await _fileService.ReadFile(filePath);

            // Parse the file content to get the data. This depends on the format of your file.
            // Here's an example if your file was in JSON format and contained an array of T objects:
            List<EmployeeInfo> data = JsonConvert.DeserializeObject<List<EmployeeInfo>>(fileContent);

            // Insert the data into the MongoDB collection
            if (data.Any())
            {
                foreach (EmployeeInfo item in data.Select(r => r).ToList())
                {
                    _empList.TryAdd(item.EIN, item);
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
    public IEnumerable<EmployeeInfo> GetAll() => _empList.Values;
    public object? getEmpSchedule()
    {
        //payweek
        string payweek = _empList.Select(y => y.Value.PayWeek)
            .FirstOrDefault() ?? "";
        if (payweek == "")
        {
            return null;
        }
        else
        {
            return _schReport.Where(r => r.Value.PayWeek == payweek).Select(r => r.Value).ToList();
        }
    }
    private string? GetDaySchedule(List<Schedule> wkschedule, string Day)
    {
        try
        {
            var curday = "OFF";
            foreach (var wksch in wkschedule)
            {
                if (wksch.Day == Day)
                {
                    if (wksch.GroupName == "Holiday Off")
                    {
                        curday = "HOLOFF";
                    }
                    else if (wksch.HrLeave == wksch.HrSched)
                    {
                        curday = "LV";
                    }
                    else
                    {
                        curday = wksch.Btour + "-" + wksch.Etour;
                    }
                }
            }
            return curday;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }
    private double? GetTotalHours(List<Schedule> wkschedule)
    {
        try
        {
            double totalhour = 0.00;
            foreach (var wksch in wkschedule)
            {
                if (wksch.GroupName != "Holiday Off" && wksch.HrLeave != wksch.HrSched)
                {
                    totalhour += Convert.ToDouble(wksch.HrMove);
                }
            }
            totalhour = Math.Round(totalhour, 2);
            return totalhour;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }
    public async Task LoadEmployees(JToken data)
    {
        bool savetoFile = false;
        try
        {
            foreach (var item in data["DATA"]!["EMPLOYEES"]!["DATA"]!)
            {
                var employeeInfo = new EmployeeInfo
                {
                    EIN = item[0]!.ToString(),
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
                    employeeInfo.EIN,
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
                await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_empList.Values, Formatting.Indented));
            }
        }
    }
    public async Task LoadEmpSchedule(JToken data)
    {
        bool savetoFile = false;
        try
        {
            foreach (var item in data["DATA"]!["EMPLOYEES"]!["DATA"]!)
            {
                var empsch = new Schedule
                {
                    EIN = item[0]!.ToString(),
                    PayWeek = item[1]!.ToString(),
                    Day = item[2]!.ToString(),
                    HrCodeId = item[3]!.ToString(),
                    GroupName = item[4]!.ToString(),
                    BeginTourDtm = item[5]!.ToString(),
                    EndTourDtm = item[6]!.ToString(),
                    BeginLunchDtm = item[7]!.ToString(),
                    EndLunchDtm = item[8]!.ToString(),
                    BeginMoveDtm = item[9]!.ToString(),
                    EndMoveDtm = item[10]!.ToString(),
                    Btour = item[11]!.ToString(),
                    Etour = item[12]!.ToString(),
                    Blunch = item[13]!.ToString(),
                    Elunch = item[14]!.ToString(),
                    Bmove = item[15]!.ToString(),
                    Emove = item[16]!.ToString(),
                    SectionId = item[17]!.ToString(),
                    SectionName = item[18]!.ToString(),
                    OpCode = item[19]!.ToString(),
                    SortOrder = item[20]!.ToString(),
                    SfasCode = item[21]!.ToString(),
                    RteZipCode = item[22]!.ToString(),
                    RteNbr = item[23]!.ToString(),
                    PvtInd = item[24]!.ToString(),
                    HrLeave = item[25]!.ToString(),
                    HrSched = item[26]!.ToString(),
                    HrTour = item[27]!.ToString(),
                    HrMove = item[28]!.ToString(),
                    HrOt = item[29]!.ToString(),
                    DayErrCnt = item[30]!.ToString()

                };
                _empsch.AddOrUpdate(
                empsch.EIN,
                empsch,
                (existingKey, existingValue) =>
                {
                    // Update the existing value with the new value
                    existingValue.PayWeek = empsch.PayWeek;
                    existingValue.Day = empsch.Day;
                    existingValue.HrCodeId = empsch.HrCodeId;
                    existingValue.GroupName = empsch.GroupName;
                    existingValue.BeginTourDtm = empsch.BeginTourDtm;
                    existingValue.EndTourDtm = empsch.EndTourDtm;
                    existingValue.BeginLunchDtm = empsch.BeginLunchDtm;
                    existingValue.EndLunchDtm = empsch.EndLunchDtm;
                    existingValue.BeginMoveDtm = empsch.BeginMoveDtm;
                    existingValue.EndMoveDtm = empsch.EndMoveDtm;
                    existingValue.Btour = empsch.Btour;
                    existingValue.Etour = empsch.Etour;
                    existingValue.Blunch = empsch.Blunch;
                    existingValue.Elunch = empsch.Elunch;
                    existingValue.Bmove = empsch.Bmove;
                    existingValue.Emove = empsch.Emove;
                    existingValue.SectionId = empsch.SectionId;
                    existingValue.SectionName = empsch.SectionName;
                    existingValue.OpCode = empsch.OpCode;
                    existingValue.SortOrder = empsch.SortOrder;
                    existingValue.SfasCode = empsch.SfasCode;
                    existingValue.RteZipCode = empsch.RteZipCode;
                    existingValue.RteNbr = empsch.RteNbr;
                    existingValue.PvtInd = empsch.PvtInd;
                    existingValue.HrLeave = empsch.HrLeave;
                    existingValue.HrSched = empsch.HrSched;
                    existingValue.HrTour = empsch.HrTour;
                    existingValue.HrMove = empsch.HrMove;
                    existingValue.HrOt = empsch.HrOt;
                    existingValue.DayErrCnt = empsch.DayErrCnt;

                    return existingValue;
                });

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
              await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_empList.Values, Formatting.Indented));
            }
        }

    }
    public void RunEmployeeScheduleDailySummaryReport()
    {
        try
        {
            var empEIN = _empList.Select(item => item.Value).Distinct().ToList();
            if (empEIN.Any())
            {
                foreach (var ein in empEIN)
                {
                    var sch = _empsch.Where(r => r.Value.EIN == ein.EIN && r.Value.PayWeek == ein.PayWeek).Select(r => r.Value).ToList();
                    if (sch.Any())
                    {
                        List<DateTime> days = sch.Where(r => r.PayWeek == ein.PayWeek).Select(r => DateTime.ParseExact(r.BeginTourDtm, "MMMM, dd yyyy HH:mm:ss",
                                                   System.Globalization.CultureInfo.InvariantCulture)).Distinct().ToList();

                        if (!_employeeScheduleDailySummary.ContainsKey(ein.EIN))
                        {
                            _employeeScheduleDailySummary.TryAdd(ein.EIN, new Dictionary<DateTime, EmployeeScheduleSummary>());
                        }
                        foreach (var schday in sch)
                        {
                            //var daySch = GetEmployeeDaySchedule(schday, days, ein);
                            //lock (_employeeScheduleDailySummary)
                            //{
                            //    _employeeScheduleDailySummary[ein.EIN][day] = daySch;
                            //}
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error running Employee Summary Report");
        }

    }

    private EmployeeScheduleSummary? GetEmployeeDaySchedule(Schedule sch, DateTime day, EmployeeInfo ein)
    {
        try
        {
            string date = day.ToString("yyyy-MM-dd");
            var selsHrs = new Dictionary<string, double>();
            var tacsHrs = new Dictionary<string, double>();

            //var selsTimeLine = _tags.GetTagTimelineList(date);
            //if (selsTimeLine != null)
            //{
            //    selsHrs = selsTimeLine.GroupBy(e => e.Ein).ToDictionary(g => g.Key, g => g.Sum(e => e.Duration.TotalMilliseconds));
            //}

            return new EmployeeScheduleSummary
            {
                FirstName = ein.FirstName,
                PayWeek = sch.PayWeek,
                SelsLaborHrs = selsHrs,
                TACSLaborHrs = tacsHrs
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }

    public void RunEmpScheduleReport()
    {

    }

}