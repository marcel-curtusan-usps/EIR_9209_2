using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.SignalR;
using EIR_9209_2.Service;



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
        [Route("/api/AddNewConnection")]
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
            _connectionRepository.Add(connection);
            //add the connection to the worker
            if (_worker.AddEndpoint(connection))
            {
                //return the connection id
                connection = _connectionRepository.Get(connection.Id);
                await _hubContext.Clients.All.SendAsync("AddConnection", connection);
                return Ok(connection);
            }
            else
            {
                return BadRequest(new JObject { ["message"] = "End Point was not Started" });
            }



        }

        // PUT api/<Connection>/5
        [HttpPut("{id}")]
        public async Task<object> Put(string id, [FromBody] JObject value)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _hubContext.Clients.All.SendAsync("UpdateConnection", id);
            return Ok(_connectionRepository.Get(id));
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
            _connectionRepository.Remove(id);
            Connection connection = _connectionRepository.Get(id);
            if (_worker.RemoveEndpoint(connection))
            {
                await _hubContext.Clients.All.SendAsync("DeleteConnection", id);
            }

            return Ok(_connectionRepository.Get(id));
        }
    }
}
