using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZoneController(IInMemoryGeoZonesRepository zonesRepository) : ControllerBase
    {
        private readonly IInMemoryGeoZonesRepository _zonesRepository = zonesRepository;

        // GET: api/<ZoneController>
        [HttpGet]
        [Route("/AllZones")]
        public async Task<object> GetAllZones()
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _zonesRepository.GetAll().Select(r => r.Properties);
        }

        // GET api/<ZoneController>/5
        [HttpGet]
        [Route("/ZoneId")]
        public async Task<object> GetByZoneId(string id)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _zonesRepository.Get(id);
        }
        [HttpGet]
        [Route("/MpeName")]
        public async Task<object> GetByMpeName(string id)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _zonesRepository.GetMPEName(id);
        }
        [HttpGet]
        [Route("/api/GetZoneNameList")]
        public async Task<object> GetByZoneNameList(string ZoneType)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _zonesRepository.GetZoneNameList(ZoneType);
        }
        //// POST api/<ZoneController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<ZoneController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<ZoneController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
