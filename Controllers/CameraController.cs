using EIR_9209_2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System.Web;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CameraController(IInMemoryCamerasRepository cameras, IHubContext<HubServices> hubContext) : ControllerBase
    {
        private readonly IInMemoryCamerasRepository _cameras = cameras;
        private readonly IHubContext<HubServices> _hubContext = hubContext;

        // GET: api/<TagController>
        [HttpGet]
        public async Task<object> GetAsync()
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(_cameras.GetAll());
        }
        //add new camera
        // POST api/<CameraController>
        [HttpPost]
        [Route("Add")]
        public async Task<object> Post([FromBody] CameraMarker camera)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _cameras.Add(camera);

            return Ok();
        }
        /// <summary>
        /// Get Camera by Id
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        // GET api/<TagController>/5
        [HttpGet]
        [Route("GetCameraById")]
        public async Task<object> Get(string id)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return Ok(_cameras.Get(id));
        }

        /// <summary>
        /// Get list of Cameras
        /// </summary>
        /// <param name="cameraName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCameraList")]
        public async Task<object> GetCameraList()
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _cameras.GetCameraListAll();
        }
        /// <summary>
        /// Update Tag Info
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // PUT api/<TagController>/5
        [HttpPost]
        [Route("UpdateCameraInfo")]
        public async Task<object> PutByTagInfo([FromBody] JObject value)
        {
            if (!ModelState.IsValid)
            {
                BadRequest(ModelState);
            }

            //await _cameras.UpdateTagUIInfo(value);

            return Ok();
        }

        // DELETE api/<TagController>/5
        [HttpDelete]
        [Route("DeleteCameraTag")]
        public async Task<object> Delete(string id)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                BadRequest(ModelState);
            }
            //await _cameras.Delete(id);

            return Ok();
        }
    }
}
