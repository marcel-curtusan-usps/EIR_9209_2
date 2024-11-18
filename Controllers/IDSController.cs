using EIR_9209_2.DatabaseCalls.IDS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryName"></param>
        /// <param name="startHour"></param>
        /// <param name="endHour"></param>
        /// <returns></returns>
        // GET: api/<IDS>
        [HttpGet]
        [Route("GetIDSData")]
        public async Task<object> GetByIDS(string queryName, int startHour, int endHour)
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!string.IsNullOrEmpty(queryName))
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
                        _ = Task.Run(() => _geoZones.ProcessIDSData(result, CancellationToken.None)).ConfigureAwait(false);
                    }
                    if (result.Type == JTokenType.Array)
                    {
                        return await Task.FromResult(Ok(result));
                    }
                    else
                    {
                        if (((JObject)result).ContainsKey("Error"))
                        {
                            return await Task.FromResult(BadRequest(result)).ConfigureAwait(false);

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
        //// POST api/<IDSController>
        //[HttpPost]
        //[ApiExplorerSettings(IgnoreApi = true)]
        //[Route("GetIDSData")]
        //public async Task<object> PostByGetIDSData([FromBody] JToken data)
        //{   //handle bad requests
        //    if (!ModelState.IsValid)
        //    {
        //        return await Task.FromResult(BadRequest(ModelState));
        //    }
        //    else if (data != null && data.HasValues)
        //    {
        //        if (data.HasValues && data.Type == JTokenType.Object)
        //        {
        //            JToken result = await _ids.GetOracleIDSData(data);

        //            if (((JObject)result).ContainsKey("Error"))
        //            {
        //                return await Task.FromResult(result);

        //            }
        //            else
        //            {
        //                return await Task.FromResult(result);
        //            }
        //        }

        //        else
        //        {
        //            return await Task.FromResult(BadRequest());
        //        }
        //    }
        //    else
        //    {
        //        return await Task.FromResult(BadRequest());
        //    }
        //}
        [HttpPost]
        [Route("IDSData")]
        public async Task<object> PostByIDSData(string queryName, int startHour, int endHour)
        {   //handle bad requests
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }

            if (!string.IsNullOrEmpty(queryName) && (startHour > 0) && endHour > 0 && endHour < 24)
            {
                JObject data = new JObject
                {
                    ["startHour"] = startHour,
                    ["endHour"] = endHour,
                    ["queryName"] = queryName
                };
                if (data.HasValues && data.Type == JTokenType.Object)
                {
                    return Ok(await _ids.GetOracleIDSData(data));
                    //if (result.HasValues)
                    //{
                    //    _ = Task.Run(() => _geoZones.ProcessIDSData(result)).ConfigureAwait(false);
                    //}
                    //if (result.Type == JTokenType.Array)
                    //{
                    //    return await Task.FromResult(Ok(result));
                    //}
                    //else
                    //{
                    //    if (((JObject)result).ContainsKey("Error"))
                    //    {
                    //        return await Task.FromResult(BadRequest(result)).ConfigureAwait(false);

                    //    }
                    //    else
                    //    {
                    //        return await Task.FromResult(Ok(result));
                    //    }
                    //}

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
    }
}
