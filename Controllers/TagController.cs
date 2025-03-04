using EIR_9209_2.Models;
using EIR_9209_2.DataStore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System.Web;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController(IInMemoryTagsRepository tags, IInMemoryEmployeesRepository employees, IHubContext<HubServices> hubContext) : ControllerBase
    {
        private readonly IInMemoryTagsRepository _tags = tags;
        private readonly IInMemoryEmployeesRepository _emp = employees;
        private readonly IHubContext<HubServices> _hubContext = hubContext;

        // GET: api/<TagController>
        [HttpGet]
        public object Get()
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
                return BadRequest(ModelState);
            }
            return Ok( await _tags.Get(tagId));
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
                return BadRequest(ModelState);
            }
            string searchValue = string.IsNullOrEmpty(value) ? "" : HttpUtility.UrlDecode(value).Replace("\"", "");
            var query = await _tags.SearchTag(searchValue);
            var SearchReuslt = await _emp.SearchEmployee(searchValue);
           
            var finalReuslt = query.Concat(SearchReuslt).Distinct();
            return Ok(finalReuslt);
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

            var taginfo = await _tags.UpdateTagUIInfo(value);

            return Ok(taginfo);
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
        [HttpPost]
        [Route("UploadTagAssociation")]
        public async Task<IActionResult> UploadCSV(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }
                JArray tagAssociationArray = [];
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    //loop through the CSV file and process the data
                    //this is where you would save the data to the database
                    //or send it to the front end
                    //or do whatever you need to do with the data
                    // Read the CSV file and process the data
                    var fileContent = await reader.ReadToEndAsync();

                    // Split the file content into lines
                    var lines = fileContent.Split('\n');
                    if (lines.Length < 2)
                    {
                        return BadRequest("CSV file is empty or does not contain enough data.");
                    }

                    // Read the header line
                    var headerLine = lines[0].Replace("\"", "").Replace("\\", "").Trim();
                    var headers = headerLine.Split(',');

                    // Check if the header contains the required field "type"
                    if (!headers.Contains("type", StringComparer.OrdinalIgnoreCase))
                    {
                        return BadRequest("CSV file does not contain the required 'type' field in the header.");
                    }
                    // Check if the header contains the required field "type"
                    if (!headers.Contains("tagId", StringComparer.OrdinalIgnoreCase))
                    {
                        return BadRequest("CSV file does not contain the required 'tagId' field in the header.");
                    }
                    // Loop through the lines and process the data
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var line = lines[i];

                        // Skip empty lines
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        // Remove quotes and backslashes from the line
                        var cleanLine = line.Replace("\"", "").Replace("\\", "").Trim();

                        // Split the line into values
                        var fields = cleanLine.Split(',');

                        // Validate the data from each field
                        if (fields.Length != headers.Length) // Check the number of fields
                        {
                            return BadRequest("Invalid data format");
                        }

                        // Create a JSON object using the headers as keys
                        var jsonObject = new JObject();
                        for (int j = 0; j < headers.Length; j++)
                        {
                            jsonObject[headers[j]] = fields[j];
                        }

                        tagAssociationArray.Add(jsonObject);
                    }
                }
                return Ok(new JObject { ["message"] = "Tag Association data was uploaded successfully." });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
