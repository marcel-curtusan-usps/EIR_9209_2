using EIR_9209_2.Models;
using EIR_9209_2.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;



// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Dacodetocrafttypes : ControllerBase
    {
        private readonly IInMemoryDacodeRepository _dacodeRepository;
        private readonly IInMemoryTagsRepository _tags;


        public Dacodetocrafttypes(IInMemoryDacodeRepository dacodeRepository, IInMemoryTagsRepository tags, IHubContext<HubServices> hubContext, Worker worker)
        {
            _dacodeRepository = dacodeRepository;
            _tags = tags;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // GET: api/<DAcode>
        [HttpGet]
        [Route("GetDacodeToCraftTypeList")]
        public object Get()
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(_dacodeRepository.GetAll());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET api/<DAcode>/5
        [HttpGet]
        [Route("GetById")]
        public object Get(string id)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(_dacodeRepository.Get(id));
        }

        // POST api/<DAcode>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]

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
            DesignationActivityToCraftType loadedDacode = await _dacodeRepository.Add(dacode);
            if (loadedDacode != null)
            {
                //update all tag that have this da code
                _ = Task.Run(() => _tags.UpdateTagDesignationActivity(loadedDacode)).ConfigureAwait(false);
                return Ok(loadedDacode);
            }
            else
            {
                return BadRequest(new JObject { ["message"] = "Designation Activity Code was not Added " });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        // PUT api/<DAcode>/5
        [HttpPut]
        [Route("Update")]
        public async Task<object> Put(string id, [FromBody] JObject value)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //convert the JObject to a Connection object
            //find id to update 
            DesignationActivityToCraftType dacodeToUpdate = await _dacodeRepository.Get(id);
            if (dacodeToUpdate != null)
            {
                DesignationActivityToCraftType dacode = value.ToObject<DesignationActivityToCraftType>();
                DesignationActivityToCraftType updatedDacode = await _dacodeRepository.Update(dacode);
                if (updatedDacode != null)
                {
                    //update all tag that have this da code
                    _ = Task.Run(() => _tags.UpdateTagDesignationActivity(updatedDacode)).ConfigureAwait(false);
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


        // DELETE api/<DAcode>/5
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Delete")]
        public async Task<object> Delete(string id)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            DesignationActivityToCraftType removedDacode = await _dacodeRepository.Remove(id);
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
