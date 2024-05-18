using EIR_9209_2.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController(IInMemoryTagsRepository tagsRepository, IHubContext<HubServices> hubContext) : ControllerBase
    {
        private readonly IInMemoryTagsRepository _tagsRepository = tagsRepository;
        private readonly IHubContext<HubServices> _hubContext = hubContext;

        // GET: api/<TagController>
        [HttpGet]
        public async Task<object> GetAsync()
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _tagsRepository.GetAll();
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
            return _tagsRepository.Get(id);
        }
        // PUT api/<TagController>/5
        [HttpPut("{id}")]
        public async Task<object> Put(string id, [FromBody] string value)
        {
            if (!ModelState.IsValid)
            {
                await Task.FromResult(BadRequest(ModelState));
            }
            return _tagsRepository.Get(id);
        }

        // DELETE api/<TagController>/5
        [HttpDelete("{id}")]
        public async Task<object> Delete(string id)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                await Task.FromResult(BadRequest(ModelState));
            }
            _tagsRepository.Remove(id);
            _hubContext.Clients.All.SendAsync("DeleteTag", id);
            return _tagsRepository.Get(id);
        }
    }
}
