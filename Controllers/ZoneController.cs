using EIR_9209_2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZoneController(ILogger<ZoneController> logger, IInMemoryGeoZonesRepository zonesRepository, IHubContext<HubServices> hubContext) : ControllerBase
    {
        private readonly IInMemoryGeoZonesRepository _zonesRepository = zonesRepository;
        private readonly IHubContext<HubServices> _hubContext = hubContext;
        private readonly ILogger<ZoneController> _logger = logger;

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
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                GeoZone newZone = zone.ToObject<GeoZone>();
                newZone.Properties.Id = Guid.NewGuid().ToString();

                var GeoZone = await _zonesRepository.Add(newZone);
                if (GeoZone != null)
                {
                    await _hubContext.Clients.Group(GeoZone.Properties.ZoneType).SendAsync($"add{GeoZone.Properties.ZoneType}zone", GeoZone);
                    return Ok(GeoZone);
                }
                else
                {
                    return BadRequest(new JObject { ["message"] = "Zone was not Added " });
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        // POST api/<ZoneController>
        [HttpPost]
        [Route("Update")]
        public async Task<object> PostByUpdateZone([FromBody] JObject zone)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                var GeoZone = await _zonesRepository.UiUpdate(zone.ToObject<GeoZone>());
                if (GeoZone != null)
                {
                    await _hubContext.Clients.Group(GeoZone.Properties.ZoneType).SendAsync($"update{GeoZone.Properties.ZoneType}zone", GeoZone);
                    return Ok(GeoZone);
                }
                else
                {
                    return BadRequest(new JObject { ["message"] = "Zone was not Updated " });
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        // DELETE api/<ZoneController>/5
        [HttpDelete]
        [Route("Delete")]
        public async Task<object> Delete(string id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                var GeoZone = await _zonesRepository.Remove(id);

                if (GeoZone != null)
                {
                    await _hubContext.Clients.Group(GeoZone.Properties.ZoneType).SendAsync($"delete{GeoZone.Properties.ZoneType}zone", GeoZone);
                    return Ok(GeoZone);
                }
                else
                {
                    return BadRequest(new JObject { ["message"] = "Zone was not Removes " });
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        [HttpGet]
        [Route("GetTagTimelineList")]
        public async Task<object> GetTagTimelineByEIN(string ein)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _zonesRepository.GetTagTimelineList(ein);
        }
    }
}
