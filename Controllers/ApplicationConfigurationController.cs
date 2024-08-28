using EIR_9209_2.Utilities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationConfigurationController(ILogger<ApplicationConfigurationController> logger, IConfiguration configuration, IEncryptDecrypt encryptDecrypt) : ControllerBase
    {
        private readonly ILogger<ApplicationConfigurationController> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IEncryptDecrypt _encryptDecrypt = encryptDecrypt;

        // GET: api/<SiteConfigurationController>
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("AllConfiguration")]
        public object GetByAllConfiguration()
        {

            // hold all settings as a dictionary
            Dictionary<string, string?> configurationValues = [];

            // Example: Retrieve a specific configuration section
            var applicationSettings = _configuration.GetSection("ApplicationConfiguration");
            if (applicationSettings.Exists())
            {
                foreach (var setting in applicationSettings.GetChildren())
                {
                    if (setting.Key.EndsWith("ConnectionString"))
                    {
                        configurationValues.Add(setting.Key, _encryptDecrypt.Decrypt(setting.Value));
                    }
                    else
                    {
                        configurationValues.Add(setting.Key, setting.Value);
                    }

                }
            }

            // If you want to retrieve all configurations, you might need to iterate through all sections
            // Note: Returning all settings might include sensitive information like connection strings

            return Ok(configurationValues);
        }
        /// <summary>
        /// 
        /// </summary>

        /// <param name="id"></param>
        /// <returns></returns>
        // GET api/<SiteConfigurationController>/5
        [HttpGet]
        [Route("Configuration")]
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // PUT api/<SiteConfigurationController>/5
        [HttpPost]
        [Route("Update")]
        public object PostByUpdateApplicationConfiguration([FromBody] JObject value)
        {
            // Example: Update a specific configuration setting
            var applicationSettings = _configuration.GetSection("ApplicationConfiguration");
            if (applicationSettings.Exists())
            {
                //var setting = applicationSettings.GetSection(key);
                //if (setting.Exists())
                //{
                //    if (key.EndsWith("ConnectionString"))
                //    {
                //        _encryptDecrypt.Encrypt(value);
                //    }
                //    else
                //    {
                //        setting.Value = value;
                //    }

                //    return Ok();
                //}
            }

            return BadRequest();

        }

    }
}
