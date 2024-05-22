using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Connections(IInMemoryConnectionRepository connectionRepository, IHubContext<HubServices> hubContext) : ControllerBase
    {
        private readonly IInMemoryConnectionRepository _connectionRepository = connectionRepository;
        private readonly IHubContext<HubServices> _hubContext = hubContext;

        // GET: api/<Connection>
        [HttpGet]
        public async Task<object> Get()
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return _connectionRepository.GetAll();
        }
        // GET api/<Connection>/5
        [HttpGet("{id}")]
        public async Task<object> Get(string id)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return Task.FromResult(BadRequest(ModelState));
            }
            return _connectionRepository.Get(id);
        }

        // POST api/<Connection>
        [HttpPost]
        [Route("/AddConnection")]
        /// <summary>
        /// Adds a new connection.
        /// </summary>
        /// <param name="value">The connection details.</param>
        /// <returns>The added connection.</returns>
        public async Task<object> PostAddNewConnection([FromBody] JObject value)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return Task.FromResult(BadRequest(ModelState));
            }
            //convert the JObject to a Connection object
            Connection? connection = value.ToObject<Connection>();
            //add the connection id
            connection.Id = Guid.NewGuid().ToString();
            connection.CreatedDate = DateTime.Now;

            //add to the connection repository
            _connectionRepository.Add(connection);
            //return the connection id
            connection = _connectionRepository.Get(connection.Id);
            await _hubContext.Clients.All.SendAsync("AddConnection", connection);
            return connection;
        }

        // PUT api/<Connection>/5
        [HttpPut("{id}")]
        public async Task<object> Put(string id, [FromBody] JObject value)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return Task.FromResult(BadRequest(ModelState));
            }
            await _hubContext.Clients.All.SendAsync("UpdateConnection", id);
            return _connectionRepository.Get(id);
        }

        // DELETE api/<Connection>/5
        [HttpDelete]
        [Route("/DeleteConnection")]
        public async Task<object> Delete(string id)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return Task.FromResult(BadRequest(ModelState));
            }
            _connectionRepository.Remove(id);
            _hubContext.Clients.All.SendAsync("DeleteConnection", id);
            return _connectionRepository.Get(id);

        }
    }
}
