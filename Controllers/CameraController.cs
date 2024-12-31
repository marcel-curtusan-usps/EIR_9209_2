using EIR_9209_2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Camera(ILogger<Camera> logger, IInMemoryCamerasRepository cameras, IHubContext<HubServices> hubContext) : ControllerBase
    {
        private readonly IInMemoryCamerasRepository _cameras = cameras;
        private readonly IHubContext<HubServices> _hubContext = hubContext;
        private readonly ILogger<Camera> _logger = logger;
        // GET: api/<TagController>
        [HttpGet]
        public object Get()
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(_cameras.GetAll());
        }
        //add new camera
        // POST api/<Camera>
        [HttpPost]
        [Route("Add")]
        public async Task<object> Post([FromBody] JObject value)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var cameraFromList = _cameras.GetCameraListByIp(value["properties"]["ip"].ToString()).Result;
                if (cameraFromList != null)
                {
                    //convert the JObject to a Connection object
                    CameraGeoMarker cameraMarker = value.ToObject<CameraGeoMarker>();
                    cameraMarker.Properties.Id = cameraFromList.AuthKey;
                    cameraMarker.Properties.ModelNum = cameraFromList.ModelNum;
                    cameraMarker.Properties.FacilityPhysAddrTxt = cameraFromList.FacilityPhysAddrTxt;
                    cameraMarker.Properties.GeoProcRegionNm = cameraFromList.GeoProcRegionNm;
                    cameraMarker.Properties.FacilitySubtypeDesc = cameraFromList.FacilitySubtypeDesc;
                    cameraMarker.Properties.GeoProcDivisionNm = cameraFromList.GeoProcDivisionNm;
                    cameraMarker.Properties.FacilityLatitudeNum = cameraFromList.FacilityLatitudeNum;
                    cameraMarker.Properties.FacilityLongitudeNum = cameraFromList.FacilityLongitudeNum;
                    cameraMarker.Properties.HostName = cameraFromList.HostName;
                    cameraMarker.Properties.Description = cameraFromList.Description;
                    cameraMarker.Properties.Reachable = cameraFromList.Reachable;
                    cameraMarker.Properties.FacilityDisplayName = cameraFromList.FacilityDisplayName;
                    //add the connection id
                    cameraMarker.Properties.Id = Guid.NewGuid().ToString();
                    cameraMarker.Properties.CreatedDate = DateTime.Now;
                    var addCammera = _cameras.Add(cameraMarker).Result;
                    if (addCammera != null)
                    {
                        await _hubContext.Clients.Group(addCammera.Properties.Type).SendAsync($"add{addCammera.Properties.Type}", addCammera);

                        return Ok(addCammera);
                    }
                    else
                    {
                        return new JObject { ["Message"] = $"CameraMarker Id:{cameraMarker.Properties.Id} was not Found" };
                    }
                }
                else
                {
                    return new JObject { ["Message"] = $"Camera with IP:{value["Properties"]["IP"].ToString()} was not Found" };
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        /// <summary>
        /// Get Camera by Id
        /// </summary>
        /// <returns></returns>
        // GET api/<Camera>/5
        [HttpGet]
        [Route("GetById")]
        public async Task<object> Get(string id)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return await Task.FromResult(BadRequest(ModelState));
                }
                return Ok(_cameras.Get(id));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Get list of Cameras
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetList")]
        public object GetCameraList()
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return _cameras.GetCameraListAll();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        // DELETE api/<Camera>/5
        [HttpDelete]
        [Route("Delete")]
        public async Task<object> Delete(string id)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    BadRequest(ModelState);
                }
                var removeCamera = _cameras.Delete(id).Result;
                if (removeCamera != null)
                {
                    await _hubContext.Clients.Group(removeCamera.Properties.Type).SendAsync($"delete{removeCamera.Properties.Type}", removeCamera);
                    return Ok(removeCamera);
                }
                else
                {
                    return new JObject { ["Message"] = $"Connection Id:{id} was not Found" };
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
