using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QREReportResultController : ControllerBase
    {
        private readonly ILogger<QREReportResultController> _logger;
        private readonly IInMemoryGeoZonesRepository _zones;

        public QREReportResultController(ILogger<QREReportResultController> logger, IInMemoryGeoZonesRepository zones)
        {
            _logger = logger;
            _zones = zones;
        }

        /// <summary>
        /// Adds a new report.
        /// </summary>
        /// <param name="reportRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> AddReport([FromBody] JsonElement reportRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (reportRequest.ValueKind != JsonValueKind.Object)
                {
                    return BadRequest("Invalid report data format.");
                }
                //var report = await _zones.AddReportContentItem(reportRequest);
                return Ok(new
                {
                    message = "report added successfully.",
                    data = reportRequest
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return BadRequest(new { message = e.Message });
            }
        }
    }
}
