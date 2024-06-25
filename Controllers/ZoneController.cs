using EIR_9209_2.Models;
using EIR_9209_2.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using static EIR_9209_2.Models.GeoMarker;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZoneController(IInMemoryGeoZonesRepository zonesRepository) : ControllerBase
    {

        private readonly IInMemoryGeoZonesRepository _zonesRepository = zonesRepository;   
        private readonly IHubContext<HubServices> _hubContext;
        //private readonly Worker _worker;
       

        
        // GET: api/<ZoneController>
        [HttpGet]
        [Route("/AllZones")]
        public async Task<object> GetAllZones()
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _zonesRepository.GetAll().Select(r => r.Properties);
        }

        // GET api/<ZoneController>/5
        [HttpGet]
        [Route("/ZoneId")]
        public async Task<object> GetByZoneId(string id)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _zonesRepository.Get(id);
        }
        [HttpGet]
        [Route("/MpeName")]
        public async Task<object> GetByMpeName(string id)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _zonesRepository.GetMPEName(id);
        }
        [HttpGet]
        [Route("/api/GetZoneNameList")]
        public async Task<object> GetByZoneNameList(string ZoneType)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _zonesRepository.GetZoneNameList(ZoneType);
        }

        // GET api/<Connection>/5
        [HttpGet("{id}")]
        public async Task<object> Get(string id)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(_zonesRepository.Get(id));
        }

        // POST api/<Zone>

        [HttpPost]
        [Route("/api/AddZone")]
        /// <summary>
        /// Adds a new zone.
        public async Task<IActionResult> PostAddNewZone([FromBody] JObject zone)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            GeoZone geoZone = zone.ToObject<GeoZone>();
            geoZone.Properties.Id = Guid.NewGuid().ToString();
            GeoZone loadedCon = _zonesRepository.Add(geoZone);

            //if (loadedCon != null)
            if (loadedCon != null && zone.ContainsKey("zonetype") && zone["zonetype"].ToString()== "MPEBinZones")
            {
                await _hubContext.Clients.Group("GeoZone").SendAsync("AddZone", loadedCon);
                return Ok(loadedCon);
            }
            else
            {
                return BadRequest(new JObject { ["message"] = "End Point was not Added " });
            }



        }
        
        //// POST api/<ZoneController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<ZoneController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<ZoneController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
