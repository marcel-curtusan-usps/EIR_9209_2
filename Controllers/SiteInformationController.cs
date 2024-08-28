using EIR_9209_2.DataStore;
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

        //get site information
        [HttpGet]
        [Route("SiteInfo")]
        public IActionResult Get()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var siteInformation = _siteInfo.GetSiteInfo();
                return Ok(siteInformation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting site information");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
