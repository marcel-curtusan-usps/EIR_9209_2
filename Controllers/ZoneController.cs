using EIR_9209_2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;

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
        [Route("GetZoneNameList")]
        public async Task<object> GetByZoneNameList(string type)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return await _zonesRepository.GetZoneNameList(type);
        }
        [HttpGet]
        [Route("ZoneType")]
        public async Task<object> GetByZoneTypeList(string type)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return await _zonesRepository.GetGeoZone(type);
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
                if (zone["properties"]?["type"]?.ToString() == "DockDoor")
                {
                    GeoZoneDockDoor newDockDoorZone = zone.ToObject<GeoZoneDockDoor>();
                    newDockDoorZone.Properties.Id = Guid.NewGuid().ToString();

                    var dockDoorZone = await _zonesRepository.AddDockDoor(newDockDoorZone);
                    if (dockDoorZone != null)
                    {
                        await _hubContext.Clients.Group(dockDoorZone.Properties.Type).SendAsync($"add{dockDoorZone.Properties.Type}zone", dockDoorZone);
                        return Ok(dockDoorZone);
                    }
                    else
                    {
                        return BadRequest(new JObject { ["message"] = "Dock door zone was not added" });
                    }
                }
                else if (zone["properties"]?["type"]?.ToString() == "Kiosk")
                {
                    GeoZoneKiosk newZone = zone.ToObject<GeoZoneKiosk>();
                    newZone.Properties.Id = Guid.NewGuid().ToString();
                    newZone.Properties.KioskId = string.Concat(newZone.Properties.Name,"-", newZone.Properties.Number.PadLeft(3,'0'));
                    var geoZone = await _zonesRepository.AddKiosk(newZone);
                    if (geoZone != null)
                    {
                        await _hubContext.Clients.Group(geoZone.Properties.Type).SendAsync($"add{geoZone.Properties.Type}zone", geoZone);
                        return Ok(geoZone);
                    }
                    else
                    {
                        return BadRequest(new JObject { ["message"] = "Zone was not added" });
                    }
                }
                else
                {
                    GeoZone newZone = zone.ToObject<GeoZone>();
                    newZone.Properties.Id = Guid.NewGuid().ToString();

                    var geoZone = await _zonesRepository.Add(newZone);
                    if (geoZone != null)
                    {
                        await _hubContext.Clients.Group(geoZone.Properties.Type).SendAsync($"add{geoZone.Properties.Type}zone", geoZone);
                        return Ok(geoZone);
                    }
                    else
                    {
                        return BadRequest(new JObject { ["message"] = "Zone was not added" });
                    }
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
                if (zone.ContainsKey("type") && zone["type"]?.ToString() == "DockDoor")
                {
                    GeoZoneDockDoor? updatedDockDoorZone = zone?.ToObject<GeoZoneDockDoor>();

                    var dockDoorZone = await _zonesRepository.UpdateDockDoor(updatedDockDoorZone);
                    if (dockDoorZone != null)
                    {
                        await _hubContext.Clients.Group(dockDoorZone.Properties.Type).SendAsync($"update{dockDoorZone.Properties.Type}zone", dockDoorZone);
                        return Ok(dockDoorZone);
                    }
                    else
                    {
                        return BadRequest(new JObject { ["message"] = "Dock door zone was not updated" });
                    }
                }

                else if (zone.ContainsKey("type") && zone["type"]?.ToString() == "Kiosk")
                {
                    KioskProperties? updatedKioskZone = zone?.ToObject<KioskProperties>();

                    var KioskZone = await _zonesRepository.UpdateKiosk(updatedKioskZone);
                    if (KioskZone != null)
                    {
                        await _hubContext.Clients.Group(KioskZone.Properties.Type).SendAsync($"update{KioskZone.Properties.Type}zone", KioskZone);
                        return Ok(KioskZone);
                    }
                    else
                    {
                        return BadRequest(new JObject { ["message"] = "Dock door zone was not updated" });
                    }
                }
                else
                {
                    Properties updatedZone = zone?.ToObject<Properties>();

                    var geoZone = await _zonesRepository.UiUpdate(updatedZone);
                    if (geoZone != null)
                    {
                       await _hubContext.Clients.Group(geoZone.Properties.Type).SendAsync($"update{geoZone.Properties.Type}zone", geoZone);
                        return Ok(geoZone);
                    }
                    else
                    {
                        return BadRequest(new JObject { ["message"] = "Zone was not updated" });
                    }
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


                var geoZone = await _zonesRepository.Remove(id);
                var dockDoorZone = await _zonesRepository.RemoveDockDoor(id);
                var kioskZone = await _zonesRepository.RemoveKiosk(id);
                if (dockDoorZone != null)
                {
                    await _hubContext.Clients.Group(dockDoorZone.Properties.Type).SendAsync($"delete{dockDoorZone.Properties.Type}zone", dockDoorZone);
                    return Ok(dockDoorZone);
                }
                else if (geoZone != null)
                {
                    await _hubContext.Clients.Group(geoZone.Properties.Type).SendAsync($"delete{geoZone.Properties.Type}zone", geoZone);
                    return Ok(geoZone);
                }
                else if (kioskZone != null)
                {
                    await _hubContext.Clients.Group(kioskZone.Properties.Type).SendAsync($"delete{kioskZone.Properties.Type}zone", geoZone);
                    return Ok(kioskZone);
                }
                else
                {
                    return BadRequest(new JObject { ["message"] = "Zone was not removed" });
                }


            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
    }
}
