using EIR_9209_2.DatabaseCalls.IDS;
using EIR_9209_2.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IDSController : ControllerBase
    {
        private readonly ILogger<IDSController> _logger;
        private readonly IIDS _ids;
        private readonly IInMemoryGeoZonesRepository _geoZones;
        public IDSController(ILogger<IDSController> logger, IIDS ids, IInMemoryGeoZonesRepository geoZones)
        {
            _logger = logger;
            _ids = ids;
            _geoZones = geoZones;
        }
        // POST api/<IDSController>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("/GetIDSData")]
        public async Task<object> PostGetIDSData([FromBody] JToken data)
        {   //handle bad requests
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            else if (data != null && data.HasValues)
            {
                if (data.HasValues && data.Type == JTokenType.Object)
                {
                    JToken result = await _ids.GetOracleIDSData(data);

                    if (((JObject)result).ContainsKey("Error"))
                    {
                        return await Task.FromResult(result);

                    }
                    else
                    {
                        return await Task.FromResult(result);
                    }
                }

                else
                {
                    return await Task.FromResult(BadRequest());
                }
            }
            else
            {
                return await Task.FromResult(BadRequest());
            }
        }
        [HttpPost]
        [Route("/IDSData")]
        public async Task<object> PostIDSData(string queryName, int startHour, int endHour)
        {   //handle bad requests
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }

            if (!string.IsNullOrEmpty(queryName) && (startHour > 0) && (endHour > 0 && endHour < 24))
            {
                JObject data = new JObject
                {
                    ["startHour"] = startHour,
                    ["endHour"] = endHour,
                    ["queryName"] = queryName
                };
                if (data.HasValues && data.Type == JTokenType.Object)
                {
                    JToken result = await _ids.GetOracleIDSData(data);
                    if (result.HasValues)
                    {
                        _ = Task.Run(() => ProcessIDSData(result));
                    }
                    if (result.Type == JTokenType.Array)
                    {
                        return await Task.FromResult(Ok(result));
                    }
                    else
                    {
                        if (((JObject)result).ContainsKey("Error"))
                        {
                            return await Task.FromResult(BadRequest(result));

                        }
                        else
                        {
                            return await Task.FromResult(Ok(result));
                        }
                    }

                }
                else
                {
                    return await Task.FromResult(BadRequest(new { message = "Invalid Object Type in the Request.", data_message = data }));
                }
            }
            else
            {
                return await Task.FromResult(BadRequest(new { message = "Invalid Parameters in the Request.", Parameters = new { QueryName = queryName, StartHour = startHour, EndHour = endHour } }));
            }
        }

        private void ProcessIDSData(JToken result)
        {
            try
            {
                List<string> mpeNames = result.Select(item => item["MPE_NAME"]?.ToString()).Distinct().OrderBy(name => name).ToList();
                foreach (string mpeName in mpeNames)
                {
                    bool pushDBUpdate = false;
                    var geoZone = _geoZones.GetMPEName(mpeName);
                    if (geoZone != null && geoZone.Properties.MPERunPerformance != null)
                    {

                        if (geoZone.Properties.DataSource != "IDS")
                        {
                            geoZone.Properties.DataSource = "IDS";
                            pushDBUpdate = true;
                        }
                        List<string> hourslist = GetListofHours(24);
                        foreach (string hr in hourslist)
                        {
                            var mpeData = result.Where(item => item["MPE_NAME"]?.ToString() == mpeName && item["HOUR"]?.ToString() == hr).FirstOrDefault();
                            if (mpeData != null)
                            {
                                if (geoZone.Properties.MPERunPerformance.HourlyData.Where(h => h.Hour == hr).Any())
                                {
                                    geoZone.Properties.MPERunPerformance.HourlyData.Where(h => h.Hour == hr).ToList().ForEach(h =>
                                    {
                                        if (h.Sorted != (int)mpeData["SORTED"])
                                        {
                                            h.Sorted = (int)mpeData["SORTED"];
                                            pushDBUpdate = true;
                                        }

                                        if (h.Rejected != (int)mpeData["REJECTED"])
                                        {
                                            h.Rejected = (int)mpeData["REJECTED"];
                                            pushDBUpdate = true;
                                        }
                                        if (h.Count != (int)mpeData["INDUCTED"])
                                        {
                                            h.Count = (int)mpeData["INDUCTED"];
                                            pushDBUpdate = true;
                                        }

                                    });
                                }
                                else
                                {
                                    geoZone.Properties.MPERunPerformance.HourlyData.Add(new HourlyData
                                    {
                                        Hour = hr,
                                        Sorted = (int)mpeData["SORTED"],
                                        Rejected = (int)mpeData["REJECTED"],
                                        Count = (int)mpeData["INDUCTED"]
                                    });
                                    pushDBUpdate = true;
                                }
                            }
                            else
                            {
                                if (geoZone.Properties.MPERunPerformance.HourlyData.Where(h => h.Hour == hr).Any())
                                {
                                    geoZone.Properties.MPERunPerformance.HourlyData.Where(h => h.Hour == hr).ToList().ForEach(h =>
                                    {
                                        h.Sorted = 0;
                                        h.Rejected = 0;
                                        h.Count = 0;
                                    });
                                }
                                else
                                {
                                    geoZone.Properties.MPERunPerformance.HourlyData.Add(new HourlyData
                                    {
                                        Hour = hr,
                                        Sorted = 0,
                                        Rejected = 0,
                                        Count = 0
                                    });
                                }
                            }
                        }
                        if (pushDBUpdate)
                        {
                            _geoZones.Update(geoZone);
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error Processing data from");
            }
        }
        private List<string> GetListofHours(int hours)
        {
            var localTime = DateTime.Now;
            return Enumerable.Range(0, hours).Select(i => localTime.AddHours(-23).AddHours(i).ToString("yyyy-MM-dd HH:00")).ToList();
        }
    }
}
