using EIR_9209_2.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Buffers;
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

        // GET api/<TagController>/5
        [HttpGet("{id}")]
        public async Task<object> Get(string id)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return Ok(_tags.Get(id));
        }
        // GET api/<TagController>/5
        [HttpGet]
        [Route("Search")]
        public async Task<object> GetBySearch(string id)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            string searchValue = string.IsNullOrEmpty(id) ? "" : HttpUtility.UrlDecode(id).Replace("\"", "");
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
        // PUT api/<TagController>/5
        [HttpPut("{id}")]
        public async Task<object> Put(string id, [FromBody] string value)
        {
            if (!ModelState.IsValid)
            {
                BadRequest(ModelState);
            }
            return Ok(_tags.Get(id));
        }
        // PUT api/<TagController>/5
        [HttpPut()]
        [Route("UpdateTagInfo")]
        public async Task<object> PutByTagInfo(string id, [FromBody] JObject value)
        {
            if (!ModelState.IsValid)
            {
                BadRequest(ModelState);
            }
            //update all tag that have this da code
            var result = Task.Run(() => _tags.UpdateTagInfo(value)).Result;

            return Ok(result);
        }

        // DELETE api/<TagController>/5
        [HttpDelete("{id}")]
        public async Task<object> Delete(string id)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                BadRequest(ModelState);
            }
            _tags.Remove(id);
            await _hubContext.Clients.All.SendAsync("DeleteTag", id);
            return Ok(_tags.Get(id));
        }
    }
}
