using CsvHelper;
using CsvHelper.Configuration;
using EIR_9209_2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System.Globalization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MPETragetsController(ILogger<MPETragetsController> logger, IInMemoryGeoZonesRepository geoZone, IHubContext<HubServices> hubContext) : ControllerBase
    {

        private readonly IInMemoryGeoZonesRepository _geoZone = geoZone;
        private readonly IHubContext<HubServices> _hubContext = hubContext;
        private readonly ILogger<MPETragetsController> _logger = logger;


        // GET: api/<MpeTragetsController>
        [HttpGet]
        [Route("AllMPETarges")]
        public async Task<object> Get()
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return await _geoZone.GetAllMPETragets();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        // GET api/<MpeTragetsController>/5
        [HttpGet]
        [Route("MPETargets")]
        public async Task<object> GetByMPE(string Name)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return await _geoZone.GetMPETargets(Name);

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mpeData"></param>
        /// <returns></returns>
        // POST api/<MpeTragetsController>
        [HttpPost]
        [Route("Add")]
        public async Task<object> Post([FromBody] JToken mpeData)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var response = await _geoZone.AddMPETargets(mpeData);
                if (response != null)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest();
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        // PUT api/<MpeTragetsController>/5
        [HttpPut]
        [Route("Update")]
        public async Task<object> Put([FromBody] JToken mpeData)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _geoZone.UpdateMPETargets(mpeData);
                if (response != null)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest();
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        // DELETE api/<MpeTragetsController>/5
        [HttpDelete]
        [Route("Delete")]
        public async Task<object> Delete([FromBody] JToken mpeData)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var response = await _geoZone.RemoveMPETargets(mpeData);
                if (response != null)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest();
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <remarks>
        /// </remarks>
        /// <response code="201">Returns When WebEOR Data has been Loaded</response>
        /// <response code="400">If the File name was provided </response>
        [HttpPost]
        [HttpPost]
        [Route("UploadTarget")]
        public async Task<IActionResult> UploadTargetData(IFormFile file)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }
                List<TargetHourlyData> targetHourlyDatas = [];
                using var reader = new StreamReader(file.OpenReadStream());
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                csv.Context.RegisterClassMap<TargetHourlyDataMap>();
                List<TargetHourly> targetHourly = csv.GetRecords<TargetHourly>().ToList();
                foreach (var item in targetHourly)
                {
                    TargetHourlyData targetHourlyData = new TargetHourlyData();
                    targetHourlyData.MpeType = item.MpeType;
                    targetHourlyData.MpeNumber = item.MpeNumber;
                    targetHourlyData.MpeId = $"{item.MpeType}-{item.MpeNumber.ToString().PadLeft(3, '0')}";
                    targetHourlyData.Id = $"{item.MpeType}-{item.MpeNumber.ToString().PadLeft(3, '0')}{item.Hour}";
                    targetHourlyData.TargetHour = item.Hour.ToString().PadLeft(5, '0');
                    targetHourlyData.HourlyTargetVol = item.TargetVolume;
                    targetHourlyData.HourlyRejectRatePercent = item.TargetReject;
                    targetHourlyDatas.Add(targetHourlyData);
                }
                var load = await _geoZone.LoadCSVMpeTargets(targetHourlyDatas);

                if (load == false)
                {
                    return BadRequest(new JObject { ["message"] = "Target data Failed to load data." });
                }
                else
                {
                    return Ok(new JObject { ["message"] = "Target data was uploaded successfully." });
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        // DELETE whole tour api/<MpeTragetsController>/5
        [HttpPost]
        [Route("DeleteTour")]
        public async Task<object> DeleteTour([FromBody] JToken mpeData)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return await _geoZone.RemoveMPETargets(mpeData);

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
    }

    internal class TargetHourly
    {
        public string MpeType { get; set; }
        public string MpeNumber { get; set; }
        public string Hour { get; set; }
        public int TargetVolume { get; set; }
        public double TargetReject { get; set; }
    }

    internal class TargetHourlyDataMap : ClassMap<TargetHourly>
    {
        public TargetHourlyDataMap()
        {
            Map(m => m.MpeType).Name("MpeType");
            Map(m => m.MpeNumber).Name("MpeNumber");
            Map(m => m.Hour).Name("Hour");
            Map(m => m.TargetVolume).Name("TargetVolume");
            Map(m => m.TargetReject).Name("TargetReject");
        }
    }
}
