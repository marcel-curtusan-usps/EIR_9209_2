using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.SignalR;
using EIR_9209_2.Service;
using NuGet.Protocol.Plugins;



// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Connections : ControllerBase
    {
        private readonly IInMemoryConnectionRepository _connectionRepository;
        private readonly IHubContext<HubServices> _hubContext;
        private readonly IWorker _worker;

        public Connections(IInMemoryConnectionRepository connectionRepository, IHubContext<HubServices> hubContext, IWorker worker)
        {
            _connectionRepository = connectionRepository;
            _hubContext = hubContext;
            _worker = worker;
        }

        // GET: api/<Connection>
        [HttpGet]
        public async Task<object> Get()
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(_connectionRepository.GetAll());
        }

        // GET api/<Connection>/5
        [HttpGet("{id}")]
        public async Task<object> Get(string id)
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
        [Route("/api/AddConnection")]
        /// <summary>
        /// Adds a new connection.
        /// </summary>
        /// <param name="value">The connection details.</param>
        /// <returns>The added connection.</returns>
        public async Task<IActionResult> PostAddNewConnection([FromBody] JObject value)
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

        // PUT api/<Connection>/5
        [HttpPut]
        [Route("/api/UpdateConnection")]
        public async Task<object> Put(string id, [FromBody] JObject value)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //convert the JObject to a Connection object
            //find id to update 
            Connection conToUpdate = _connectionRepository.Get(id);
            if (conToUpdate != null)
            {
                Connection connection = value.ToObject<Connection>();
                connection.LastupDate = DateTime.Now;
                if (!connection.ActiveConnection)
                {
                    connection.DeactivatedDate = DateTime.Now;
                }
                Connection updatedCon = _connectionRepository.Update(connection);
                if (updatedCon != null)
                {
                    //add the connection to the worker
                    if (_worker.UpdateEndpoint(updatedCon))
                    {
                        await _hubContext.Clients.Group("Connections").SendAsync("UpdateConnection", updatedCon);
                        return Ok(updatedCon);
                    }
                    else
                    {
                        return BadRequest(new JObject { ["message"] = "End Point was not Started" });
                    }
                }
                else
                {
                    return BadRequest(new JObject { ["message"] = "End Point was not Updated " });
                }
            }
            else
            {
                return new JObject { ["Message"] = $"Connection Id:{id} was not Found" };
            }
        }

        // DELETE api/<Connection>/5
        [HttpDelete]
        [Route("/api/DeleteConnection")]
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
