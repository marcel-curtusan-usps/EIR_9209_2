using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SMSTransactionsController : ControllerBase
    {
        private readonly IInMemoryTagsRepository _tags;

        public SMSTransactionsController(IInMemoryTagsRepository tagsRepository)
        {
            _tags = tagsRepository;
        }

        // GET: api/<SMSTransactionsController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<SMSTransactionsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<SMSTransactionsController>
        [HttpPost]
        public async void Post([FromBody] JObject transaction)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                BadRequest(ModelState);
            }
            _tags.UpdateBadgeTransactionScan(transaction);
        }

        // PUT api/<SMSTransactionsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<SMSTransactionsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
