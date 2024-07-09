using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.SignalR;
using EIR_9209_2.Service;
using NuGet.Protocol.Plugins;
using EIR_9209_2.Models;



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
        public async Task<object> GetByAllConnection()
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(_connectionRepository.GetAll());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET api/<Connection>/5
        [HttpGet]
        [Route("ConnectionId")]
        public async Task<object> GetByConnectionId(string id)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(_connectionRepository.Get(id));
        }

        // POST api/<Connection>
        [HttpPost]
        [Route("Add")]
        /// <summary>
        /// Adds a new connection.
        /// </summary>
        /// <param name="value">The connection details.</param>
        /// <returns>The added connection.</returns>
        public async Task<object> PostAddConnection([FromBody] JObject value)
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
            Connection loadedCon = _connectionRepository.Add(connection);
            if (loadedCon != null)
            {
                //add the connection to the worker
                if (_worker.AddEndpoint(loadedCon))
                {

                    await _hubContext.Clients.Group("Connections").SendAsync("AddConnection", loadedCon);
                    return Ok(loadedCon);
                }
                else
                {
                    return BadRequest(new JObject { ["message"] = "End Point was not Started" });
                }
            }
            else
            {
                return BadRequest(new JObject { ["message"] = "End Point was not Added " });
            }



        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // POST api/<Connection>
        [HttpPost]
        [Route("Update")]
        public async Task<object> PostUpdateConnection([FromBody] JObject value)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //convert the JObject to a Connection object
                //find id to update 
                string id = "";
                if (value.ContainsKey("id") && !string.IsNullOrEmpty(value["id"].ToString()))
                {
                    id = value["id"].ToString();
                }
                else
                {
                    BadRequest(new JObject { ["message"] = "No Connection id Found" });
                }
                Connection conToUpdate = _connectionRepository.Get(id);
                if (conToUpdate != null)
                {
                    Connection connection = value.ToObject<Connection>();
                    connection.LastupDate = DateTime.Now;
                    if (!connection.ActiveConnection)
                    {
                        connection.DeactivatedDate = DateTime.Now;
                    }
                    await _connectionRepository.Update(connection);
                    return Ok();

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

        // DELETE api/<Connection>/5
        [HttpDelete]
        [Route("Delete")]
        public async Task<object> Delete(string id)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Connection removedCon = _connectionRepository.Remove(id);
            if (removedCon != null)
            {

                if (_worker.RemoveEndpoint(removedCon))
                {
                    await _hubContext.Clients.Group("Connections").SendAsync("DeleteConnection", id);
                }

                return Ok(_connectionRepository.Get(id));
            }
            else
            {
                return new JObject { ["Message"] = $"Connection Id:{id} was not Found" };
            }
        }
    }
}
