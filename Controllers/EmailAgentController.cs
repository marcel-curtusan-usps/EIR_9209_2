using Microsoft.AspNetCore.Mvc;
using EIR_9209_2.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class EmailAgentController : ControllerBase
    {
        private readonly ScreenshotService _screenshotService;
        private readonly EmailService _emailService;
        private readonly ILogger<EmailAgentController> _logger;
        private readonly IConfiguration _configuration;

        public EmailAgentController(ILogger<EmailAgentController> logger, ScreenshotService screenshotService, EmailService emailService, IConfiguration configuration)
        {
            _screenshotService = screenshotService;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        // POST api/<EmailAgentController>
        [HttpPost]
        [Route("/SendEmail")]
        public async Task<object> PostByEmail(string url, string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var screenshotStream = await _screenshotService.CaptureScreenshotAsync(url);

            var body = $"Here is the screenshot you requested. <a href={url}>Click here to visit our website</a>";
            await _emailService.SendEmailAsync(_configuration["ApplicationConfiguration:SupportEmail"], email, "MPE Screenshot", body, screenshotStream);
            screenshotStream = null;
            return Ok("Email sent successfully!");

        }
    }
}
