using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MPESummaryController : ControllerBase
    {
        private readonly IInMemoryGeoZonesRepository _zonesRepository;

        public MPESummaryController(IInMemoryGeoZonesRepository zonesRepository)
        {
            _zonesRepository = zonesRepository;
        }
        // GET: api/<MPESummaryController>
        [HttpGet]
        public async Task<object> GetByMPE(string mpe)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _zonesRepository.getMPESummary(mpe);
        }


        // GET api/<MPESummaryController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<MPESummaryController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // POST api/<MPESummaryController>
        //[HttpPost]
        //public async Task<object> PostByRunSummary()
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return await Task.FromResult(BadRequest(ModelState));
        //    }
        //    //_zonesRepository.RunMPESummaryReport();
        //    return Ok();
        //}

        // PUT api/<MPESummaryController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<MPESummaryController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
