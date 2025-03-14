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
            return await _zonesRepository.GetAll();
        }
        [HttpGet]
        [Route("ZonesTypeByFloorId")]
        public async Task<object> GetZonesTypeByFloorId(string floorId, string type)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return await _zonesRepository.GetGeoZonesTypeByFloorId(floorId, type);
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
            return await _zonesRepository.GetGeoZonebyType(type);
        }
        // POST api/<ZoneController>
        [HttpPost]
        [Route("Add")]
        public async Task<ActionResult> PostByAddNewZone([FromBody] JObject zone)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                var zoneType = zone["properties"]?["type"]?.ToString();
                if (zoneType == "DockDoor")
                {
                    return await AddDockDoorZone(zone);
                }
                else if (zoneType == "Kiosk")
                {
                    return await AddKioskZone(zone);
                }
                else if (zoneType == "Cube")
                {
                    return await AddCubeZone(zone);
                }
                else
                {
                    return await AddGenericZone(zone);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return BadRequest(e.Message);
            }
        }

        private async Task<ActionResult> AddDockDoorZone(JObject zone)
        {
            GeoZoneDockDoor? newDockDoorZone = zone?.ToObject<GeoZoneDockDoor>();
            if (newDockDoorZone == null)
            {
                return BadRequest(new { message = "Invalid DockDoor zone data" });
            }
            newDockDoorZone.Properties.Id = Guid.NewGuid().ToString();

            var dockDoorZone = await _zonesRepository.AddDockDoor(newDockDoorZone);
            if (dockDoorZone != null)
            {
                await _hubContext.Clients.Group(dockDoorZone.Properties.Type).SendAsync($"add{dockDoorZone.Properties.Type}zone", dockDoorZone);
                return Ok(dockDoorZone);
            }
            else
            {
                return BadRequest(new { message = "Dock door zone was not added" });
            }
        }

        private async Task<ActionResult> AddKioskZone(JObject zone)
        {
            GeoZoneKiosk? newZone = zone?.ToObject<GeoZoneKiosk>();
            if (newZone == null)
            {
                return BadRequest(new { message = "Invalid Kiosk zone data" });
            }
            newZone.Properties.Id = Guid.NewGuid().ToString();
            newZone.Properties.KioskId = string.Concat(newZone.Properties.Name, "-", newZone.Properties.Number.PadLeft(3, '0'));

            var geoZone = await _zonesRepository.AddKiosk(newZone);
            if (geoZone != null)
            {
                await _hubContext.Clients.Group(geoZone.Properties.Type).SendAsync($"add{geoZone.Properties.Type}zone", geoZone);
                return Ok(geoZone);
            }
            else
            {
                return BadRequest(new { message = "Zone was not added" });
            }
        }
        private async Task<ActionResult> AddCubeZone(JObject zone)
        {
            GeoZoneCube? newZone = zone?.ToObject<GeoZoneCube>();
            if (newZone == null)
            {
                return BadRequest(new { message = "Invalid Kiosk zone data" });
            }
            newZone.Properties.Id = Guid.NewGuid().ToString();
            var geoZone = await _zonesRepository.AddCube(newZone);
            if (geoZone != null)
            {
                await _hubContext.Clients.Group(geoZone.Properties.Type).SendAsync($"add{geoZone.Properties.Type}zone", geoZone);
                return Ok(geoZone);
            }
            else
            {
                return BadRequest(new { message = "Zone was not added" });
            }
        }
        private async Task<ActionResult> AddGenericZone(JObject zone)
        {
            GeoZone? newZone = zone?.ToObject<GeoZone>();
            if (newZone == null)
            {
                return BadRequest(new { message = "Invalid zone data" });
            }
            newZone.Properties.Id = Guid.NewGuid().ToString();

            var geoZone = await _zonesRepository.Add(newZone);
            if (geoZone != null)
            {
                await _hubContext.Clients.Group(geoZone.Properties.Type).SendAsync($"add{geoZone.Properties.Type}zone", geoZone);
                return Ok(geoZone);
            }
            else
            {
                return BadRequest(new { message = "Zone was not added" });
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
                    return await UpdateDockDoorZone(zone);
                }
                else if (zone.ContainsKey("type") && zone["type"]?.ToString() == "Kiosk")
                {
                    return await UpdateKioskZone(zone);
                }
                else
                {
                    return await UpdateGenericZone(zone);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return BadRequest(e.Message);
            }
        }

        private async Task<ActionResult> UpdateDockDoorZone(JObject zone)
        {
            GeoZoneDockDoor? updatedDockDoorZone = zone?.ToObject<GeoZoneDockDoor>();

            if (updatedDockDoorZone == null)
            {
                return BadRequest(new { message = "Invalid DockDoor zone data" });
            }
            var dockDoorZone = await _zonesRepository.UpdateDockDoor(updatedDockDoorZone);
            if (dockDoorZone != null)
            {
                await _hubContext.Clients.Group(dockDoorZone.Properties.Type).SendAsync($"update{dockDoorZone.Properties.Type}zone", dockDoorZone);
                return Ok(dockDoorZone);
            }
            else
            {
                return BadRequest(new { message = "Dock door zone was not updated" });
            }
        }

        private async Task<ActionResult> UpdateKioskZone(JObject zone)
        {
            KioskProperties? updatedKioskZone = zone?.ToObject<KioskProperties>();

            if (updatedKioskZone == null)
            {
                return BadRequest(new { message = "Invalid Kiosk zone data" });
            }
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

        private async Task<ActionResult> UpdateGenericZone(JObject zone)
        {
            Properties? updatedZone = zone?.ToObject<Properties>();
            if (updatedZone == null)
            {
                return BadRequest(new { message = "Invalid zone data" });
            }

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
                var cubeZone = await _zonesRepository.RemoveCube(id);
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
                    await _hubContext.Clients.Group(kioskZone.Properties.Type).SendAsync($"delete{kioskZone.Properties.Type}zone", kioskZone);
                    return Ok(kioskZone);
                }
                else if (cubeZone != null)
                {
                    await _hubContext.Clients.Group(cubeZone.Properties.Type).SendAsync($"delete{cubeZone.Properties.Type}zone", cubeZone);
                    return Ok(cubeZone);
                }
                else
                {
                    return BadRequest(new JObject { ["message"] = "Zone was not removed" });
                }


            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return BadRequest(e.Message);
            }
        }
    }
}
