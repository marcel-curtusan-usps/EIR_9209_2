using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KioskController(ILogger<KioskController> logger, IInMemoryGeoZonesRepository zone) : ControllerBase
    {
        private readonly IInMemoryGeoZonesRepository _zones = zone;
        private readonly ILogger<KioskController> _logger = logger;
        // GET: api/<KioskController>
        [HttpGet]
        [Route("KioskList")]
        public async Task<object> GetAllKiosk()
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _zones.GetAllKiosk().Result;
        }
        // GET: api/<KioskController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
    }
}
