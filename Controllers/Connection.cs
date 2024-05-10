using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Connection : ControllerBase
    {
        // GET: api/<Connection>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<Connection>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<Connection>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<Connection>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<Connection>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
