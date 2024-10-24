using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Pkcs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MPETragetsController(ILogger<MPETragetsController> logger, IInMemoryGeoZonesRepository geoZone, IHubContext<HubServices> hubContext) : ControllerBase
    {

        private readonly IInMemoryGeoZonesRepository _geoZone = geoZone;
        private readonly IHubContext<HubServices> _hubContext = hubContext;
        private readonly ILogger<MPETragetsController> _logger = logger;


        // GET: api/<MpeTragetsController>
        [HttpGet]
        [Route("GetAllMPETarges")]
        public async Task<object> Get()
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return await _geoZone.GetAllMPETragets();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        // GET api/<MpeTragetsController>/5
        [HttpGet]
        [Route("GetByMPE")]
        public async Task<object> GetByMPE(string mpeId)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return await _geoZone.GetMPETargets(mpeId);

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        // POST api/<MpeTragetsController>
        [HttpPost]
        [Route("Add")]
        public async Task<object> Post([FromBody] JToken mpeData)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return await _geoZone.AddMPETargets(mpeData);

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        // PUT api/<MpeTragetsController>/5
        [HttpPut]
        [Route("Update")]
        public async Task<object> Put([FromBody] JToken mpeData)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return await _geoZone.UpdateMPETargets(mpeData);

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        // DELETE api/<MpeTragetsController>/5
        [HttpDelete]
        [Route("Delete")]
        public async Task<object> Delete(string mpeData)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return await _geoZone.RemoveMPETargets(mpeData);

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
    }
}
