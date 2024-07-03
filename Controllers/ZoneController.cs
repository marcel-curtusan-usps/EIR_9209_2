using EIR_9209_2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZoneController(IInMemoryGeoZonesRepository zonesRepository, IHubContext<HubServices> hubServices) : ControllerBase
    {
        private readonly IInMemoryGeoZonesRepository _zonesRepository = zonesRepository;
        private readonly IHubContext<HubServices> _hubServices = hubServices;

        // GET: api/<ZoneController>
        [HttpGet]
        [Route("AllZones")]
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
        [Route("ZoneId")]
        public async Task<object> GetByZoneId(string id)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _zonesRepository.Get(id);
        }
        [HttpGet]
        [Route("MpeName")]
        public async Task<object> GetByMpeName(string id)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _zonesRepository.GetMPEName(id);
        }
        [HttpGet]
        [Route("GetZoneNameList")]
        public async Task<object> GetByZoneNameList(string ZoneType)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _zonesRepository.GetZoneNameList(ZoneType);
        }
        // POST api/<ZoneController>
        [HttpPost]
        [Route("AddZone")]
        public async Task<object> PostByAddNewZone([FromBody] JObject zone)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            GeoZone newZone = zone.ToObject<GeoZone>();
            newZone.Properties.Id = Guid.NewGuid().ToString();

            _zonesRepository.Add(newZone);

            if (zone.ContainsKey("zoneType") && zone["zoneType"].ToString() == "MPEBinZones")
            {
                await _hubServices.Clients.Group("MPEBinZones").SendAsync("AddMPEBinZones", newZone);
            }
            return Ok(newZone);
        }

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
