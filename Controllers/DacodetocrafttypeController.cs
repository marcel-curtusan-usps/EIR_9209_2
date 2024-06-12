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
    public class Dacodetocrafttypes : ControllerBase
    {
        private readonly IInMemoryDacodeRepository _dacodeRepository;
        private readonly IHubContext<HubServices> _hubContext;
        private readonly Worker _worker;

        public Dacodetocrafttypes(IInMemoryDacodeRepository dacodeRepository, IHubContext<HubServices> hubContext, Worker worker)
        {
            _dacodeRepository = dacodeRepository;
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
            return Ok(_dacodeRepository.GetAll());
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
            return Ok(_dacodeRepository.Get(id));
        }

        // POST api/<Connection>
        
        [HttpPost]
        [Route("/api/AddDacodetocrafttype")]
        /// <summary>
        /// Adds a new connection.
        /// </summary>
        /// <param name="value">The connection details.</param>
        /// <returns>The added connection.</returns>
        public async Task<object> PostAddNewDacodetocrafttype([FromBody] JObject value)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //convert the JObject to a Connection object
            DesignationActivityToCraftType dacode = value.ToObject<DesignationActivityToCraftType>();
            //add to the connection repository
            DesignationActivityToCraftType loadedDacode = _dacodeRepository.Add(dacode);
            if (loadedDacode != null)
            {
                return Ok(loadedDacode);
            }
            else
            {
                return BadRequest(new JObject { ["message"] = "Designation Activity Code was not Added " });
            }
        }
        // PUT api/<Connection>/5
        [HttpPut]
        [Route("/api/UpdateDacodetocrafttype")]
        public async Task<object> Put(string id, [FromBody] JObject value)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //convert the JObject to a Connection object
            //find id to update 
            DesignationActivityToCraftType dacodeToUpdate = _dacodeRepository.Get(id);
            if (dacodeToUpdate != null)
            {
                DesignationActivityToCraftType dacode = value.ToObject<DesignationActivityToCraftType>();
                DesignationActivityToCraftType updatedDacode = _dacodeRepository.Update(dacode);
                if (updatedDacode != null)
                {
                    return Ok(updatedDacode);
                }
                else
                {
                    return new JObject { ["Message"] = $"Designation Activity Code:{id} was not Found" };
                }
            }
            else
            {
                return new JObject { ["Message"] = $"Designation Activity Code:{id} was not Found" };
            }
        }
    

        // DELETE api/<Connection>/5
        [HttpDelete]
        [Route("/api/DeleteDacodetocrafttype")]
        public async Task<object> Delete(string id)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            DesignationActivityToCraftType removedDacode = _dacodeRepository.Remove(id);
            if (removedDacode != null)
            {
                //return Ok(_dacodeRepository.Get(id));
                return Ok(removedDacode);
            }
            else
            {
                return new JObject { ["Message"] = $"Designation Activity Code:{id} was not Found" };
            }
            }
        }
}
