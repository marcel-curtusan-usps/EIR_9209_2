using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.AspNetCore.SignalR;

namespace EIR_9209_2.DataStore
{
    public class InMemoryEmployeesSchedule : IInMemoryEmployeesSchedule
    {
        private readonly ConcurrentDictionary<string, Schedule> _empsch = new();
        private readonly ConcurrentDictionary<string, Dictionary<DateTime, ScheduleReport>> _schReport = new();
        private readonly ConcurrentDictionary<string, Dictionary<DateTime, EmployeeScheduleSummary>> _employeeScheduleDailySummary = new();
        private readonly IInMemoryTagsRepository _tags;
        private readonly IInMemoryEmployeesRepository _employees;
        private readonly ILogger<InMemoryEmployeesSchedule> _logger;
        private readonly IFileService _fileService;
        protected readonly IHubContext<HubServices> _hubServices;
        private readonly string fileName = "EmployeesSchedule.json";
        public InMemoryEmployeesSchedule(ILogger<InMemoryEmployeesSchedule> logger, IInMemoryEmployeesRepository employees, IInMemoryTagsRepository tags, IFileService fileService, IHubContext<HubServices> hubServices)
        {
            _employees = employees;
            _tags = tags;
            _logger = logger;
            _fileService = fileService;
            _hubServices = hubServices;

            // Load data from the first file into the first collection
            LoadDataFromFile().Wait();
        }
        private async Task LoadDataFromFile()
        {
            try
            {
                // Read data from file
                var fileContent = await _fileService.ReadFile(fileName);
                if (!string.IsNullOrEmpty(fileContent))
                {

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
        }

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
        public async void RunEmpScheduleReport()
        {
            try
            {
                ConcurrentDictionary<string, EmployeeInfo> _empList = _employees.GetEMPInfo();
                List<string?> empList = _empList.Where(r => !string.IsNullOrEmpty(r.Value.EmployeeId)).Select(item => item.Value.EmployeeId).Distinct().ToList();
                // If any employee is found
                if (empList.Any())
                {
                    // Ensure the empDate list contains all 7 days of the week starting from the identified Saturday
                    DateTime startOfWeek = new DateTime();
                    string? payWeek = "";
                    List<DateTime> fullWeekRange = new List<DateTime>(new DateTime[7]);

                    foreach (var emp in empList)
                    {
                        payWeek = _empsch.Select(r => r.Value.PayWeek).Distinct().ToList().FirstOrDefault();
                        var empSchdate = _empsch.Where(r => r.Value.EIN == emp && r.Value.PayWeek == payWeek).Select(y => y.Value).OrderBy(o => o.Day).ToList();
                        if (empSchdate.Any())
                        {
                            if (startOfWeek == DateTime.MinValue && _empsch.Count() > 0)
                            {

                                //•	_empsch: This is a ConcurrentDictionary < string, Schedule >, where the key is a string(likely an employee identifier) and the value is a Schedule object.
                                //•	.Select(r => r.Value.PayWeek): This selects the PayWeek property from each Schedule object in the dictionary.
                                //•	.Distinct(): This ensures that only unique PayWeek values are considered.
                                //•	.ToList(): Converts the distinct PayWeek values into a list.
                                //•	.FirstOrDefault(): Retrieves the first PayWeek from the list, or null if the list is empty.

                               
                                //•	.Where(r => r.Value.EIN == emp && r.Value.PayWeek == payWeek): Filters the _empsch dictionary to find entries where the EIN(Employee Identification Number) matches the emp variable and the PayWeek matches the previously selected payWeek.
                                //•	.Select(y => y.Value): Selects the Schedule objects from the filtered entries.
                                //•	.FirstOrDefault(): Retrieves the first Schedule object from the filtered results, or null if no such object exists.

                                var curt = _empsch.Where(r => r.Value.EIN == emp && r.Value.PayWeek == payWeek).Select(y => y.Value).FirstOrDefault();

                                //•	if (curt == null): Checks if the curt variable is null, which would indicate that no matching Schedule was found.
                                //•	continue;: If curt is null, the continue statement skips the rest of the current iteration of the loop and moves to the next iteration.
                                if (curt == null)
                                {
                                    continue;
                                }
                                var daydiff = (curt.Day - 1) * -1;
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

                            }
                      

                       
                            if (!_schReport.ContainsKey(emp))
                            {
                                _schReport.TryAdd(emp, new Dictionary<DateTime, ScheduleReport>());
                            }

                            for (var i = 0; i < 7; i++)
                            {
                                var schData = empSchdate.FirstOrDefault(r => r.Day == i + 1);
                                if (schData == null)
                                {
                                    schData = new Schedule
                                    {
                                        PayWeek = payWeek,
                                        EIN = emp,
                                        GroupName = "OFF",
                                        Day = i + 1,
                                        BeginTourDtm = fullWeekRange[i].ToString("MMMM, dd yyyy HH:mm:ss"),
                                        EndTourDtm = fullWeekRange[i].ToString("MMMM, dd yyyy HH:mm:ss")
                                    };
                                }
                                var dailyInfo = await GetDailySummaryforEmployee(emp, fullWeekRange[i], schData, _empList[emp]);

                                _schReport[emp][fullWeekRange[i].Date] = dailyInfo;

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error running Employee Schedule Summary Report");
            }
        }

        private async Task<ScheduleReport> GetDailySummaryforEmployee(string emp, DateTime schDate, Schedule? schedule, EmployeeInfo? employeeInfo)
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
                //var entireTASCThisEmp = await _tags.GetTagTimeline(emp, schDate);
                //if (entireTASCThisEmp != null)
                //{
                //    TACSlaborHrs = entireTASCThisEmp.GroupBy(e => e.Ein).ToDictionary(g => g.Key, g => g.Sum(e => e.Duration.TotalMilliseconds / 60 / 60 / 1000));
                //}
                return new ScheduleReport
                {
                    PayWeek = schedule.PayWeek,
                    Date = dayFormat,
                    Day = schedule.Day,
                    DayName = schDate.DayOfWeek,
                    EIN = schedule.EIN,
                    WorkStatus = (schedule.GroupName == "Working" && schedule.HrMove == 0.0) ? "Leave" : schedule.GroupName,
                    BeginTourHour = schedule.Btour,
                    EndTourHour = schedule.Etour,
                    OpCode = schedule.OpCode,
                    SectionName = schedule.SectionName,
                    LastName = employeeInfo.LastName,
                    FirstName = employeeInfo.FirstName,
                    TourNumber = employeeInfo.TourNumber,
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
        public Task<bool> ResetScheduleList()
        {
            try
            {
                _empsch.Clear();
                _schReport.Clear();
                _employeeScheduleDailySummary.Clear();
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
