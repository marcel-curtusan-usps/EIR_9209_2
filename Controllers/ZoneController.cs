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
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<ZoneController>/5
        [HttpGet("{id}")]
        public async Task<object> Get(string id)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _zonesRepository.Get(id);
        }

        // POST api/<ZoneController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ZoneController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ZoneController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
