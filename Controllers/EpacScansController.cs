using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using EIR_9209_2.DataStore;
using NuGet.Protocol;
namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EpacScansController(ILogger<EpacScansController> logger, IInMemoryGeoZonesRepository zones, IInMemoryEmployeesRepository employees, IHubContext<HubServices> hubContext, IInMemoryTACSReports tacs) : ControllerBase
    {
        private readonly IInMemoryEmployeesRepository _employees = employees;
        private readonly IInMemoryTACSReports _tacs = tacs;
        private readonly IHubContext<HubServices> _hubContext = hubContext;
        private readonly ILogger<EpacScansController> _logger = logger;
        private readonly IInMemoryGeoZonesRepository _zones = zones;
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
                    //log the scan 

                    var scanDeviceId = scan["deviceId"]?.ToString();
                    var scanId = scan["id"]?.ToString();
                    var kioskConfig = await _zones.CheckKioskZone(scanDeviceId);

                    if (scanDeviceId != null && kioskConfig.IsFound)
                    {
                        await _hubContext.Clients.Group("CRS").SendAsync("epacScan",
                         new
                         {
                             kioskId = kioskConfig.KioskId,
                             kioskName = kioskConfig.KioskName,
                             kioskNumber = kioskConfig.KioskNumber,
                             deviceId = scanDeviceId,
                             id = scanId
                         },
                         CancellationToken.None);
                    }
                    else
                    {

                        _logger.LogInformation($"Device Id {scan["deviceId"]}");
                    }

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

