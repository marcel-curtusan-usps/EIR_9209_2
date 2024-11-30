using EIR_9209_2.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;



// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Connections(ILogger<Connections> logger, IInMemoryConnectionRepository connectionRepository, IHubContext<HubServices> hubContext, Worker worker) : ControllerBase
    {
        private readonly IInMemoryConnectionRepository _connectionRepository = connectionRepository;
        private readonly IHubContext<HubServices> _hubContext = hubContext;
        private readonly Worker _worker = worker;
        private readonly ILogger<Connections> _logger = logger;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // GET: api/<Connection>
        [HttpGet]
        [Route("AllConnection")]
        public async Task<ActionResult> GetByAllConnection()
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(await _connectionRepository.GetAll());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET api/<Connection>/5
        [HttpGet]
        [Route("ConnectionId")]
        public async Task<ActionResult> GetByConnectionId(string id)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(await _connectionRepository.Get(id));
        }

        // POST api/<Connection>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        public async Task<ActionResult> PostAddConnection([FromBody] JObject value)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //convert the JObject to a Connection object
                Connection connection = value.ToObject<Connection>();
                //add the connection id
                connection.Id = Guid.NewGuid().ToString();
                connection.CreatedDate = DateTime.Now;
                //add to the connection repository
                var addCon = await _connectionRepository.Add(connection);
                if (addCon != null)
                {
                    //add the connection to the worker
                    if (_worker.AddEndpoint(addCon))
                    {
                        return Ok(addCon);
                    }
                    else
                    {
                        return BadRequest(ModelState);
                    }
                }
                else
                {
                    return BadRequest(new JObject { ["message"] = "End Point was not Added " });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        /// <summary>
        /// update connection 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // POST api/<Connection>
        [HttpPost]
        [Route("Update")]
        public async Task<ActionResult> PostUpdateConnection([FromBody] JObject value)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var connectionToUpdate = value.ToObject<Connection>();
                if (await _worker.UpdateEndpoint(connectionToUpdate))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(ModelState);
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
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE api/<Connection>/5
        [HttpDelete]
        [Route("Delete")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var connection = await _connectionRepository.Get(id);
                connection.ActiveConnection = false;
                if (await _worker.UpdateEndpoint(connection))
                {
                    var conn = await _connectionRepository.Remove(id);
                    if (conn != null && _worker.RemoveEndpoint(connection))
                    {
                        await _hubContext.Clients.Group("Connections").SendAsync("DeleteConnection", id);
                    }
                    return Ok(connection);
                }
                else
                {
                    return BadRequest(new JObject { ["Message"] = $"Connection Id:{id} was not Found" });
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
