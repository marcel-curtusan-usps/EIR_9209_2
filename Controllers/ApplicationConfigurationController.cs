using EIR_9209_2.DataStore;
using EIR_9209_2.Models;
using EIR_9209_2.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.RegularExpressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationConfigurationController(ILogger<ApplicationConfigurationController> logger, IConfiguration configuration, IEncryptDecrypt encryptDecrypt, IHubContext<HubServices> hubContext, IResetApplication resetApplication, IInMemoryApplicationRepository application, IInMemorySiteInfoRepository siteInfo, IWebHostEnvironment env) : ControllerBase
    {
        private readonly IResetApplication _resetApplication = resetApplication;
        private readonly ILogger<ApplicationConfigurationController> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IInMemoryApplicationRepository _application = application;
        private readonly IInMemorySiteInfoRepository _siteInfo = siteInfo;
        private readonly IEncryptDecrypt _encryptDecrypt = encryptDecrypt;
        private readonly IWebHostEnvironment _env = env;
        private readonly IHubContext<HubServices> _hubContext = hubContext;

        // GET: api/<SiteConfigurationController>
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Configuration")]
        public async Task<ActionResult> GetByAllConfiguration()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
               var siteData = await _siteInfo.GetSiteInfo();
                JArray siteInformation = new JArray { siteData.ToJToken() };
                JArray appSetting = new JArray { await GetAppSetting() };

                // Merge the two geoZoneKiosk into a single JArray
                siteInformation.Merge(appSetting, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Merge,

                });
                return Ok(siteInformation[0]);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        // GET: api/<SiteConfigurationController>
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Setting")]
        public async Task<ActionResult> GetByConfigurationSetting()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                var appSetting = await GetConfigurationSetting();

                return Ok(appSetting);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        private async Task<JObject> GetConfigurationSetting()
        {
            try
            {
                JObject configurationValues = [];
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

        private async Task<JObject> GetAppSetting()
        {
            try
            {
                JObject configurationValues = [];
                if (_env.IsDevelopment())
                {
                    configurationValues.Add("User", "CF Admin");
                    configurationValues.Add("Role", "Admin");
                    configurationValues.Add("Phone", "555-555-1234");
                    configurationValues.Add("EmailAddress", "cf-sels_support@usps.gov");
                }
                else
                {
                    configurationValues.Add("User", this.User.Identity.IsAuthenticated ? await GetUserName(this.User) : "Operator");
                    configurationValues.Add("Role", this.User.Identity.IsAuthenticated ? await GetUserRole(this.User) : "Operator");
                    configurationValues.Add("Phone", this.User.Identity.IsAuthenticated ? await GetUserPhone(this.User) : "");
                    configurationValues.Add("EmailAddress", this.User.Identity.IsAuthenticated ? await GetUserEmail(this.User) : "");
                }
                configurationValues.Add("ApplicationVersion", Helper.GetCurrentVersion());
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
                    if (value.Properties().Any(p => Regex.IsMatch(p.Name, "NassCode", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10))))
                    {
                        var NassCode = applicationSettings.GetSection("NassCode");
                        var nassCodeValue = value.Properties().First(p => Regex.IsMatch(p.Name, "NassCode", RegexOptions.IgnoreCase,TimeSpan.FromSeconds(10))).Value.ToString();
                        if (nassCodeValue == "")
                        {
                            NassCode.Value = nassCodeValue;

                            if (await _resetApplication.Reset())
                            {
                                if (await _application.Update(NassCode.Key, NassCode.Value))
                                {
                                    _logger.LogInformation($"NASS Code have been update {nassCodeValue}");
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
                    else if (value.Properties().Any(p => Regex.IsMatch(p.Name, "IdsConnectionString", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10))))
                    {
                        var currentValue = value.Properties().First(p => Regex.IsMatch(p.Name.ToString(), "IdsConnectionString", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10))).Value.ToString();
                        var ConnectionString = applicationSettings.GetSection("IdsConnectionString");
                        ConnectionString.Value = _encryptDecrypt.Encrypt(currentValue);
                        if (await _application.Update(ConnectionString.Key, ConnectionString.Value))
                        {
                            _logger.LogInformation($"IDS Connection String drive have been update {ConnectionString.Value}");

                        }
                    }
                    else if (value.Properties().Any(p => Regex.IsMatch(p.Name, "BaseDrive", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10))))
                    {
                        var currentBaseDriveValue = value.Properties().First(p => Regex.IsMatch(p.Name, "BaseDrive", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10))).Value.ToString();
                        var BaseDrive = applicationSettings.GetSection("BaseDrive");
                        BaseDrive.Value = currentBaseDriveValue;
                        if (await _application.Update(BaseDrive.Key, BaseDrive.Value))
                        {
                            _logger.LogInformation($"Base drive have been update {BaseDrive.Value}");

                        }
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

        private async Task<string> GetUserEmail(ClaimsPrincipal user)
        {
            //get the user email from claims
            var email = user?.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
            return await Task.FromResult(email ?? string.Empty);
        }

        private async Task<string> GetUserPhone(ClaimsPrincipal user)
        {
            //get the user phone from claims
            var phone = user?.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/phone")?.Value;
            return await Task.FromResult(phone ?? string.Empty);
        }

        private Task<string> GetUserName(ClaimsPrincipal? user)
        {
            return Task.FromResult(user?.Identity?.Name ?? string.Empty);
        }
        private async Task<string> GetUserRole(ClaimsPrincipal? user)
        {
            try
            {
                var userGroups = await GetUserGroups(user).ConfigureAwait(false);
                return GetRoleFromGroups(userGroups);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return "Operator";
            }
        }
        private string GetRoleFromGroups(IEnumerable<string> userGroups)
        {
            var roles = new Dictionary<string, string>
        {
            { "AdminRole", "Admin" },
            { "PlantManager", "PM" },
            { "MaintenanceRole", "Maintenance" },
            { "OIE", "OIE" }
        };

            foreach (var role in roles)
            {
                var configRoles = _configuration[$"UserRole:{role.Key}"]?.Split(',').Select(r => r.Trim()).ToList() ?? new List<string>();
                if (configRoles.Intersect(userGroups, StringComparer.OrdinalIgnoreCase).Any())
                {
                    return role.Value;
                }
            }

            return "Operator";
        }
        private Task<IEnumerable<string>> GetUserGroups(ClaimsPrincipal? user)
        {
            if (user == null)
            {
                return Task.FromResult(Enumerable.Empty<string>());
            }

            if (user is not WindowsPrincipal windowsPrincipal)
            {
                return Task.FromResult(Enumerable.Empty<string>());
            }

            if (windowsPrincipal.Identity is not WindowsIdentity windowsIdentity)
            {
                return Task.FromResult(Enumerable.Empty<string>());
            }

            var groups = windowsIdentity.Groups
                                        .Select(g => g.Translate(typeof(NTAccount)).ToString().TrimStart(@"USA\".ToCharArray()))
                                        .ToList();

            return Task.FromResult<IEnumerable<string>>(groups);
        }
    }
}
