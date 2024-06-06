using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MPERunActivityController(ILogger<MPERunActivityController> logger, IInMemoryGeoZonesRepository zones) : ControllerBase
    {
        private readonly IInMemoryGeoZonesRepository _zones = zones;
        private readonly ILogger<MPERunActivityController> _logger = logger;
        // GET: api/<MPERunActivityController>
        [HttpGet]
        [Route("/MPERunActivity")]
        public async Task<object> GetByMPE(string mpe)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            IEnumerable<JObject> plan = new List<JObject>{
                            new JObject {
                            ["sortPlanName"] = "none",
                            ["startToEndtime"] = new JArray("1900-01-01T00:00:00", "1900-01-01T00:00:00"),
                            ["mpeName"] = "none",
                            ["mpeNumber"] = 0,
                            ["actualVolume"] = 0,
                            ["actualThroughput"] = 0,
                            ["opn"] = 000,
                            ["type"] = "Plan"
                        }};
            IEnumerable<JObject> run = new List<JObject>{
                            new JObject {
                            ["sortPlanName"] = "none",
                            ["startToEndtime"] = new JArray("1900-01-01T00:00:00", "1900-01-01T00:00:00"),
                            ["mpeName"] = "none",
                            ["mpeNumber"] = 0,
                            ["opn"] = 000,
                            ["actualVolume"] = 0,
                            ["actualThroughput"] = 0,
                            ["type"] = "Run"
                        }};
            IEnumerable<JObject> standard = new List<JObject>{
                         new JObject {
                         ["sortPlanName"] = "none",
                         ["startToEndtime"] = new JArray("1900-01-01T00:00:00", "1900-01-01T00:00:00"),
                         ["mpeName"] = "none",
                         ["mpeNumber"] = 0,
                         ["actualVolume"] = 0,
                         ["actualThroughput"] = 0,
                         ["opn"] = 000,
                         ["type"] = "Standard"
                        }};
            var runquery = _zones.getMPERunActivity(mpe);
            if (runquery.Any())
            {
                run = from re in runquery
                      select new JObject
                      {
                          ["sortPlanName"] = re.CurSortplan,
                          ["startToEndtime"] = new JArray(re.CurrentRunStart, re.CurrentRunEnd),
                          ["activeRun"] = re.ActiveRun,
                          ["mpeName"] = re.MpeId,
                          ["mpeNumber"] = re.MpeNumber,
                          ["expectedPiecesFed"] = re.RpgEstVol,
                          ["expectedThruput"] = re.RpgExpectedThruput,
                          ["opn"] = re.CurOperationId,
                          ["actualVolume"] = re.TotSortplanVol,
                          ["actualThroughput"] = re.CurThruputOphr,
                          ["type"] = "Run"
                      };
            }
            return plan.Concat(run).Concat(standard);
        }

    }
}
