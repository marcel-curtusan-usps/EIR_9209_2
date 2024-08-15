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

        // GET api/<MPEController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
        [HttpGet]
        [Route("MPENames")]
        public async Task<object> GetByMPENameList(string type)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return await Task.FromResult(BadRequest(ModelState));
                }
                return Ok(await _zonesRepository.GetMPENameList(type));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }

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

        // POST api/<MPEController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<MPEController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<MPEController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
