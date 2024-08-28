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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transaction"></param>
        // POST api/<SMSTransactionsController>
        [HttpPost]
        [Route("BadgeScan")]
        public void Post([FromBody] JObject transaction)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                BadRequest(ModelState);
            }
            _tags.UpdateBadgeTransactionScan(transaction);
        }
    }
}
