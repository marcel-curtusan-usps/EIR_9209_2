using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PuppeteerSharp.Input;
using System.Collections.Concurrent;
using System.Data;
using System.Globalization;


public class InMemoryEmployeesRepository : IInMemoryEmployeesRepository
{
    private readonly ConcurrentDictionary<string, EmployeeInfo> _empList = new();
    private readonly ConcurrentDictionary<string, Schedule> _empsch = new();
    private readonly ConcurrentDictionary<string, Dictionary<DateTime, ScheduleReport>> _schReport = new();
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
            var root = JsonConvert.DeserializeObject<Root>(data.ToString());
           
            foreach (var employeeitem in root.DATA.EMPLOYEES.DATA)
            {

                _empsch.AddOrUpdate(
               string.Concat(employeeitem[0]?.ToString(), "_", employeeitem[1]?.ToString(), "_", Convert.ToInt32(employeeitem[2]?.ToString() ?? "0")),
                      new Schedule
                      {
                          EIN = employeeitem[0]?.ToString(),
                          PayWeek = employeeitem[1]?.ToString(),
                          Day = Convert.ToInt32(employeeitem[2]?.ToString() ?? "0"),
                          HrCodeId = Convert.ToInt32(employeeitem[3]?.ToString() ?? "0"),
                          GroupName = employeeitem[4]?.ToString(),
                          BeginTourDtm = employeeitem[5]?.ToString(),
                          EndTourDtm = employeeitem[6]?.ToString(),
                          BeginLunchDtm = employeeitem[7]?.ToString(),
                          EndLunchDtm = employeeitem[8]?.ToString(),
                          BeginMoveDtm = employeeitem[9]?.ToString(),
                          EndMoveDtm = employeeitem[10]?.ToString(),
                          Btour = employeeitem[11]?.ToString(),
                          Etour = employeeitem[12]?.ToString(),
                          Blunch = employeeitem[13]?.ToString(),
                          Elunch = employeeitem[14]?.ToString(),
                          Bmove = employeeitem[15]?.ToString(),
                          Emove = employeeitem[16]?.ToString(),
                          SectionId = Convert.ToInt32(employeeitem[17]?.ToString() ?? "0"),
                          SectionName = employeeitem[18]?.ToString(),
                          OpCode = employeeitem[19]?.ToString(),
                          SortOrder = Convert.ToInt32(employeeitem[20]?.ToString() ?? "0"),
                          SfasCode = employeeitem[21]?.ToString(),
                          RteZipCode = employeeitem[22]?.ToString(),
                          RteNbr = employeeitem[23]?.ToString(),
                          PvtInd = employeeitem[24]?.ToString(),
                          HrLeave = Convert.ToDouble(employeeitem[25]?.ToString() ?? "0"),
                          HrSched = Convert.ToDouble(employeeitem[26]?.ToString() ?? "0"),
                          HrTour = Convert.ToDouble(employeeitem[27]?.ToString() ?? "0"),
                          HrMove = Convert.ToDouble(employeeitem[28]?.ToString() ?? "0"),
                          HrOt = Convert.ToDouble(employeeitem[29]?.ToString() ?? "0"),
                          DayErrCnt = Convert.ToDouble(employeeitem[30]?.ToString() ?? "0"),
                      },
               (existingKey, existingValue) =>
               {
                   // Update the existing value with the new value
                   existingValue.PayWeek = employeeitem[1]?.ToString();
                   existingValue.Day = Convert.ToInt32(employeeitem[2]?.ToString() ?? "0");
                   existingValue.HrCodeId = Convert.ToInt32(employeeitem[3]?.ToString() ?? "0");
                   existingValue.GroupName = employeeitem[4]?.ToString();
                   existingValue.BeginTourDtm = employeeitem[5]?.ToString();
                   existingValue.EndTourDtm = employeeitem[6]?.ToString();
                   existingValue.BeginLunchDtm = employeeitem[7]?.ToString();
                   existingValue.EndLunchDtm = employeeitem[8]?.ToString();
                   existingValue.BeginMoveDtm = employeeitem[9]?.ToString();
                   existingValue.EndMoveDtm = employeeitem[10]?.ToString();
                   existingValue.Btour = employeeitem[11]?.ToString();
                   existingValue.Etour = employeeitem[12]?.ToString();
                   existingValue.Blunch = employeeitem[13]?.ToString();
                   existingValue.Elunch = employeeitem[14]?.ToString();
                   existingValue.Bmove = employeeitem[15]?.ToString();
                   existingValue.Emove = employeeitem[16]?.ToString();
                   existingValue.SectionId = Convert.ToInt32(employeeitem[17]?.ToString() ?? "0");
                   existingValue.SectionName = employeeitem[18]?.ToString();
                   existingValue.OpCode = employeeitem[19]?.ToString();
                   existingValue.SortOrder = Convert.ToInt32(employeeitem[20]?.ToString() ?? "0");
                   existingValue.SfasCode = employeeitem[21]?.ToString();
                   existingValue.RteZipCode = employeeitem[22]?.ToString();
                   existingValue.RteNbr = employeeitem[23]?.ToString();
                   existingValue.PvtInd = employeeitem[24]?.ToString();
                   existingValue.HrLeave = Convert.ToDouble(employeeitem[25]?.ToString() ?? "0");
                   existingValue.HrSched = Convert.ToDouble(employeeitem[26]?.ToString() ?? "0");
                   existingValue.HrTour = Convert.ToDouble(employeeitem[27]?.ToString() ?? "0");
                   existingValue.HrMove = Convert.ToDouble(employeeitem[28]?.ToString() ?? "0");
                   existingValue.HrOt = Convert.ToDouble(employeeitem[29]?.ToString() ?? "0");
                   existingValue.DayErrCnt = Convert.ToDouble(employeeitem[30]?.ToString() ?? "0");
                   savetoFile = true;
                   return existingValue;
               });
            }
           
        }
        catch (Exception e)
        {
            _logger.LogError($"Error loading Employee schedule data: {e.Message}");
        }
        finally
        {
            if (savetoFile)
            {
                await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_empList.Values, Formatting.Indented));
            }
        }
    }
    public async void RunEmpScheduleReport()
    {
        try
        {
            List<string> empList = _empList.Where(r => !string.IsNullOrEmpty(r.Value.EIN)).Select(item => item.Value.EIN).Distinct().ToList();
            // If any employee is found
            if (empList.Any())
            {
                // Ensure the empDate list contains all 7 days of the week starting from the identified Saturday
                DateTime startOfWeek = new DateTime();
                string payWeek = "";
                List<DateTime> fullWeekRange = new List<DateTime>(new DateTime[7]);

                foreach (var emp in empList)
                {
                    if (startOfWeek == DateTime.MinValue)
                    {
                        payWeek = _empList[emp].PayWeek;
                        var curt = _empsch.Where(r => r.Value.EIN == emp && r.Value.PayWeek == payWeek).Select(y => y.Value).FirstOrDefault();
                        var daydiff = (Int32.Parse(curt.Day!.ToString()) - 1) * -1;
                        DateTime curdate = new DateTime();
                        if (_empList[emp].TourNumber == "3")
                        {
                            curdate = DateTime.Parse(curt.BeginTourDtm, CultureInfo.CurrentCulture, DateTimeStyles.None);
                        }
                        else
                        {
                            curdate = DateTime.Parse(curt.EndTourDtm, CultureInfo.CurrentCulture, DateTimeStyles.None);
                        }
                        startOfWeek = new DateTime(curdate.Year, curdate.Month, curdate.Day, 0, 0, 0).AddDays(daydiff);
                        fullWeekRange = Enumerable.Range(0, 7)
                           .Select(offset => startOfWeek.AddDays(offset))
                           .ToList();


                        //List<DateTime> fullWeekRange = Enumerable.Range(0, 7)
                        //    .Select(offset => startOfWeek.AddDays(offset))
                        //    .ToList();

                        //    firstdate = DateTime.ParseExact(item[6]!.ToString().ToString(), "MMMM, dd yyyy HH:mm:ss",
                        //        System.Globalization.CultureInfo.InvariantCulture);
                        //    var daydiff = (Int32.Parse(item[2]!.ToString()) - 1) * -1;
                        //    weekdate = new DateTime(firstdate.Year, firstdate.Month, firstdate.Day, 0, 0, 0).AddDays(daydiff);
                        //    for (var i = 0; i < 7; i++)
                        //    {
                        //        weekts[i] = weekdate.AddDays(i);
                        //    }
                        //    if (!_schReport.ContainsKey(payWeek))
                        //    {
                        //        ScheduleReport schrpt = new ScheduleReport
                        //        {
                        //            PayWeek = payWeek,
                        //            WeekDate = weekts
                        //        };
                        //        _schReport.TryAdd(payWeek, schrpt);
                        //    }
                        //    else if (_schReport.TryGetValue(payWeek, out ScheduleReport? EmpData))
                        //    {
                        //        EmpData.WeekDate = weekts;
                        //    }
                    }
                    var empSchdate = _empsch.Where(r => r.Value.EIN == emp && r.Value.PayWeek == payWeek).Select(y => y.Value).OrderBy(o => o.Day).ToList();

                    if (empSchdate.Any())
                    {
                        //List<DateTime> empDate = empSchdate
                        //    .Select(x => DateTime.Parse(x.EndTourDtm, CultureInfo.CurrentCulture, DateTimeStyles.None))
                        //    .OrderBy(o => o)
                        //    .ToList();

                        // Generate the full range of dates
                        //DateTime startDate = empDate.First();
                        //DateTime endDate = empDate.Last();

                        // Find the nearest Saturday after or on the startDate
                        //DateTime startOfWeek = startDate.AddDays((6 - (int)startDate.DayOfWeek + 1) % 7);

                        // Ensure the empDate list contains all 7 days of the week starting from the identified Saturday
                        //List<DateTime> fullWeekRange = Enumerable.Range(0, 7)
                        //    .Select(offset => startOfWeek.AddDays(offset))
                        //    .ToList();

                        // Identify missing dates
                        //List<DateTime> missingDates = fullWeekRange.Except(empDate).ToList();

                        // Add missing dates to empDate
                        //empDate.AddRange(missingDates);
                        //empDate = empDate.OrderBy(date => date).ToList();

                        if (!_schReport.ContainsKey(emp))
                        {
                            _schReport.TryAdd(emp, new Dictionary<DateTime, ScheduleReport>());
                        }

                        for (var i=0; i<7; i++)
                        {
                            var schData = empSchdate.FirstOrDefault(r => r.Day == i+1);
                            if (schData == null)
                            {
                                schData = new Schedule
                                {
                                    PayWeek = payWeek,
                                    EIN = emp,
                                    GroupName = "OFF",
                                    Day = i+1,
                                    BeginTourDtm = fullWeekRange[i].ToString("MMMM, dd yyyy HH:mm:ss"),
                                    EndTourDtm = fullWeekRange[i].ToString("MMMM, dd yyyy HH:mm:ss")
                                };
                            }
                            var dailyInfo = await GetDailySummaryforEmployee(emp, fullWeekRange[i], schData);

                            _schReport[emp][fullWeekRange[i].Date] = dailyInfo;

                        }
                        //foreach (var schDate in empDate)
                        //{
                        //    var schData = empSchdate.FirstOrDefault(r => r.EndTourDtm == schDate.ToString("MMMM, dd yyyy HH:mm:ss"));
                        //    if (schData == null)
                        //    {
                        //        schData = new Schedule
                        //        {
                        //            PayWeek = _empList[emp].PayWeek,
                        //            EIN = _empList[emp].EIN,
                        //            GroupName = "OFF",
                        //            Day = (int)schDate.DayOfWeek,
                        //            BeginTourDtm = schDate.ToString("MMMM, dd yyyy HH:mm:ss"),
                        //            EndTourDtm = schDate.ToString("MMMM, dd yyyy HH:mm:ss")
                        //        };
                        //    }
                        //    var dailyInfo = await GetDailySummaryforEmployee(emp, schDate, schData);

                        //    _schReport[emp][schDate.Date] = dailyInfo;

                        //}
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error running Employee Schedule Summary Report");
        }
    }

    private async Task<ScheduleReport> GetDailySummaryforEmployee(string emp, DateTime schDate, Schedule? schedule)
    {
        try
        {
            string dayFormat = schDate.ToString("yyyy-MM-dd");
            var QRElaborHrs = new Dictionary<string, double>();
            var TACSlaborHrs = new Dictionary<string, double>();
            var entireDayThisEmp = await _tags.GetTagTimeline(emp, schDate);
            if (entireDayThisEmp != null)
            {
                QRElaborHrs = entireDayThisEmp.GroupBy(e => e.Ein).ToDictionary(g => g.Key, g => g.Sum(e => e.Duration.TotalMilliseconds));
            }
            var entireTASCThisEmp = await _tags.GetTagTimeline(emp, schDate);
            if (entireTASCThisEmp != null)
            {
                TACSlaborHrs = entireTASCThisEmp.GroupBy(e => e.Ein).ToDictionary(g => g.Key, g => g.Sum(e => e.Duration.TotalMilliseconds));
            }
            return new ScheduleReport
            {
                PayWeek = schedule.PayWeek,
                Date = dayFormat,
                Day = schedule.Day,
                DayName = schDate.DayOfWeek,
                EIN = schedule.EIN,
                WorkStatus = schedule.GroupName,
                BeginTourHour = schedule.Btour,
                EndTourHour = schedule.Etour,
                OpCode = schedule.OpCode,
                SectionName = schedule.SectionName,
                LastName = _empList[schedule.EIN].LastName,
                FirstName = _empList[schedule.EIN].FirstName,
                TourNumber = _empList[schedule.EIN].TourNumber,
                HrsLeave = schedule.HrLeave,
                HrsSchedule = schedule.HrLeave,
                HrsMove = schedule.HrMove,
                DailyQREhr = QRElaborHrs.Sum(x => x.Value),
                DailyTACShr = TACSlaborHrs.Sum(x => x.Value),
                Percentage = (QRElaborHrs.Sum(x => x.Value) / TACSlaborHrs.Sum(x => x.Value)) * 100
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }

    //private ScheduleReport GetDailySummaryforEmployee(string emp, DateTime schDate, Schedule? schedule)
    //{
    //    try
    //    {
    //        string dayFormat = schDate.ToString("yyyy-MM-dd");
    //        var QRElaborHrs = new Dictionary<string, double>();
    //        var TACSlaborHrs = new Dictionary<string, double>();
    //        var entireDayThisEmp = _tags.GetTagTimeline(emp, schDate);
    //        if (entireDayThisEmp != null)
    //        {
    //            QRElaborHrs = entireDayThisEmp.GroupBy(e => e.Ein).ToDictionary(g => g.Key, g => g.Sum(e => e.Duration.TotalMilliseconds));
    //        }
    //        var entireTASCThisEmp = _tags.GetTagTimeline(emp, schDate);
    //        if (entireTASCThisEmp != null)
    //        {
    //            TACSlaborHrs = entireTASCThisEmp.GroupBy(e => e.Ein).ToDictionary(g => g.Key, g => g.Sum(e => e.Duration.TotalMilliseconds));
    //        }
    //        return new ScheduleReport
    //        {
    //            PayWeek = schedule.PayWeek,
    //            Date = dayFormat,
    //            Day = schedule.Day,
    //            DayName = schDate.DayOfWeek,
    //            EIN = schedule.EIN,
    //            WorkStatus = schedule.GroupName,
    //            BeginTourHour = schedule.Btour,
    //            EndTourHour = schedule.Etour,
    //            OpCode= schedule.OpCode,
    //            LastName = _empList[schedule.EIN].LastName,
    //            FirstName = _empList[schedule.EIN].FirstName,
    //            TourNumber = _empList[schedule.EIN].TourNumber,
    //            HrsLeave= schedule.HrLeave,
    //            HrsSchedule = schedule.HrLeave,
    //            HrsMove = schedule.HrMove,
    //            DailyQREhr = QRElaborHrs.Sum(x => x.Value),
    //            DailyTACShr = TACSlaborHrs.Sum(x => x.Value),
    //            Percentage = (QRElaborHrs.Sum(x => x.Value) / TACSlaborHrs.Sum(x => x.Value)) * 100
    //        };
    //    }
    //    catch (Exception e)
    //    {
    //        _logger.LogError(e.Message);
    //        return null;
    //    }
    //}

    public Task<List<string>> GetPayWeeks()
    {
        try
        {
            //get a list of pay week form the schedule
            var payWeeks = _empsch.Select(r => r.Value.PayWeek).Distinct().ToList();
            return Task.FromResult(payWeeks);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error loading Employee data {e.Message}");
            return Task.FromResult(new List<string>());
        }
    }

    public Task<List<ScheduleReport>> GetEmployeesForPayWeek(string payWeek)
    {
        try
        {
            // Find all the employees for the given pay week from _schReport
            var reportsForPayWeek = _schReport
                .SelectMany(emp => emp.Value.Values)
                .Where(report => report.PayWeek == payWeek)
                .ToList();

            return Task.FromResult(reportsForPayWeek);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error loading Employee data {e.Message}");
            return Task.FromResult(new List<ScheduleReport>());
        }
    }
}