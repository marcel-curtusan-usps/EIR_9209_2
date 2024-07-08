using EIR_9209_2.DataStore;
using EIR_9209_2.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteInformation(IInMemorySiteInfoRepository siteInfo, ILogger<SiteInformation> logger) : ControllerBase
    {
        private readonly IInMemorySiteInfoRepository _siteInfo = siteInfo;
        private readonly ILogger<SiteInformation> _logger = logger;
        // GET: api/<SiteInformationController>

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nassCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSiteInfo")]
        public async Task<object> Get(string nassCode)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(_siteInfo.GetByNASSCode(nassCode));
        }
    }
}
