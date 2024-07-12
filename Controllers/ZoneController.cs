using EIR_9209_2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZoneController(IInMemoryGeoZonesRepository zonesRepository) : ControllerBase
    {
        private readonly IInMemoryGeoZonesRepository _zonesRepository = zonesRepository;

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
        [Route("Id")]
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
        public async Task<object> GetByZoneNameList(string zoneType)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _zonesRepository.GetZoneNameList(zoneType);
        }
        // POST api/<ZoneController>
        [HttpPost]
        [Route("Add")]
        public async Task<object> PostByAddNewZone([FromBody] JObject zone)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            GeoZone newZone = zone.ToObject<GeoZone>();
            newZone.Properties.Id = Guid.NewGuid().ToString();

            _zonesRepository.Add(newZone);
            return Ok();
        }
        // DELETE api/<ZoneController>/5
        [HttpDelete]
        [Route("Delete")]
        public async Task<object> Delete(string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            _zonesRepository.Remove(id);
            return Ok();
        }
    }
}
