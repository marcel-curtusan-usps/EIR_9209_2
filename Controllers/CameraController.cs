using EIR_9209_2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    /// <summary>
    /// Camera Controller
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="cameras"></param>
    /// <param name="hubContext"></param> <summary>
    /// 
    /// </summary>
    /// <typeparam name="Camera"></typeparam>
    [Route("api/[controller]")]
    [ApiController]
    public class Camera(ILogger<Camera> logger, IInMemoryCamerasRepository cameras, IHubContext<HubServices> hubContext) : ControllerBase
    {
        private readonly IInMemoryCamerasRepository _cameras = cameras;
        private readonly IHubContext<HubServices> _hubContext = hubContext;
        private readonly ILogger<Camera> _logger = logger;
        

        /// <summary>
        /// Get list of Cameras
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("List")]
        public async Task<object> GetAllCamerasTags()
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return Ok(await _cameras.GetAll());
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
          
        }
        [HttpGet]
        [Route("CameraByFloorId")]
        public async Task<object> GetCameraByFloorId(string floorId, string type)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var (found, cameras) = await _cameras.GetCameraByFloorId(floorId, type);
                if (found)
                {
                    return Ok(cameras);
                }
                else
                {
                    return NotFound($"No cameras found for FloorId: {floorId} and Type: {type}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        /// <summary>
        /// Update Camera Direction by Id
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateCameraDirection")]
        public async Task<object> UpdateCameraDirection(JObject payload)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var id = payload["id"]?.ToString();
                var direction = payload["direction"]?.ToObject<int>() ?? 0;

                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("ID is required.");
                }

                var (found, cameras) = await _cameras.UpdateCameraDirectionById(id, direction);
                if (found)
                {
                    return Ok(cameras);
                }
                else
                {
                    return NotFound($"No cameras found for Id: {id}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        ///  Adds a new camera based on the provided JSON object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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
                var ipValue = value["properties"]?["ip"]?.ToString();
                if (string.IsNullOrEmpty(ipValue))
                {
                    return BadRequest("IP value is missing or invalid.");
                }
                var cameraFromList = await _cameras.GetCameraListByIp(ipValue);
                if (cameraFromList != null)
                {
                    //convert the JObject to a Connection object
                    CameraGeoMarker cameraMarker = value.ToObject<CameraGeoMarker>();

                    cameraMarker.Properties.ModelNum = cameraFromList.ModelNum;
                    cameraMarker.Properties.AuthKey = cameraFromList.AuthKey;
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
                    var addCamera = await _cameras.Add(cameraMarker);
                    if (addCamera != null)
                    {
                        await _hubContext.Clients.Group(addCamera.Properties.Type).SendAsync($"add{addCamera.Properties.Type}", addCamera);

                        return Ok(addCamera);
                    }
                    else
                    {
                        return new JObject { ["Message"] = $"CameraMarker Id:{cameraMarker.Properties.Id} was not Found" };
                    }
                }
                else
                {
                    var ip = value["Properties"]?["IP"]?.ToString();
                    return new JObject { ["Message"] = $"Camera with IP:{ip ?? "unknown"} was not Found" };
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
        public async Task<object> GetCameraList()
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
        /// <summary>
        ///  Deletes a camera by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Delete")]
        public async Task<object> Delete(string id)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var removeCamera = await _cameras.Delete(id);
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
