using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MPESummaryController(ILogger<MPESummaryController> logger, IInMemoryGeoZonesRepository zonesRepository) : ControllerBase
    {
        private readonly IInMemoryGeoZonesRepository _zones = zonesRepository;
        private readonly ILogger<MPESummaryController> _logger = logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mpe"></param>
        /// <returns></returns>
        // GET: api/<MPESummaryController>
        [HttpGet]
        [Route("GetByMPEName")]
        public async Task<object> GetByMPE(string mpe)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _zones.getMPESummary(mpe);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mpe"></param>
        /// <param name="endDateTime"></param>
        /// <param name="startDateTime" ></param>
        /// <returns></returns>
        // GET: api/<MPESummaryController>
        [HttpGet]
        [Route("MPENameDatetime")]
        public async Task<object> GetByMPEDatetime(string mpe, string startDateTime, string endDateTime)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return await Task.FromResult(BadRequest(ModelState));
                }
                DateTime startDT = DateTime.Parse(startDateTime);
                DateTime endDT = DateTime.Parse(endDateTime);

                return await _zones.getMPESummaryDateRange(mpe, startDT, endDT);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting MPE Summary");
                return null;
            }

        }


    }
}
