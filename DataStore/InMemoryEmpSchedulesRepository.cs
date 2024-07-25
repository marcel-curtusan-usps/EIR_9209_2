using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PuppeteerSharp.Input;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Data;
using Microsoft.Extensions.Hosting;
using static System.Runtime.InteropServices.JavaScript.JSType;


public class InMemoryEmpSchedulesRepository : IInMemoryEmpSchedulesRepository
{
    private readonly ConcurrentDictionary<string, EmpSchedule> _empScheduleList = new();
    private readonly IConfiguration _configuration;
    private readonly ILogger<InMemoryEmpSchedulesRepository> _logger;
    private readonly IFileService _fileService;
    private readonly IInMemoryGeoZonesRepository _zones;
    protected readonly IHubContext<HubServices> _hubServices;

    private readonly string filePath = "";
    private readonly string fileName = "";
    public InMemoryEmpSchedulesRepository(ILogger<InMemoryEmpSchedulesRepository> logger, IConfiguration configuration, IFileService fileService, IHubContext<HubServices> hubServices, IInMemoryGeoZonesRepository zones)
    {
        _fileService = fileService;
        _logger = logger;
        _configuration = configuration;
        _hubServices = hubServices;
        _zones = zones;
        fileName = $"{_configuration[key: "InMemoryCollection:CollectionEmpSchedule"]}.json";
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
            List<EmpSchedule> data = JsonConvert.DeserializeObject<List<EmpSchedule>>(fileContent);

            // Insert the data into the MongoDB collection
            if (data.Any())
            {
                foreach (EmpSchedule item in data.Select(r => r).ToList())
                {
                    _empScheduleList.TryAdd(item.EIN, item);
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
    public IEnumerable<EmpSchedule> GetAll() => _empScheduleList.Values;
    public object getEmpSchedule()
    {
        ConcurrentDictionary<string, IList<string>> reportResults = new ConcurrentDictionary<string, IList<string>>();

        //get dates for the week
        List<Schedule> weekday = _empScheduleList.Where(r => r.Value.WeekSchedule[0].Day == "1").Select(y => y.Value.WeekSchedule).FirstOrDefault();
        DateTime firstdate = DateTime.ParseExact(weekday[0].EndTourDtm.ToString(), "MMMM, dd yyyy HH:mm:ss",
                      System.Globalization.CultureInfo.InvariantCulture);
        List<string> weeklist = new List<string>(new string[7]);
        for (var i = 0; i < 7; i++)
        {
            weeklist[i] = firstdate.AddDays(i).ToString("MMMM dd");
        }

        //add weekly dates for header
        reportResults.TryAdd("weekdate", new List<string> {
            weeklist[0],
            weeklist[1],
            weeklist[2],
            weeklist[3],
            weeklist[4],
            weeklist[5],
            weeklist[6],
        });

        foreach (KeyValuePair<string, EmpSchedule> data in _empScheduleList)
        {
            var hourstotalpercent = "0";

            var day1 = GetDaySchedule(data.Value.WeekSchedule, "1")!.ToString();
            var day2 = GetDaySchedule(data.Value.WeekSchedule, "2")!.ToString();
            var day3 = GetDaySchedule(data.Value.WeekSchedule, "3")!.ToString();
            var day4 = GetDaySchedule(data.Value.WeekSchedule, "4")!.ToString();
            var day5 = GetDaySchedule(data.Value.WeekSchedule, "5")!.ToString();
            var day6 = GetDaySchedule(data.Value.WeekSchedule, "6")!.ToString();
            var day7 = GetDaySchedule(data.Value.WeekSchedule, "7")!.ToString();
            var hourst = GetTotalHours(data.Value.WeekSchedule)?.ToString("0.##");

            var selsday1 = GetSelsSchedule(data.Value.SelsSchedule, "1")?.ToString("0.##");
            var selsday2 = GetSelsSchedule(data.Value.SelsSchedule, "2")?.ToString("0.##");
            var selsday3 = GetSelsSchedule(data.Value.SelsSchedule, "3")?.ToString("0.##");
            var selsday4 = GetSelsSchedule(data.Value.SelsSchedule, "4")?.ToString("0.##");
            var selsday5 = GetSelsSchedule(data.Value.SelsSchedule, "5")?.ToString("0.##");
            var selsday6 = GetSelsSchedule(data.Value.SelsSchedule, "6")?.ToString("0.##");
            var selsday7 = GetSelsSchedule(data.Value.SelsSchedule, "7")?.ToString("0.##");
            var selshourst = GetTotalSelsHours(data.Value.SelsSchedule)?.ToString("0.##");

            reportResults.TryAdd(data.Key, new List<string> {
                data.Value.LastName + ", " + data.Value.FirstName + "<br>" + data.Value.EIN,
                data.Value.TourNumber,
                day1,
                day2,
                day3,
                day4,
                day5,
                day6,
                day7,
                hourst,
                selsday1,
                selsday2,
                selsday3,
                selsday4,
                selsday5,
                selsday6,
                selsday7,
                selshourst,
                hourstotalpercent
            });
        }
        return reportResults;
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
            return totalhour;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }
    private double? GetSelsSchedule(List<Selshour> wkschedule, string Day)
    {
        try
        {
            var curday = 0.00;
            foreach (var wksch in wkschedule)
            {
                if (wksch.Day == Day)
                {
                    curday = wksch.Duration.TotalHours;
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
    private double? GetTotalSelsHours(List<Selshour> wkschedule)
    {
        try
        {
            double totalhour = 0.00;
            foreach (var wksch in wkschedule)
            {
                totalhour += wksch.Duration.TotalHours;
            }
            return totalhour;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }
    public Task LoadEmpInfo(JToken data)
    {
        bool savetoFile = false;
        string empId = "";
        string payWeek = "";
        try
        {
            foreach (var item in data["DATA"]!["EMPLOYEES"]!["DATA"]!)
            {
                empId = item[0]!.ToString();
                payWeek = item[1]!.ToString();
                EmpSchedule? EmpSch = null;
                _empScheduleList.TryGetValue(empId, out EmpSch);
                if (EmpSch != null)
                {
                    EmpSch.PayWeek = item[1]!.ToString();
                    EmpSch.PayLoc = item[2]!.ToString();
                    EmpSch.LastName = item[3]!.ToString();
                    EmpSch.FirstName = item[4]!.ToString();
                    EmpSch.MiddleInit = item[5]!.ToString();
                    EmpSch.DesActCode = item[6]!.ToString();
                    EmpSch.Title = item[7]!.ToString();
                    EmpSch.BaseOp = item[8]!.ToString();
                    EmpSch.TourNumber = item[9]!.ToString();
                    savetoFile = true;
                }
                else
                {
                    EmpSchedule empSch = new EmpSchedule
                    {
                        EIN = empId,
                        PayWeek = item[1]!.ToString(),
                        PayLoc = item[2]!.ToString(),
                        LastName = item[3]!.ToString(),
                        FirstName = item[4]!.ToString(),
                        MiddleInit = item[5]!.ToString(),
                        DesActCode = item[6]!.ToString(),
                        Title = item[7]!.ToString(),
                        BaseOp = item[8]!.ToString(),
                        TourNumber = item[9]!.ToString()
                    };
                    _empScheduleList.TryAdd(empId, empSch);
                    savetoFile = true;
                }
            }
            if (!string.IsNullOrEmpty(payWeek))
            {
                _empScheduleList.Where(y => y.Value.PayWeek != payWeek).ToList().ForEach(y => _empScheduleList.TryRemove(y));
            }
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error loading Employee data {e.Message}");
            return Task.FromResult(true);
        }
        finally
        {
            if (savetoFile)
            {
                _fileService.WriteFile(fileName, JsonConvert.SerializeObject(_empScheduleList.Values, Formatting.Indented));
            }
        }

    }
    public Task LoadEmpSchedule(JToken data)
    {
        bool savetoFile = false;
        bool schupdated = false;
        try
        {
            foreach (var item in data["DATA"]!["EMPLOYEES"]!["DATA"]!)
            {
                string empId = item[0]!.ToString();
                string payWeek = item[1]!.ToString();
                EmpSchedule? EmpSch = null;
                _empScheduleList.TryGetValue(empId, out EmpSch);
                if (EmpSch != null)
                {
                    EmpSch.PayWeek = payWeek;
                    List<Schedule> schList = EmpSch.WeekSchedule.ToList();
                    if (schList.Any())
                    {
                        schupdated = false;
                        foreach (var sch in schList)
                        {
                            if (sch.PayWeek == payWeek)
                            {
                                if (sch.Day == item[2]!.ToString())
                                {
                                    schupdated = true;
                                    //sch.HrCodeId = item[3]!.ToString();
                                    sch.GroupName = item[4]!.ToString();
                                    sch.BeginTourDtm = item[5]!.ToString();
                                    sch.EndTourDtm = item[6]!.ToString();
                                    //sch.BeginLunchDtm = item[7]!.ToString();
                                    //sch.EndLunchDtm = item[8]!.ToString();
                                    //sch.BeginMoveDtm = item[9]!.ToString();
                                    //sch.EndMoveDtm = item[10]!.ToString();
                                    sch.Btour = item[11]!.ToString();
                                    sch.Etour = item[12]!.ToString();
                                    //sch.Blunch = item[13]!.ToString();
                                    //sch.Elunch = item[14]!.ToString();
                                    //sch.Bmove = item[15]!.ToString();
                                    //sch.Emove = item[16]!.ToString();
                                    //sch.SectionId = item[17]!.ToString();
                                    //sch.SectionName = item[18]!.ToString();
                                    //sch.OpCode = item[19]!.ToString();
                                    //sch.SortOrder = item[20]!.ToString();
                                    //sch.SfasCode = item[21]!.ToString();
                                    //sch.RteZipCode = item[22]!.ToString();
                                    //sch.RteNbr = item[23]!.ToString();
                                    //sch.PvtInd = item[24]!.ToString();
                                    sch.HrLeave = item[25]!.ToString();
                                    sch.HrSched = item[26]!.ToString();
                                    sch.HrTour = item[27]!.ToString();
                                    sch.HrMove = item[28]!.ToString();
                                    sch.HrOt = item[29]!.ToString();
                                    sch.DayErrCnt = item[30]!.ToString();
                                }
                            }
                            else
                            {
                                EmpSch.WeekSchedule.Remove(sch);
                            }
                        }
                        if (!schupdated)
                        {
                            EmpSch.WeekSchedule.Add(new Schedule
                            {
                                PayWeek = item[1]!.ToString(),
                                Day = item[2]!.ToString(),
                                //HrCodeId = item[3]!.ToString(),
                                GroupName = item[4]!.ToString(),
                                BeginTourDtm = item[5]!.ToString(),
                                EndTourDtm = item[6]!.ToString(),
                                //BeginLunchDtm = item[7]!.ToString(),
                                //EndLunchDtm = item[8]!.ToString(),
                                //BeginMoveDtm = item[9]!.ToString(),
                                //EndMoveDtm = item[10]!.ToString(),
                                Btour = item[11]!.ToString(),
                                Etour = item[12]!.ToString(),
                                //Blunch = item[13]!.ToString(),
                                //Elunch = item[14]!.ToString(),
                                //Bmove = item[15]!.ToString(),
                                //Emove = item[16]!.ToString(),
                                //SectionId = item[17]!.ToString(),
                                //SectionName = item[18]!.ToString(),
                                //OpCode = item[19]!.ToString(),
                                //SortOrder = item[20]!.ToString(),
                                //SfasCode = item[21]!.ToString(),
                                //RteZipCode = item[22]!.ToString(),
                                //RteNbr = item[23]!.ToString(),
                                //PvtInd = item[24]!.ToString(),
                                HrLeave = item[25]!.ToString(),
                                HrSched = item[26]!.ToString(),
                                HrTour = item[27]!.ToString(),
                                HrMove = item[28]!.ToString(),
                                HrOt = item[29]!.ToString(),
                                DayErrCnt = item[30]!.ToString()
                            });
                        }

                    }
                    else
                    {
                        EmpSch.WeekSchedule.Add(new Schedule
                        {
                            PayWeek = item[1]!.ToString(),
                            Day = item[2]!.ToString(),
                            //HrCodeId = item[3]!.ToString(),
                            GroupName = item[4]!.ToString(),
                            BeginTourDtm = item[5]!.ToString(),
                            EndTourDtm = item[6]!.ToString(),
                            //BeginLunchDtm = item[7]!.ToString(),
                            //EndLunchDtm = item[8]!.ToString(),
                            //BeginMoveDtm = item[9]!.ToString(),
                            //EndMoveDtm = item[10]!.ToString(),
                            Btour = item[11]!.ToString(),
                            Etour = item[12]!.ToString(),
                            //Blunch = item[13]!.ToString(),
                            //Elunch = item[14]!.ToString(),
                            //Bmove = item[15]!.ToString(),
                            //Emove = item[16]!.ToString(),
                            //SectionId = item[17]!.ToString(),
                            //SectionName = item[18]!.ToString(),
                            //OpCode = item[19]!.ToString(),
                            //SortOrder = item[20]!.ToString(),
                            //SfasCode = item[21]!.ToString(),
                            //RteZipCode = item[22]!.ToString(),
                            //RteNbr = item[23]!.ToString(),
                            //PvtInd = item[24]!.ToString(),
                            HrLeave = item[25]!.ToString(),
                            HrSched = item[26]!.ToString(),
                            HrTour = item[27]!.ToString(),
                            HrMove = item[28]!.ToString(),
                            HrOt = item[29]!.ToString(),
                            DayErrCnt = item[30]!.ToString()
                        });
                    }
                    savetoFile = true;
                }
                else
                {
                    EmpSchedule empSch = new EmpSchedule
                    {
                        EIN = empId,
                        PayWeek = item[1]!.ToString()
                    };
                    empSch.WeekSchedule.Add(new Schedule
                    {
                        PayWeek = item[1]!.ToString(),
                        Day = item[2]!.ToString(),
                        //HrCodeId = item[3]!.ToString(),
                        GroupName = item[4]!.ToString(),
                        BeginTourDtm = item[5]!.ToString(),
                        EndTourDtm = item[6]!.ToString(),
                        //BeginLunchDtm = item[7]!.ToString(),
                        //EndLunchDtm = item[8]!.ToString(),
                        //BeginMoveDtm = item[9]!.ToString(),
                        //EndMoveDtm = item[10]!.ToString(),
                        Btour = item[11]!.ToString(),
                        Etour = item[12]!.ToString(),
                        //Blunch = item[13]!.ToString(),
                        //Elunch = item[14]!.ToString(),
                        //Bmove = item[15]!.ToString(),
                        //Emove = item[16]!.ToString(),
                        //SectionId = item[17]!.ToString(),
                        //SectionName = item[18]!.ToString(),
                        //OpCode = item[19]!.ToString(),
                        //SortOrder = item[20]!.ToString(),
                        //SfasCode = item[21]!.ToString(),
                        //RteZipCode = item[22]!.ToString(),
                        //RteNbr = item[23]!.ToString(),
                        //PvtInd = item[24]!.ToString(),
                        HrLeave = item[25]!.ToString(),
                        HrSched = item[26]!.ToString(),
                        HrTour = item[27]!.ToString(),
                        HrMove = item[28]!.ToString(),
                        HrOt = item[29]!.ToString(),
                        DayErrCnt = item[30]!.ToString()
                    });
                    _empScheduleList.TryAdd(empId, empSch);
                    savetoFile = true;
                }
            }
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error loading Employee data {e.Message}");
            return Task.FromResult(true);
        }
        finally
        {
            if (savetoFile)
            {
                _fileService.WriteFile(fileName, JsonConvert.SerializeObject(_empScheduleList.Values, Formatting.Indented));
            }
        }

    }
    public void UpdateEmpScheduleSels()
    {
        bool savetoFile = false;
        bool schupdated = false;
        try
        {
            //get dates for the week
            List<Schedule> weekday = _empScheduleList.Where(r => r.Value.WeekSchedule[0].Day == "1").Select(y => y.Value.WeekSchedule).FirstOrDefault();
            DateTime firstdate = DateTime.ParseExact(weekday[0].EndTourDtm.ToString(), "MMMM, dd yyyy HH:mm:ss",
                          System.Globalization.CultureInfo.InvariantCulture);

            DateTime weekdate = new DateTime(firstdate.Year, firstdate.Month, firstdate.Day, 0, 0, 0).AddHours(7);

            List<long> weekts = new List<long>(new long[7]);
            for (var i = 0; i < 7; i++)
            {
                weekts[i] = (long)weekdate.AddDays(i).Subtract(DateTime.UnixEpoch).TotalSeconds * 1000;
            }
            EmpSchedule? EmpSch = null;
            List<string> einList = _empScheduleList.Select(item => item.Value.EIN).Distinct().ToList();
            if (einList.Any())
            {
                foreach (var ein in einList)
                {
                    _empScheduleList.TryGetValue(ein, out EmpSch);
                    if (EmpSch != null)
                    {
                        List<TagTimeline>? curemp = _zones.GetTagTimelineList(ein);
                        if (curemp.Count > 0)
                        {
                            List<TimeSpan> selstotal = new List<TimeSpan>(new TimeSpan[7]);
                            foreach (var ts in curemp)
                            {
                                for (var i = 0; i < 7; i++)
                                {
                                    if ((i == 6 && ts.End >= weekts[i]) || (ts.End >= weekts[i] && ts.End < weekts[i + 1]))
                                    {
                                        selstotal[i] += ts.Duration;
                                    }
                                }
                            }

                            List<Selshour> selsList = EmpSch.SelsSchedule.ToList();
                            foreach (var sch in selsList)
                            {
                                if (sch.PayWeek != EmpSch.PayWeek)
                                {
                                    EmpSch.SelsSchedule.Remove(sch);
                                }
                            }
                            if (selsList.Count > 0)
                            {
                                for (var i = 0; i < 7; i++)
                                {
                                    if (selstotal[i].TotalSeconds > 0)
                                    {
                                        schupdated = false;
                                        var dayi = (i + 1).ToString();
                                        foreach (var sch in selsList)
                                        {
                                            if (dayi == sch.Day)
                                            {
                                                schupdated = true;
                                                if (selstotal[i] > sch.Duration)
                                                {
                                                    sch.Duration = selstotal[i];
                                                }
                                            }
                                        }
                                        if (!schupdated)
                                        {
                                            EmpSch.SelsSchedule.Add(new Selshour
                                            {
                                                PayWeek = EmpSch.PayWeek,
                                                Day = dayi,
                                                Duration = selstotal[i]
                                            });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                List<Selshour> selshrList = new List<Selshour>();
                                for (var i = 0; i < 7; i++)
                                {
                                    if (selstotal[i].TotalSeconds > 0)
                                    {
                                        selshrList.Add(new Selshour
                                        {
                                            PayWeek = EmpSch.PayWeek,
                                            Day = (i + 1).ToString(),
                                            Duration = selstotal[i]
                                        });
                                    }
                                }
                                EmpSch.SelsSchedule = selshrList;
                            }
                            savetoFile = true;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"Error Updating Employee data {e.Message}");
        }
        finally
        {
            if (savetoFile)
            {
                _fileService.WriteFile(fileName, JsonConvert.SerializeObject(_empScheduleList.Values, Formatting.Indented));
            }
        }
    }
}