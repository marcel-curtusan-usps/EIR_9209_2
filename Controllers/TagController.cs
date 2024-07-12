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
    public class TagController(IInMemoryTagsRepository tags, IHubContext<HubServices> hubContext) : ControllerBase
    {
        private readonly IInMemoryTagsRepository _tags = tags;
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
            return Ok(_tags.GetAll());
        }
        /// <summary>
        /// Get Tag by TagId
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        // GET api/<TagController>/5
        [HttpGet]
        [Route("GetTagByTagId")]
        public async Task<object> Get(string tagId)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return Ok(_tags.Get(tagId));
        }

        /// <summary>
        /// Get list of Tag by TagType
        /// </summary>
        /// <param name="tagType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTagTypeList")]
        public async Task<object> GetByTagType(string tagType)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _tags.GetTagByType(tagType);
        }
        /// <summary>
        /// Search for Tag by search value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // GET api/<TagController>/5
        [HttpGet]
        [Route("Search")]
        public async Task<object> GetBySearch(string value)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            string searchValue = string.IsNullOrEmpty(value) ? "" : HttpUtility.UrlDecode(value).Replace("\"", "");
            var query = await Task.Run(() => _tags.SearchTag(searchValue));
            var searchReuslt = (from sr in query
                                select new JObject
                                {
                                    ["id"] = sr.Id,
                                    ["eIN"] = sr.EIN,
                                    ["tagType"] = sr.TagType,
                                    ["name"] = sr.Name,
                                    ["encodedId"] = sr.EncodedId,
                                    ["empFirstName"] = sr.EmpFirstName,
                                    ["empLastName"] = sr.EmpLastName,
                                    ["craftName"] = sr.CraftName,
                                    ["payLocation"] = sr.PayLocation,
                                    ["designationActivity"] = sr.DesignationActivity,
                                    ["color"] = sr.Color
                                }).ToList();
            return Ok(searchReuslt);
        }
        //add new tag
        // POST api/<TagController>
        [HttpPost]
        [Route("Add")]
        public async Task<object> Post([FromBody] GeoMarker tag)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _tags.Add(tag);

            return Ok();
        }

        //// PUT api/<TagController>/5
        //[HttpPut("{id}")]
        //public async Task<object> Put(string id, [FromBody] string value)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        BadRequest(ModelState);
        //    }
        //    return Ok(_tags.Get(id));
        //}

        /// <summary>
        /// Update Tag Info
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // PUT api/<TagController>/5
        [HttpPost]
        [Route("UpdateTagInfo")]
        public async Task<object> PutByTagInfo([FromBody] JObject value)
        {
            if (!ModelState.IsValid)
            {
                BadRequest(ModelState);
            }

            await _tags.UpdateTagUIInfo(value);

            return Ok();
        }

        // DELETE api/<TagController>/5
        [HttpDelete]
        [Route("DeleteTag")]
        public async Task<object> Delete(string id)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                BadRequest(ModelState);
            }
            await _tags.Delete(id);

            return Ok();
        }
    }
}
