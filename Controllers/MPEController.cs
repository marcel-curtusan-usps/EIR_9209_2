using EIR_9209_2.DatabaseCalls.MPE;
using Microsoft.AspNetCore.Mvc;
using EIR_9209_2.Service;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MPEController : ControllerBase
    {
        private readonly ILogger<MPEController> _logger;
        private readonly IWorker _worker;
        private readonly IMpe _MPE;
        private readonly IInMemoryGeoZonesRepository _geoZones;
        public MPEController(ILogger<MPEController> logger, IWorker worker, IMpe MPE, IInMemoryGeoZonesRepository geoZones)
        {
            _logger = logger;
            _worker = worker;
            _MPE = MPE;
            _geoZones = geoZones;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryName"></param>
        /// <param name="startHour"></param>
        /// <param name="endHour"></param>
        /// <returns></returns>
        // GET: api/<MPE>
        [HttpGet]
        [Route("GetMPEData")]
        public async Task<object> GetByMPE(string queryName, int startHour, int endHour)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var queryParmaters = HttpContext?.Request?.Query ?? new QueryCollection();
            string? parameterQueryName = queryName ?? (queryParmaters.ContainsKey("queryName") ? queryParmaters["queryName"].ToString() : string.Empty);
            int parameterStartHour = startHour > 0 ? startHour : queryParmaters.ContainsKey("startHour") ? int.Parse(queryParmaters["startHour"]) : 0;
            int parameterEndHour = endHour > 0 ? endHour : queryParmaters.ContainsKey("endHour") ? int.Parse(queryParmaters["endHour"]) : 0;

            if (!string.IsNullOrEmpty(parameterQueryName) && parameterStartHour > 0 && parameterStartHour < 24 && parameterEndHour > 0 && parameterEndHour < 24)
            {
                JObject data = new JObject
                {
                    ["startHour"] = parameterStartHour,
                    ["endHour"] = parameterEndHour,
                    ["queryName"] = parameterQueryName
                };
                if (data.HasValues && data.Type == JTokenType.Object)
                {
                    var (status, callData) = await _worker.FetchDataOndemand(data);
                    if (status && callData != null)
                    {
                        if (callData is JToken result && result.Type == JTokenType.Array && result.HasValues)
                        {
                            _ = Task.Run(() => _geoZones.ProcessIDSData(result, CancellationToken.None)).ConfigureAwait(false);
                            return Ok(result);
                        }
                        else
                        {
                            return BadRequest(callData);
                        }
                    }
                    else
                    {
                        return BadRequest(callData);
                    }
                }
                else
                {
                    return BadRequest(new { message = "Invalid Object Type in the Request.", data_message = data });
                }
            }
            else
            {
                return BadRequest(new { message = "Invalid Parameters in the Request.", Parameters = new { QueryName = parameterQueryName, StartHour = parameterStartHour, EndHour = parameterEndHour } });
            }
        }
        /// <summary>
        /// Post MPE Data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MPEData")]
        public async Task<object> PostByMPEData([FromBody] JObject reqBody)
        {   //handle bad requests
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            var payload = (JObject?)(reqBody ?? new JObject());
            var queryParmaters = HttpContext?.Request?.Query ?? new QueryCollection();
            string? parameterQueryName = payload.ContainsKey("queryName") ? payload["queryName"]?.ToString() : queryParmaters.ContainsKey("queryName") ? queryParmaters["queryName"].ToString() : string.Empty;
            int parameterStartHour = payload.ContainsKey("startHour") ? payload["startHour"]?.ToObject<int>() ?? 0 : queryParmaters.ContainsKey("startHour") ? int.Parse(queryParmaters["startHour"]) : 0;
            int parameterEndHour = payload.ContainsKey("endHour") ? payload["endHour"]?.ToObject<int>() ?? 0 : queryParmaters.ContainsKey("endHour") ? int.Parse(queryParmaters["endHour"]) : 0;
           

            if (!string.IsNullOrEmpty(parameterQueryName) && parameterStartHour > 0 && parameterStartHour < 24 && parameterEndHour > 0 && parameterEndHour < 24)
            {
                JObject data = new JObject
                {
                    ["startHour"] = parameterStartHour,
                    ["endHour"] = parameterEndHour,
                    ["queryName"] = parameterQueryName
                };
                if (data.HasValues && data.Type == JTokenType.Object)
                {
                    var (status, callData) = await _worker.FetchDataOndemand(data);
                    if (status && callData != null)
                    {
                        if (callData is JToken result && result.Type == JTokenType.Array && result.HasValues)
                        {
                            _ = Task.Run(() => _geoZones.ProcessIDSData(result, CancellationToken.None)).ConfigureAwait(false);
                            return Ok(result);
                        }
                        else
                        {
                            return BadRequest(callData);
                        }
                    }
                    else
                    {
                        return BadRequest(callData);
                    }
                }
                else
                {
                    return BadRequest(new { message = "Invalid Object Type in the Request.", data_message = data });
                }
            }
            else
            {
                return BadRequest(new { message = "Invalid Parameters in the Request.", Parameters = new { QueryName = parameterQueryName, StartHour = parameterStartHour, EndHour = parameterEndHour } });
            }
        }
    }
}
