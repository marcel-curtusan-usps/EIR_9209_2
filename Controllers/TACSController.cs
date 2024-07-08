using EIR_9209_2.DataStore;
using EIR_9209_2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Globalization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TACSController(ILogger<TACSController> logger, IInMemoryTACSReports reports) : ControllerBase
    {

        private readonly IInMemoryTACSReports _reports = reports;
        private readonly ILogger<TACSController> _logger = logger;

        //// GET: api/<TACSController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/<TACSController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/<TACSController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        // POST api/<TACSController>
        /// <summary>
        /// Upload TACS Employee All for pay period report
        /// </summary>
        /// <param name="file Name"></param>
        /// <returns>WebEOR Data has been Loaded</returns>
        /// <remarks>
        /// </remarks>
        /// <response code="201">Returns When WebEOR Data has been Loaded</response>
        /// <response code="400">If the File name was provided </response>
        [HttpPost]
        [Route("UploadEmployeeForPayPeriod")]
        public async Task<IActionResult> UploadTACSEmployeeForPayPeriodCSV(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    List<TACSEmployeePayPeirod> employeePayPeirods = [];
                    // Read the CSV file and process the data
                    var fileContent = await reader.ReadToEndAsync();

                    // Split the file content into lines
                    var lines = fileContent.Split('\n');
                    var isFirstLine = true; // Flag to skip the first line
                                            // Loop through the lines and process the data
                    var isSecondLine = true; // Flag to skip the Second line
                                             // Loop through the lines and process the data
                    var isThrerdLine = true; // Flag to skip the 3rd line
                                             // Loop through the lines and process the data
                    foreach (var line in lines)
                    {
                        if (isFirstLine)
                        {
                            isFirstLine = false;
                            continue; // Skip the first line
                        }
                        if (isSecondLine)
                        {
                            isSecondLine = false;
                            continue; // Skip the first line
                        }
                        if (isThrerdLine)
                        {
                            isThrerdLine = false;
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
                        // Validate the data from each field
                        if (fields.Length == 37) // Check the number of fields
                        {
                            if (!int.TryParse(fields[1], out int financeNo))
                            {
                                return BadRequest("Field 0 is not a number");
                            }
                            if (!int.TryParse(fields[3], out int subUnit))
                            {
                                return BadRequest("Field 2 is not a number");
                            }

                            employeePayPeirods.Add(new TACSEmployeePayPeirod
                            {
                                YrPPWk = fields[0],
                                HireFinanceNo = financeNo,
                                Organization = fields[2],
                                SubUnit = subUnit,
                                EIN = fields[4],
                                LastName = fields[5],
                                FI = fields[6],
                                MI = fields[7],
                                PayLocFinUnit = fields[8],
                                VarEAS = fields[9],
                                Borrowed = fields[10],
                                AutoHL = fields[11],
                                AnnualLvBal = fields[12],
                                SickLvBal = fields[13],
                                LWOP = fields[14],
                                FMLAHrs = fields[15],
                                FMLAUsed = fields[16],
                                SLDCUsed = fields[17],
                                DefaultOPCode = fields[18],
                                TACSCode = fields[19],
                                TACSDate = fields[20],
                                TACSTime = fields[21],
                                TACSDateTime = ConvertStringToDate(fields[20] + " " + fields[21])
                            });
                        }
                        if (fields.Length == 39) // Check the number of fields
                        {
                            continue;
                        }
                        if (fields.Length == 22) // Check the number of fields
                        {
                            continue;
                        }
                    }
                    if (employeePayPeirods.Any())
                    {
                        _reports.AddEmployeePayPeirods(employeePayPeirods);
                    }
                }

                return Ok(new JObject { ["message"] = "TACS Employee For Pay Period data was uploaded successfully." });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //// PUT api/<TACSController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<TACSController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
        private DateTime ConvertStringToDate(string dateString)
        {
            try
            {


                // Split the date and time portions
                string[] parts = dateString.Split(' ');
                // Add a period if there is no period in field 21
                if (!parts[1].Contains("."))
                {
                    parts[1] += ".0";
                }
                if (parts[1].StartsWith("."))
                {
                    string temp = parts[1];
                    parts[1] = $"0{temp}";
                }

                // Parse the date portion
                DateTime dt = DateTime.ParseExact(parts[0], "dd-MMM-yy", CultureInfo.InvariantCulture);

                // Parse the time portion
                string[] timeParts = parts[1].Split('.');
                int hours = int.Parse(timeParts[0]);
                //int minutes = int.Parse(timeParts[1]) / 100; 
                int minutes = (int)Math.Round((decimal)(int.Parse(timeParts[1]) * 60) / 100);
                var currentHour = new DateTime(dt.Year, dt.Month, dt.Day, hours, minutes, 0, DateTimeKind.Local);
                return currentHour;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return DateTime.MinValue;
            }
        }
    }
}
