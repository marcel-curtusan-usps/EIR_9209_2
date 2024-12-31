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
        [HttpGet]
        [Route("UserRole")]
        public async Task<ActionResult> GetByConfigurationRoleGroups()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                var appUserRole = await GetConfigurationRoleGroups();

                return Ok(appUserRole);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        private async Task<Dictionary<string, string?>> GetConfigurationRoleGroups()
        {
            try
            {
                Dictionary<string, string?> userRoleValues = [];
                var applicationUserRole = _configuration.GetSection("UserRole");
                if (applicationUserRole.Exists())
                {
                    foreach (var setting in applicationUserRole.GetChildren())
                    {
                        userRoleValues.Add(setting.Key, setting.Value);
                    }
                }
                return await Task.FromResult(userRoleValues);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return [];
            }
        }

        private async Task<Dictionary<string, string?>> GetConfigurationSetting()
        {
            try
            {
                Dictionary<string, string?> configurationValues = [];
                var applicationSettings = _configuration.GetSection("ApplicationConfiguration");
                if (applicationSettings.Exists())
                {
                    foreach (var setting in applicationSettings.GetChildren())
                    {
                        if (setting.Key.EndsWith("ConnectionString"))
                        {
                            configurationValues.Add(setting.Key, await Task.Run(() => _encryptDecrypt.Decrypt(setting.Value ?? string.Empty)));
                        }
                        else
                        {
                            configurationValues.Add(setting.Key, setting.Value);
                        }

                    }
                }
                return await Task.FromResult(configurationValues);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return [];
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
        public async Task<object> PostByUpdateApplicationConfiguration([FromBody] JObject appSettingsData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                // Example: Update a specific configuration setting
                var applicationSettings = _configuration.GetSection("ApplicationConfiguration");
                if (!applicationSettings.Exists())
                {
                    return BadRequest("Application configuration not found.");
                }

                foreach (var setting in applicationSettings.GetChildren())
                {
                    var matchingProperty = appSettingsData.Properties().FirstOrDefault(p => p.Name.Equals(setting.Key, StringComparison.OrdinalIgnoreCase));
                    if (matchingProperty != null)
                    {
                        // Handle the matching key
                        var newValue = matchingProperty.Value.ToString().Trim();
                        if (matchingProperty.Name.EndsWith("NassCode", StringComparison.OrdinalIgnoreCase))
                        {
                            var currentNassCodeValue = setting.Value;
                            if (!string.IsNullOrEmpty(newValue) && currentNassCodeValue != newValue && (!newValue.StartsWith('-')))
                            {
                                setting.Value = newValue;

                                if (await _resetApplication.Reset())
                                {
                                    if (await _application.Update(setting.Key, setting.Value, "ApplicationConfiguration"))
                                    {
                                        _logger.LogInformation($"{setting.Key} have been update {setting.Value}");
                                        bool SetupResult = await _resetApplication.Setup();
                                    }
                                }
                            }
                            else
                            {
                                setting.Value = "";

                                if (await _resetApplication.Reset())
                                {
                                    if (await _application.Update(setting.Key, setting.Value, "ApplicationConfiguration"))
                                    {
                                        _logger.LogInformation($"{setting.Key} have been update {setting.Value}");
                                    }
                                }
                            }
                        }
                        else if (matchingProperty.Name.EndsWith("ConnectionString", StringComparison.OrdinalIgnoreCase))
                        {
                            setting.Value = _encryptDecrypt.Encrypt(newValue);
                            if (await _application.Update(setting.Key, setting.Value, "ApplicationConfiguration"))
                            {
                                _logger.LogInformation($"{setting.Key} drive have been update {setting.Value}");

                            }
                        }
                        else
                        {
                            setting.Value = newValue;
                            if (await _application.Update(setting.Key, setting.Value, "ApplicationConfiguration"))
                            {
                                _logger.LogInformation($"{setting.Key} have been update {setting.Value}");
                            }
                        }
                        await _hubContext.Clients.Group("ApplicationConfiguration").SendAsync($"updateApplicationConfiguration", await GetConfigurationSetting(), cancellationToken: CancellationToken.None);
                        return Ok();
                    }
                }
                return BadRequest();
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
        /// <param name="value"></param>
        /// <returns></returns>
        // PUT api/<SiteConfigurationController>/5
        [HttpPut]
        [Route("UpdateUserRole")]
        public async Task<object> PostByUpdateUserRole([FromBody] JObject appUserRole)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                // Example: Update a specific configuration setting
                var applicationUserRole = _configuration.GetSection("UserRole");

                if (applicationUserRole.Exists())
                {
                    foreach (var userRole in applicationUserRole.GetChildren())
                    {
                        var matchingProperty = appUserRole.Properties().FirstOrDefault(p => p.Name.Equals(userRole.Key, StringComparison.OrdinalIgnoreCase));
                        if (matchingProperty != null)
                        {
                            // Handle the matching key
                            var newValue = matchingProperty.Value.ToString().Trim();
                            if (userRole.Value != newValue)
                            {
                                userRole.Value = newValue;
                            }
                            if (await _application.Update(userRole.Key, userRole.Value, "UserRole"))
                            {
                                await _hubContext.Clients.Group("ApplicationRoleGroups").SendAsync($"updateApplicationRoleGroups", await GetConfigurationRoleGroups(), cancellationToken: CancellationToken.None);
                            }
                        }
                    }
                    return Ok(await GetConfigurationRoleGroups());

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
