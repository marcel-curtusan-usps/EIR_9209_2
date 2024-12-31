using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Globalization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MPERunActivityController(ILogger<MPERunActivityController> logger, IInMemoryGeoZonesRepository zones) : ControllerBase
    {
        private readonly IInMemoryGeoZonesRepository _zones = zones;
        private readonly ILogger<MPERunActivityController> _logger = logger;
        // GET: api/<MPERunActivityController>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mpe"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetByMPEName")]
        public async Task<object> GetByMPE(string mpe)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            //IEnumerable<JObject> plan = new List<JObject>{
            //                new JObject {
            //                ["sortPlanName"] = "none",
            //                ["startToEndtime"] = new JArray("1900-01-01T00:00:00", "1900-01-01T00:00:00"),
            //                ["mpeName"] = "none",
            //                ["mpeNumber"] = 0,
            //                ["actualVolume"] = 0,
            //                ["actualThroughput"] = 0,
            //                ["opn"] = 000,
            //                ["type"] = "Plan"
            //            }};
            //IEnumerable<JObject> run = new List<JObject>{
            //                new JObject {
            //                ["sortPlanName"] = "none",
            //                ["startToEndtime"] = new JArray("1900-01-01T00:00:00", "1900-01-01T00:00:00"),
            //                ["mpeName"] = "none",
            //                ["mpeNumber"] = 0,
            //                ["opn"] = 000,
            //                ["actualVolume"] = 0,
            //                ["actualThroughput"] = 0,
            //                ["type"] = "Run"
            //            }};
            //IEnumerable<JObject> standard = new List<JObject>{
            //             new JObject {
            //             ["sortPlanName"] = "none",
            //             ["startToEndtime"] = new JArray("1900-01-01T00:00:00", "1900-01-01T00:00:00"),
            //             ["mpeName"] = "none",
            //             ["mpeNumber"] = 0,
            //             ["actualVolume"] = 0,
            //             ["actualThroughput"] = 0,
            //             ["opn"] = 000,
            //             ["type"] = "Standard"
            //            }};
            var runquery = _zones.getMPERunActivity(mpe);

            return from re in runquery
                   select new JObject
                   {
                       ["sortPlanName"] = re.CurSortplan,
                       ["startToEndtime"] = new JArray(re.CurrentRunStart, re.CurrentRunEnd),
                       ["activeRun"] = re.ActiveRun,
                       ["mpeName"] = re.MpeId,
                       ["mpeNumber"] = re.MpeNumber,
                       ["expectedPiecesFed"] = re.RpgEstVol,
                       ["expectedThruput"] = re.RpgExpectedThruput,
                       ["opn"] = re.CurOperationId,
                       ["actualVolume"] = re.TotSortplanVol,
                       ["actualThroughput"] = re.CurThruputOphr,
                       ["type"] = re.Type
                   };
        }

        // POST: api/<MPERunActivityController>
        /// <summary>
        /// Upload WebEor Run Data.
        /// </summary>
        /// <param name="file Name"></param>
        /// <returns>WebEOR Data has been Loaded</returns>
        /// <remarks>
        /// </remarks>
        /// <response code="201">Returns When WebEOR Data has been Loaded</response>
        /// <response code="400">If the File name was provided </response>
        [HttpPost]
        [Route("UploadWebEOR")]
        public async Task<IActionResult> UploadCSV(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }
                List<MPEActiveRun> mPEActiveRuns = new List<MPEActiveRun>();
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    // Read the CSV file and process the data
                    // Your code here
                    //Site,MType,MNo,Op No.,Sort Program,Tour,Run#,Start,End,Fed,MODS,DOIS
                    //"97218-9997","AFCS200","101","750000","MODE_098.STF","2","1","06/19/24 13:03","06/19/24 13:04","10","M","NS",
                    //"97218-9997","AFCS200","101","750000","MODE_097.STF","2","2","06/19/24 13:05","06/19/24 13:07","10","M","NS",
                    //"97218-9997","AFCS200","101","750000","MODE_098.STF","2","3","06/19/24 14:23","06/19/24 14:24","5","M","NS",

                    //loop through the CSV file and process the data
                    //this is where you would save the data to the database
                    //or send it to the front end
                    //or do whatever you need to do with the data
                    // Read the CSV file and process the data
                    var fileContent = await reader.ReadToEndAsync();

                    // Split the file content into lines
                    var lines = fileContent.Split('\n');
                    var isFirstLine = true; // Flag to skip the first line
                                            // Loop through the lines and process the data
                    foreach (var line in lines)
                    {
                        if (isFirstLine)
                        {
                            isFirstLine = false;
                            continue; // Skip the first line
                        }
                        // Skip empty lines
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        // Remove quotes and backslashes from the line
                        var cleanLine = line.Replace("\"", "").Replace("\\", "");

                        // Split the line into values
                        var fields = cleanLine.Split(',');

                        // Validate the data from each field
                        if (fields.Length != 13) // Check the number of fields
                        {
                            return BadRequest("Invalid data format");
                        }
                        string MpeName = fields[1].ToString().Trim();
                        if (fields[1].ToString().Trim() == "APBS")
                        {
                            MpeName = "SPBSTS";
                        }
                        // Check if the second field is a number
                        if (!int.TryParse(fields[2], out int mpeNumber)) // Use double.TryParse if it can be a floating-point number
                        {
                            return BadRequest("Field 2 is not a number");
                        }
                        if (MpeName == "ATU" || MpeName == "HSTS" || MpeName == "USS")
                        {
                            if (fields[2].Length == 3)
                            {
                                if (!int.TryParse(fields[2].Substring(1), out mpeNumber)) // Use double.TryParse if it can be a floating-point number
                                {
                                    return BadRequest("Field 2 is not a number");
                                }
                            }
                        }
                        if (!int.TryParse(fields[3].AsSpan(0, 3), out int operationId)) // Use double.TryParse if it can be a floating-point number
                        {
                            return BadRequest("Field 3 is not a number");
                        }
                        string Sortplan = fields[4].ToString();

                        if (!int.TryParse(fields[5], out int tour)) // Use double.TryParse if it can be a floating-point number
                        {
                            return BadRequest("Field 5 is not a number");
                        }
                        if (!int.TryParse(fields[6], out int runNumber)) // Use double.TryParse if it can be a floating-point number
                        {
                            return BadRequest("Field 5 is not a number");
                        }
                        // Convert the seventh field to a DateTime
                        if (!DateTime.TryParseExact(fields[7], "MM/dd/yy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate))
                        {
                            return BadRequest("Field 7 is not a valid date");
                        }
                        // Convert the seventh field to a DateTime
                        if (!DateTime.TryParseExact(fields[8], "MM/dd/yy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
                        {
                            return BadRequest("Field 8 is not a valid date");
                        }
                        if (!int.TryParse(fields[9], out int fed)) // Use double.TryParse if it can be a floating-point number
                        {
                            return BadRequest("Field 9 is not a number");
                        }
                        // Create a new EORData object and populate its properties

                        string mpeid = string.Concat(MpeName, "-", mpeNumber.ToString().PadLeft(3, '0'));
                        mPEActiveRuns.Add(new MPEActiveRun
                        {
                            MpeType = MpeName,
                            MpeNumber = mpeNumber,
                            CurOperationId = operationId,
                            CurSortplan = Sortplan,
                            Tour = tour,
                            CurRunNumber = runNumber,
                            CurrentRunStart = startDate,
                            CurrentRunEnd = endDate,
                            TotSortplanVol = fed,
                            MpeId = mpeid
                        });


                    }
                    if (mPEActiveRuns.Any())
                    {
                        await _zones.LoadWebEORMPERun(JToken.FromObject(mPEActiveRuns));
                    }

                }

                return Ok(new JObject { ["message"] = "WebEOR data was uploaded successfully." });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
