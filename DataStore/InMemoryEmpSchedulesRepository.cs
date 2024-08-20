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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Globalization;
using System;
using Humanizer;


public class InMemoryEmpSchedulesRepository : IInMemoryEmpSchedulesRepository
{
    private readonly ConcurrentDictionary<string, EmpSchedule> _empScheduleList = new();
    private readonly ConcurrentDictionary<string, ScheduleReport> _schReport = new();
    private readonly IConfiguration _configuration;
    private readonly ILogger<InMemoryEmpSchedulesRepository> _logger;
    private readonly IFileService _fileService;
    private readonly IInMemoryTagsRepository _tags;
    protected readonly IHubContext<HubServices> _hubServices;

    private readonly string filePath = "";
    private readonly string fileName = "";
    public InMemoryEmpSchedulesRepository(ILogger<InMemoryEmpSchedulesRepository> logger, IConfiguration configuration, IFileService fileService, IHubContext<HubServices> hubServices, IInMemoryTagsRepository tags)
    {
        _fileService = fileService;
        _logger = logger;
        _configuration = configuration;
        _hubServices = hubServices;
        _tags = tags;
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
        //payweek
        string payweek = _empScheduleList.Select(y => y.Value.PayWeek).FirstOrDefault() ?? "";
        if (payweek == "")
        {
            return null;
        }
        else
        {
            return _schReport.Where(r => r.Value.PayWeek == payweek).Select(r => r.Value).ToList();
        }
    }

    private SingleReport? getScheduleAdd(EmpSchedule EmpSch, List<TimeSpan> selstotal)
    {
        try {
            double totalselshr = 0;
            foreach (TimeSpan hr in selstotal)
            {
                totalselshr += hr.TotalHours;
            }
            return new SingleReport
            {
                EIN = EmpSch.EIN,
                LastName = EmpSch.LastName,
                FirstName = EmpSch.FirstName,
                TourNumber = EmpSch.TourNumber,
                day1hr = GetDaySchedule(EmpSch.WeekSchedule, "1")!.ToString(),
                day2hr = GetDaySchedule(EmpSch.WeekSchedule, "2")!.ToString(),
                day3hr = GetDaySchedule(EmpSch.WeekSchedule, "3")!.ToString(),
                day4hr = GetDaySchedule(EmpSch.WeekSchedule, "4")!.ToString(),
                day5hr = GetDaySchedule(EmpSch.WeekSchedule, "5")!.ToString(),
                day6hr = GetDaySchedule(EmpSch.WeekSchedule, "6")!.ToString(),
                day7hr = GetDaySchedule(EmpSch.WeekSchedule, "7")!.ToString(),
                totalhr = GetTotalHours(EmpSch.WeekSchedule) ?? 0,
                day1selshr = selstotal[0].TotalHours,
                day2selshr = selstotal[1].TotalHours,
                day3selshr = selstotal[2].TotalHours,
                day4selshr = selstotal[3].TotalHours,
                day5selshr = selstotal[4].TotalHours,
                day6selshr = selstotal[5].TotalHours,
                day7selshr = selstotal[6].TotalHours,
                totalselshr = totalselshr
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
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
                    List<Schedule> schList = EmpSch.WeekSchedule.ToList();
                    foreach (var sch in schList)
                    {
                        if (sch.PayWeek != payWeek)
                        {
                            EmpSch.WeekSchedule.Remove(sch);
                        }
                    }
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
    public void RunEmpScheduleReport()
    {
        try
        {
            var payweek = "";
            DateTime firstdate = new DateTime();
            DateTime weekdate = new DateTime();
            List<DateTime> weekts = new List<DateTime>(new DateTime[7]);

            EmpSchedule? EmpSch = null;
            ScheduleReport? SchReport = null;

            List<string> einList = _empScheduleList.Select(item => item.Value.EIN).Distinct().ToList();
            if (einList.Any())
            {
                foreach (var ein in einList)
                {
                    _empScheduleList.TryGetValue(ein, out EmpSch);
                    if (EmpSch != null)
                    {
                        if (payweek == "")
                        {
                            payweek = EmpSch.PayWeek;
                            List<Schedule> schList = EmpSch.WeekSchedule.ToList();
                            foreach (var sch in schList)
                            {
                                if (firstdate == DateTime.MinValue && sch.PayWeek == payweek)
                                {
                                    firstdate = DateTime.ParseExact(sch.EndTourDtm.ToString(), "MMMM, dd yyyy HH:mm:ss",
                                                   System.Globalization.CultureInfo.InvariantCulture);
                                    var daydiff = (Int32.Parse(sch.Day)-1) * -1;
                                    weekdate = new DateTime(firstdate.Year, firstdate.Month, firstdate.Day, 0, 0, 0).AddDays(daydiff).AddHours(2);
                                    for (var i = 0; i < 7; i++)
                                    {
                                        weekts[i] = weekdate.AddDays(i);
                                    }
                                }
                            }
                        }
                        if (payweek != "" && firstdate != DateTime.MinValue)
                        {
                            if (!_schReport.ContainsKey(payweek))
                            {
                                ScheduleReport schrpt = new ScheduleReport();
                                List<string> weeklist = new List<string>(new string[7]);
                                for (var i = 0; i < 7; i++)
                                {
                                    weeklist[i] = firstdate.AddDays(i).ToString("MMMM dd");
                                }
                                schrpt.WeekDate1 = weeklist[0];
                                schrpt.WeekDate2 = weeklist[1];
                                schrpt.WeekDate3 = weeklist[2];
                                schrpt.WeekDate4 = weeklist[3];
                                schrpt.WeekDate5 = weeklist[4];
                                schrpt.WeekDate6 = weeklist[5];
                                schrpt.WeekDate7 = weeklist[6];
                                schrpt.PayWeek = payweek;
                                _schReport.TryAdd(payweek, schrpt);
                            }
                            //Get Sels Hours
                            List<TimeSpan> selstotal = new List<TimeSpan>(new TimeSpan[7]);
                            List<TagTimeline>? curemp = _tags.GetTagTimelineList(ein);
                            if (curemp.Count > 0)
                            {
                                DateTime starttmp = new DateTime();
                                DateTime endtmp = new DateTime();
                                DateTime tsstart = new DateTime();
                                TimeSpan durtmp = TimeSpan.Zero;
                                TimeSpan minustmp = TimeSpan.Zero;
                                foreach (var ts in curemp)
                                {
                                    if (ts.Start > weekts[0].AddHours(-8))
                                    {
                                        starttmp = ts.Start;
                                        if (tsstart == DateTime.MinValue)
                                        {
                                            tsstart = ts.Start;
                                        }
                                        if (endtmp != DateTime.MinValue && starttmp < endtmp)
                                        {
                                            minustmp += endtmp - starttmp;
                                        }
                                        if (endtmp != DateTime.MinValue && (starttmp - endtmp).TotalMilliseconds > 2 * 60 * 60 * 1000)
                                        {
                                            for (var i = 0; i < 7; i++)
                                            {
                                                //if (i != 6 && (tsstart >= weekts[i] && ts.End <= weekts[i + 1]))
                                                if (i != 6 && (tsstart >= weekts[i] && endtmp <= weekts[i + 1]))
                                                {
                                                    selstotal[i] += durtmp - minustmp;
                                                    break;
                                                }
                                                //else if ((weekts[i] - tsstart) > TimeSpan.Zero && (ts.End - weekts[i]) > TimeSpan.Zero)
                                                else if ((weekts[i] - tsstart) > TimeSpan.Zero && (endtmp - weekts[i]) > TimeSpan.Zero)
                                                {
                                                    //if ((weekts[i] - tsstart) < (ts.End - weekts[i]))
                                                    if ((weekts[i] - tsstart) < (endtmp - weekts[i]))
                                                        {
                                                            selstotal[i] += durtmp - minustmp;
                                                    }
                                                    else if (i != 0)
                                                    {
                                                        selstotal[i - 1] += durtmp - minustmp;
                                                    }
                                                    break;
                                                }
                                                //else if (i == 6 || (tsstart < weekts[i] && ts.End >= weekts[i + 1]))
                                                else if ((i == 6 && endtmp > weekts[i]) || (i != 6 && tsstart < weekts[i] && endtmp >= weekts[i + 1]))
                                                {
                                                    selstotal[i] += durtmp - minustmp;
                                                    break;
                                                }
                                                //if ((i == 6 && ts.End >= weekts[i]) || (ts.End >= weekts[i] && ts.End < weekts[i + 1]))
                                                //{
                                                //   selstotal[i] += durtmp - minustmp;
                                                //   selstotal[i] += durtmp;
                                                //}
                                            }
                                            tsstart = ts.Start;
                                            durtmp = TimeSpan.Zero;
                                            minustmp = TimeSpan.Zero;
                                        }
                                        durtmp += ts.Duration;
                                        endtmp = ts.End;
                                    }
                                }
                                //last one
                                for (var i = 0; i < 7; i++)
                                {
                                    if (i != 6 && (tsstart >= weekts[i] && endtmp <= weekts[i + 1]))
                                    {
                                        selstotal[i] += durtmp - minustmp;
                                        break;
                                    }
                                    else if ((weekts[i] - tsstart) > TimeSpan.Zero && (endtmp - weekts[i]) > TimeSpan.Zero)
                                    {
                                        if ((weekts[i] - tsstart) < (endtmp - weekts[i]))
                                        {
                                            selstotal[i] += durtmp - minustmp;
                                        }
                                        else if (i != 0)
                                        {
                                            selstotal[i - 1] += durtmp - minustmp;
                                        }
                                        break;
                                    }
                                    else if ((i == 6 && endtmp > weekts[i]) || (i != 6 && tsstart < weekts[i] && endtmp >= weekts[i + 1]))
                                    {
                                        selstotal[i] += durtmp - minustmp;
                                        break;
                                    }
                                }
                            }
                            //Add Employee Schedule & Sels Hours to Report
                            _schReport.TryGetValue(payweek, out SchReport);
                            if (SchReport != null)
                            {
                                if (SchReport.ScheduleList.Where(r => r.EIN == ein).Any())
                                {
                                    SchReport.ScheduleList.Where(r => r.EIN == ein).ToList().ForEach(sch =>
                                    {
                                        double totalselshr = 0;
                                        foreach (TimeSpan hr in selstotal)
                                        {
                                            totalselshr += hr.TotalHours;
                                        }

                                        sch.day1hr = GetDaySchedule(EmpSch.WeekSchedule, "1")!.ToString();
                                        sch.day1hr = GetDaySchedule(EmpSch.WeekSchedule, "2")!.ToString();
                                        sch.day1hr = GetDaySchedule(EmpSch.WeekSchedule, "3")!.ToString();
                                        sch.day1hr = GetDaySchedule(EmpSch.WeekSchedule, "4")!.ToString();
                                        sch.day1hr = GetDaySchedule(EmpSch.WeekSchedule, "5")!.ToString();
                                        sch.day1hr = GetDaySchedule(EmpSch.WeekSchedule, "6")!.ToString();
                                        sch.day1hr = GetDaySchedule(EmpSch.WeekSchedule, "7")!.ToString();
                                        sch.totalhr = GetTotalHours(EmpSch.WeekSchedule) ?? 0;
                                        sch.day1selshr = Math.Round(selstotal[0].TotalHours, 2);
                                        sch.day2selshr = Math.Round(selstotal[1].TotalHours, 2);
                                        sch.day3selshr = Math.Round(selstotal[2].TotalHours, 2);
                                        sch.day4selshr = Math.Round(selstotal[3].TotalHours, 2);
                                        sch.day5selshr = Math.Round(selstotal[4].TotalHours, 2);
                                        sch.day6selshr = Math.Round(selstotal[5].TotalHours, 2);
                                        sch.day7selshr = Math.Round(selstotal[6].TotalHours, 2);
                                        sch.totalselshr = Math.Round(totalselshr, 2);
                                    });
                                }
                                else
                                {
                                    SingleReport newsch = getScheduleAdd(EmpSch, selstotal);
                                    if (newsch != null)
                                    {
                                        SchReport.ScheduleList.Add(newsch);
                                    }
                                }
                            }
                            else
                            {
                                SingleReport newsch = getScheduleAdd(EmpSch, selstotal);
                                if (newsch != null)
                                {
                                    SchReport?.ScheduleList.Add(newsch);
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error running Employee Schedule Report");
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

            //DateTime weekdatetmp = new DateTime(firstdate.Year, firstdate.Month, firstdate.Day, 0, 0, 0);
            //TimeZoneInfo est = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            //DateTime someDateTimeInUtc = TimeZoneInfo.ConvertTimeToUtc(weekdatetmp, est);

            DateTime weekdate = new DateTime(firstdate.Year, firstdate.Month, firstdate.Day, 0, 0, 0).AddHours(7);
            //long weekdatets = (long)weekdate.Subtract(DateTime.UnixEpoch).TotalSeconds * 1000;
            //long weekdateutcts = (long)someDateTimeInUtc.Subtract(DateTime.UnixEpoch).TotalSeconds * 1000;

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
                    /*
                    if (EmpSch != null)
                    {
                        List<TagTimeline>? curemp = _zones.GetTagTimelineList(ein);
                        if (curemp.Count > 0)
                        {
                            List<TimeSpan> selstotal = new List<TimeSpan>(new TimeSpan[7]);
                            long starttmp = 0;
                            long endtmp = 0;
                            long tsstart = 0;
                            TimeSpan durtmp = TimeSpan.Zero;
                            TimeSpan minustmp = TimeSpan.Zero;
                            //foreach (var ts in curemp)
                            //{
                            //    starttmp = ts.Start;
                            //    if (tsstart == 0)
                            //    {
                            //        tsstart = ts.Start;
                            //    }
                            //    if (endtmp != 0 && starttmp < endtmp)
                            //    {
                            //        minustmp += TimeSpan.FromSeconds((endtmp - starttmp) / 1000);
                            //    }
                            //    if (endtmp != 0 && (starttmp - endtmp) > 8 * 60 * 60 * 1000)
                            //    {
                            //        for (var i = 0; i < 7; i++)
                            //        {
                            //            if (i != 6 && (tsstart >= weekts[i] && ts.End <= weekts[i + 1]))
                            //            {
                            //                selstotal[i] += durtmp - minustmp;
                            //                break;
                            //            }
                            //            else if ((weekts[i] - tsstart) > 0 && (ts.End - weekts[i]) > 0)
                            //            {
                            //                if ((weekts[i] - tsstart) < (ts.End - weekts[i]))
                            //                {
                            //                    selstotal[i] += durtmp - minustmp;
                            //                }
                            //                else if (i != 0)
                            //                {
                            //                    selstotal[i - 1] += durtmp - minustmp;
                            //                }
                            //                break;
                            //            }
                            //            else if (i == 6 || (tsstart < weekts[i] && ts.End >= weekts[i + 1]))
                            //            {
                            //                selstotal[i] += durtmp - minustmp;
                            //                break;
                            //            }
                            //            //if ((i == 6 && ts.End >= weekts[i]) || (ts.End >= weekts[i] && ts.End < weekts[i + 1]))
                            //            //{
                            //            //   selstotal[i] += durtmp - minustmp;
                            //            //   selstotal[i] += durtmp;
                            //            //}
                            //        }
                            //        tsstart = ts.Start;
                            //        durtmp = TimeSpan.Zero;
                            //        minustmp = TimeSpan.Zero;
                            //    }
                            //    durtmp += ts.Duration;
                            //    endtmp = ts.End;
                            //}
                            //last one
                            for (var i = 0; i < 7; i++)
                            {
                                if (i != 6 && (tsstart >= weekts[i] && endtmp <= weekts[i + 1]))
                                {
                                    selstotal[i] += durtmp - minustmp;
                                    break;
                                }
                                else if ((weekts[i] - tsstart) > 0 && (endtmp - weekts[i]) > 0)
                                {
                                    if ((weekts[i] - tsstart) < (endtmp - weekts[i]))
                                    {
                                        selstotal[i] += durtmp - minustmp;
                                    }
                                    else if (i != 0)
                                    {
                                        selstotal[i - 1] += durtmp - minustmp;
                                    }
                                    break;
                                }
                                else if (i == 6 || (tsstart < weekts[i] && endtmp >= weekts[i + 1]))
                                {
                                    selstotal[i] += durtmp - minustmp;
                                    break;
                                }
                                //if ((i == 6 && endtmp >= weekts[i]) || (endtmp >= weekts[i] && endtmp < weekts[i + 1]))
                                //{
                                //    selstotal[i] += durtmp - minustmp;
                                //selstotal[i] += durtmp;
                                //}
                            }

                            //foreach (var ts in curemp)
                            //{
                            //for (var i = 0; i < 7; i++)
                            //{
                            //    if ((i == 6 && ts.End >= weekts[i]) || (ts.End >= weekts[i] && ts.End < weekts[i + 1]))
                            //    {
                            //        selstotal[i] += ts.Duration;
                            //    }
                            //}
                            //}
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
                                                else if (selstotal[i] < sch.Duration && i != 0 && selstotal[i - 0].TotalSeconds > 0)
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
                    */
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