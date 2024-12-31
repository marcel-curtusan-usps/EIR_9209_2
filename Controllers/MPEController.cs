using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MPEController(ILogger<MPEController> logger, IInMemoryGeoZonesRepository zonesRepository, IHubContext<HubServices> hubContext) : ControllerBase
    {
        private readonly IInMemoryGeoZonesRepository _zonesRepository = zonesRepository;
        private readonly IHubContext<HubServices> _hubContext = hubContext;
        private readonly ILogger<MPEController> _logger = logger;
        // GET: api/<MPEController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
 
        [HttpGet]
        [Route("MPEGroups")]
        public async Task<object> GetByMPEGroupList(string type)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return await Task.FromResult(BadRequest(ModelState));
                }
                return Ok(await _zonesRepository.GetMPEGroupList(type));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }

        }
        [HttpGet]
        [Route("MPEStandard")]
        public async Task<object> GetByMPEStandardList(string name)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return await Task.FromResult(BadRequest(ModelState));
                }
                return Ok(await _zonesRepository.GetMPEGroupList(name));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }

        }
    }
}
