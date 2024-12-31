using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using EIR_9209_2.DataStore;
namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EpacScansController(ILogger<EpacScansController> logger, IInMemoryEmployeesRepository employees, IHubContext<HubServices> hubContext, IInMemoryTACSReports tacs) : ControllerBase
    {
        private readonly IInMemoryEmployeesRepository _employees = employees;
        private readonly IInMemoryTACSReports _tacs = tacs;
        private readonly IHubContext<HubServices> _hubContext = hubContext;
        private readonly ILogger<EpacScansController> _logger = logger;
        // POST api/<EpacScansController>
        [HttpPost]
        [Route("BadgeScan")]
        public async Task<ActionResult> PostAddRawRings([FromBody] JObject scan)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (scan.HasValues)
                {
                    
                    await _hubContext.Clients.Group("CRS").SendAsync("epacScan", scan, CancellationToken.None);
                }
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
    }   
}

