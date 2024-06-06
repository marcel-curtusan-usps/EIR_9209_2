using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MPESummaryController(ILogger<MPESummaryController> logger, IInMemoryGeoZonesRepository zonesRepository) : ControllerBase
    {
        private readonly IInMemoryGeoZonesRepository _zones = zonesRepository;
        private readonly ILogger<MPESummaryController> _logger = logger;

        // GET: api/<MPESummaryController>
        [HttpGet]
        [Route("/MPESummary")]
        public async Task<object> GetByMPE(string mpe)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _zones.getMPESummary(mpe);
        }


    }
}
