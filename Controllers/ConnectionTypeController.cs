using EIR_9209_2.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;



// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnectionTypesController(IInMemoryConnectionRepository connectiontypeRepository) : ControllerBase
    {
        private readonly IInMemoryConnectionRepository _connectiontypeRepository = connectiontypeRepository;

        // POST api/<Connection>
        [HttpPost]
        [Route("Add")]
        /// <summary>
        /// Adds a new connection.
        /// </summary>
        /// <param name="value">The connection details.</param>
        /// <returns>The added connection.</returns>
        public async Task<object> PostAddNewConnectionType([FromBody] JObject value)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //convert the JObject to a Connection object
            ConnectionType contype = value.ToObject<ConnectionType>();
            //add the connection id
            contype.Id = Guid.NewGuid().ToString();
            //add to the connection repository
            ConnectionType loadedCon =await _connectiontypeRepository.AddType(contype);
            if (loadedCon != null)
            {
                return Ok(loadedCon);
            }
            else
            {
                return BadRequest(new JObject { ["message"] = "Connection Type was not Added " });
            }
        }

        // PUT api/<Connection>/5
        [HttpPut]
        [Route("Update")]
        public async Task<object> PutConnectionType(string id, [FromBody] JObject value)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //convert the JObject to a Connection object
            //find id to update 
            ConnectionType conToUpdate = await _connectiontypeRepository.GetType(id);
            if (conToUpdate != null)
            {
                ConnectionType connection = value.ToObject<ConnectionType>();
                ConnectionType updatedCon = await _connectiontypeRepository.UpdateType(connection);
                if (updatedCon != null)
                {
                    return Ok(updatedCon);
                }
                else
                {
                    return BadRequest(new JObject { ["message"] = "Connection Type was not Updated " });
                }
            }
            else
            {
                return new JObject { ["Message"] = $"Connection Id:{id} was not Found" };
            }
        }

        // DELETE api/<Connection>/5
        [HttpDelete]
        [Route("Delete")]
        public async Task<object> DeleteConnectionType(string id)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ConnectionType removedCon = await _connectiontypeRepository.RemoveType(id);
            if (removedCon != null)
            {
                return removedCon;
            }
            else
            {
                return new JObject { ["Message"] = $"Connection Type was not Found" };
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        // POST api/<Connection>
        [HttpPut]
        [Route("AddSubType")]
        public async Task<object> PostAddNewConnectionSubType(string id, [FromBody] JObject value)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ConnectionType conToUpdate =await _connectiontypeRepository.GetType(id);
            if (conToUpdate != null)
            {
                Messagetype msgtype = value.ToObject<Messagetype>();
                msgtype.Id = Guid.NewGuid().ToString();
                Messagetype updatedCon = await _connectiontypeRepository.AddSubType(id, msgtype);
                if (updatedCon != null)
                {
                    return Ok(updatedCon);
                }
                else
                {
                    return BadRequest(new JObject { ["message"] = "Connection Subtype was not Added " });
                }
            }
            else
            {
                return new JObject { ["Message"] = $"Connection Type Id:{id} was not Found" };
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        // PUT api/<Connection>/5
        [HttpPut]
        [Route("UpdateSubType")]
        public async Task<object> PutSubType(string id, [FromBody] JObject value)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //convert the JObject to a Connection object
            //find id to update 
            ConnectionType conToUpdate = await _connectiontypeRepository.GetType(id);
            if (conToUpdate != null)
            {

                Messagetype msgtype = value.ToObject<Messagetype>();
                Messagetype updatedCon = await _connectiontypeRepository.UpdateSubType(id, msgtype);
                if (updatedCon != null)
                {
                    return Ok(updatedCon);
                }
                else
                {
                    return BadRequest(new JObject { ["message"] = "Connection Type was not Updated " });
                }
            }
            else
            {
                return new JObject { ["Message"] = $"Connection Id:{id} was not Found" };
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="subId"></param>
        /// <returns></returns>
        // DELETE api/<Connection>/5
        [HttpPost]
        [Route("DeleteSubType")]
        public async Task<object> DeleteConnectionSubType(string id, string subId)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                Messagetype removedCon = await _connectiontypeRepository.RemoveSubType(id, subId);
                return Ok(removedCon);
            }
            catch (Exception)
            {
                return new JObject { ["Message"] = $"Connection Type was not Found" };
            }
        }
    }
}

