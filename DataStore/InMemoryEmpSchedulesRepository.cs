using EIR_9209_2.Models;
using MailKit.Search;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using static EIR_9209_2.Models.GeoMarker;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class InMemoryEmpSchedulesRepository : IInMemoryEmpSchedulesRepository
{
    private readonly ConcurrentDictionary<string, EmpSchedule> _empScheduleList = new();
    private readonly IConfiguration _configuration;
    private readonly ILogger<InMemoryEmpSchedulesRepository> _logger;
    private readonly IFileService _fileService;
    protected readonly IHubContext<HubServices> _hubServices;
    private readonly string filePath = "";
    private readonly string fileName = "";
    public InMemoryEmpSchedulesRepository(ILogger<InMemoryEmpSchedulesRepository> logger, IConfiguration configuration, IFileService fileService, IHubContext<HubServices> hubServices)
    {
        _fileService = fileService;
        _logger = logger;
        _configuration = configuration;
        _hubServices = hubServices;
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
                                    sch.ETour = item[12]!.ToString();
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
                                ETour = item[12]!.ToString(),
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
                            ETour = item[12]!.ToString(),
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
                        ETour = item[12]!.ToString(),
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

}