using EIR_9209_2.DataStore;
using EIR_9209_2.Service;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

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
                    return Ok(await _employees.GetEmployeeByCode(code));
                }
                else
                {
                    return BadRequest("EncodedId or EmployeeId is required");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
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
        public async Task<ActionResult> Post([FromBody] JObject crsEvent)
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
                    return Ok(await _tacs.AddTacsRawRings(crsEvent));
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
