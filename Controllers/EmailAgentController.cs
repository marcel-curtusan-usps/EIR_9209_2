using Microsoft.AspNetCore.Mvc;
using EIR_9209_2.Service;
using Newtonsoft.Json.Linq;
using EIR_9209_2.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class EmailAgentController(ILogger<EmailAgentController> logger, ScreenshotService screenshotService, EmailService emailService, IConfiguration configuration, IInMemoryEmailRepository emailList) : ControllerBase
    {
        private readonly ScreenshotService _screenshotService = screenshotService;
        private readonly EmailService _emailService = emailService;
        private readonly ILogger<EmailAgentController> _logger = logger;
        private readonly IInMemoryEmailRepository _emailList = emailList;
        private readonly IConfiguration _configuration = configuration;

        // GET: api/<ZoneController>
        [HttpGet]
        [Route("AllEmail")]
        public async Task<object> GetAllEmail()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(_emailList.GetAll());
        }

        // POST api/<EmailAgentController>
        [HttpPost]
        [Route("SendEmail")]
        public async Task<object> PostByEmail(string url, string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var screenshotStream = await _screenshotService.CaptureScreenshotAsync(url);

            var body = $"Here is the screen shot you requested. <a href={url}>Click here to visit our website</a>";
            await _emailService.SendEmailAsync(_configuration["ApplicationConfiguration:SupportEmail"], email, "MPE Screen shot", body, screenshotStream);
            screenshotStream = null;
            return Ok("Email sent successfully!");

        }
        // POST api/<EmailAgentController>
        [HttpPost]
        [Route("AddEmail")]
        public async Task<object> PostByAddNewEmail([FromBody] JObject newEmail)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                // add newEmail to the _emailList
                Email email = newEmail.ToObject<Email>();
                email.Id = Guid.NewGuid().ToString();
                Email result = _emailList.Add(email);
                if (result == null)
                {
                    return BadRequest("Email was not added");
                }
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        // PUT api/<EmailAgentController>/5 
        [HttpPut("EditEmail")]
        public async Task<object> Put(string id, [FromBody] JObject value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Email email = value.ToObject<Email>();
            Email result = _emailList.Update(email);
            if (result == null)
            {
                return BadRequest("Email was not updated");
            }
            return Ok(result);
        }
        //delete api/<EmailAgentController>/5   
        [HttpDelete("DeleteEmail")]
        public async Task<object> Delete(string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = _emailList.Delete(id);
            if (result == null)
            {
                return BadRequest("Email was not deleted");
            }
            return Ok("Email deleted successfully!");
        }
    }
}
