using EIR_9209_2.DataStore;
using EIR_9209_2.Models;
using EIR_9209_2.Utilities;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using PuppeteerSharp.Input;
using System.Diagnostics;
using System;
using System.Text.RegularExpressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationConfigurationController(ILogger<ApplicationConfigurationController> logger, IConfiguration configuration, IEncryptDecrypt encryptDecrypt, IHubContext<HubServices> hubContext, IResetApplication resetApplication, IInMemoryApplicationRepository application) : ControllerBase
    {
        private readonly IResetApplication _resetApplication = resetApplication;
        private readonly ILogger<ApplicationConfigurationController> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IInMemoryApplicationRepository _application = application;
        private readonly IEncryptDecrypt _encryptDecrypt = encryptDecrypt;
        private readonly IHubContext<HubServices> _hubContext = hubContext;

        // GET: api/<SiteConfigurationController>
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("AllConfiguration")]
        public object GetByAllConfiguration()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                return Ok(GetAppSetting());
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
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
        public async Task<object> PostByUpdateApplicationConfiguration([FromBody] JObject value)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                // Example: Update a specific configuration setting
                var applicationSettings = _configuration.GetSection("ApplicationConfiguration");

                if (applicationSettings.Exists())
                {
                    var appName = applicationSettings.GetSection("ApplicationName");
                    if (value.Properties().Any(p => Regex.IsMatch(p.Name, "NassCode", RegexOptions.IgnoreCase)))
                    {
                        var NassCode = applicationSettings.GetSection("NassCode");
                        var nassCodeValue = value.Properties().First(p => Regex.IsMatch(p.Name, "NassCode", RegexOptions.IgnoreCase)).Value.ToString();
                        if (nassCodeValue == "")
                        {
                            NassCode.Value = nassCodeValue;

                            if (await _resetApplication.Reset())
                            {
                                if (await _application.Update(NassCode.Key, NassCode.Value))
                                { 
                                
                                }
                            }
                        }
                        else
                        {
                            
                            var currentnassCodeValue = NassCode.Value;
                            // <summary>
                            //1.The code checks if the value of the setting is not equal to the nassCode provided in the value parameter. This condition is used to determine if the nassCode needs to be updated.
                            //2.If the condition is true, the code enters the if block and executes the following steps:
                            //a.It calls the _resetApplication.GetNewSiteInfo method with the nassCode value as a parameter.This method is responsible for retrieving new site information based on the provided nassCode.
                            //b.If the GetNewSiteInfo method returns true, indicating that new site information is successfully retrieved, the code proceeds to update the setting.Value with the nassCode value.
                            //c.After updating the setting.Value, the code calls the _resetApplication.Reset method.This method is responsible for resetting the application based on the updated configuration.
                            //d.If the Reset method returns true, indicating that the application reset is successful, the code proceeds to update the application using the _application.Update method. This method updates the specified setting.Key with the new setting.Value.
                            //e.Finally, the code calls the _resetApplication.Setup method, which performs the setup process for the application.
                            //3.If the GetNewSiteInfo method returns false, indicating that new site information retrieval failed, the code enters the else block and returns a BadRequest response.
                            // </summary>
                            if (NassCode.Value != nassCodeValue)
                            {
                                NassCode.Value = nassCodeValue;
                                if (await _resetApplication.GetNewSiteInfo(nassCodeValue))
                                {
                                    if (await _resetApplication.Reset())
                                    {
                                        if (await _application.Update(NassCode.Key, NassCode.Value))
                                        {
                                            bool SetupResult = await _resetApplication.Setup();
                                        }                                       
                                    }
                                }
                                else
                                {
                                    NassCode.Value = currentnassCodeValue;
                                    return BadRequest();
                                }
                            }
                            else
                            {
                                return BadRequest();
                            }
                        }
                    }
                    else if (value.ContainsKey("ConnectionString"))
                    {
                      var  ConnectionString = applicationSettings.GetSection(value["ConnectionString"].ToString());
                        ConnectionString.Value = _encryptDecrypt.Encrypt(value["ConnectionString"].ToString());
                    }
                    await _hubContext.Clients.Group("ApplicationConfiguration").SendAsync($"updateApplicationConfiguration", GetAppSetting(), cancellationToken: CancellationToken.None);
                    return Ok();

                }
                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        private object? GetAppSetting()
        {
            try
            {
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
                return configurationValues;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }
    }
}
