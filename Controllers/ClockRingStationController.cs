using EIR_9209_2.DataStore;
using EIR_9209_2.Models;
using EIR_9209_2.Service;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClockRingStationController (ILogger<ClockRingStationController> logger, IInMemoryEmployeesRepository employees, IHubContext<HubServices> hubContext, IInMemoryTACSReports tacs) : ControllerBase
    {

        private readonly IInMemoryEmployeesRepository _employees = employees;
        private readonly IInMemoryTACSReports _tacs = tacs;
        private readonly IHubContext<HubServices> _hubContext = hubContext;
        private readonly ILogger<ClockRingStationController> _logger = logger;
        // GET: api/<ClockRingStationController>
        [HttpGet]
        [Route("GetByEIN")]
        public async Task<ActionResult> Get(string code)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (!string.IsNullOrEmpty(code))
                {
                    var employee = await _employees.GetEmployeeByCode(code);
                    var empRawRings = await _tacs.GetTACSRawRings(code);
                    var empTopOpnCode = await _tacs.GetTopOpnCodes(code);
                    if (employee != null)
                    {
                        var empAndRawRings = new
                        {
                            Employee = employee,
                            RawRings = empRawRings,
                            TopOpnCodes = empTopOpnCode
                        };
                        return Ok(empAndRawRings);
                    }
                    else
                    {
                        return StatusCode(404, new { Message = "Employee not found" });
                    }
                }
                else
                {
                    return BadRequest("EncodedId or EmployeeId is required");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(new { Message = e.Message });
            }
        }

        // GET api/<ClockRingStationController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ClockRingStationController>
        [HttpPost]
        [Route("AddRawRings")]
        public async Task<ActionResult> PostAddRawRings([FromBody] JObject crsEvent)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (crsEvent.HasValues)
                {
                    RawRings? rawRings = crsEvent?.ToObject<RawRings>();
                    if (rawRings == null)
                    {
                        return BadRequest("CRS Event conversion failed");
                    }
               
                    return Ok(await _tacs.AddTacsRawRings(rawRings));
                }
                else
                {
                    return BadRequest("CRS Event has no data ");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        // PUT api/<ClockRingStationController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ClockRingStationController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
