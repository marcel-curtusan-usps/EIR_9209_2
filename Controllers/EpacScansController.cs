using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using EIR_9209_2.DataStore;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using EIR_9209_2.Models;
namespace EIR_9209_2.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]

    public class EpacScansController(ILogger<EpacScansController> logger, IInMemoryGeoZonesRepository zones, IInMemoryEmployeesRepository employees, IHubContext<HubServices> hubContext, IInMemoryTACSReports tacs) : ControllerBase
    {
        private readonly IInMemoryEmployeesRepository _employees = employees;
        private readonly IInMemoryTACSReports _tacs = tacs;
        private readonly IHubContext<HubServices> _hubContext = hubContext;
        private readonly ILogger<EpacScansController> _logger = logger;
        private readonly IInMemoryGeoZonesRepository _zones = zones;
        // POST api/<EpacScansController>
        [HttpPost]
        [Route("BadgeScan")]
        public async Task<ActionResult> PostAddRawRings([FromBody] JObject scan)
        {
            try
            {
                //handle bad requests
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (scan.HasValues)
                {
                    //log the scan 
                    // {
                    //   "action": "setSmsData",
                    //   "Result": "success",
                    //   "Message": null,
                    //   "data": {
                    //     "Transactions": [
                    //        {
                    //         "TimeAdded": 1735822815054,
                    //         "encodedID": "90001260",
                    //         "transactiondatetime": "1/2/2025 5:00:37 AM",
                    //         "cardholderid": 481706,
                    //         "areaid": 2665,
                    //         "deviceid": 69579,
                    //         "cardholderdata": {
                    //           "currentStatus": "ACTIVE",
                    //           "title": "MAIL HANDLER",
                    //           "cardholderID": 481706,
                    //           "firstname": "name",
                    //           "lastname": "name",
                    //           "activation": "Wednesday, April 14, 2010",
                    //           "expiration": "Thursday, September 30, 2027",
                    //           "blocked": false,
                    //           "ein": "03073644",
                    //           "designationActivity": "120",
                    //           "dutyStationFDBID": "",
                    //           "dutystationFinanceNumber": "",
                    //           "importField": "f0eb8e5bd50a"
                    //         }
                    //       }
                    //     ]
                    //   }
                    // }
                    _logger.LogInformation($"Scan Data {JsonConvert.SerializeObject(scan, Formatting.None)}");

                    //update Employee Info
                    ScanInfo scanInfo = scan.ToObject<ScanInfo>();
                    DateTime activationDate;
                    DateTime expirationDate;
                    DateTime.TryParse(scan["data"]["Transactions"][0]["cardholderdata"]["activation"]?.ToString(), out activationDate);
                    DateTime.TryParse(scan["data"]["Transactions"][0]["cardholderdata"]["expiration"]?.ToString(), out expirationDate);
                    scanInfo.Data.Transactions.FirstOrDefault().CardholderData.Activation = activationDate;
                    scanInfo.Data.Transactions.FirstOrDefault().CardholderData.Expiration = expirationDate;
                    _ = Task.Run(() => _employees.UpdateEmployeeInfoFromEPAC(scanInfo)).ConfigureAwait(false);
                    var transaction = scan["data"]?["Transactions"]?.FirstOrDefault();
                    if (transaction == null)
                    {
                        //log request
                        _logger.LogInformation($"Scan Data {JsonConvert.SerializeObject(scan, Formatting.None)}");
                        return Ok();
                    }
                    if (string.IsNullOrEmpty(scanInfo.Data.Transactions.FirstOrDefault().CardholderData.EIN)
                    || string.IsNullOrEmpty(scanInfo.Data.Transactions.FirstOrDefault().DeviceID.ToString())
                    || string.IsNullOrEmpty(scanInfo.Data.Transactions.FirstOrDefault().AreaID.ToString()))
                    {
                        return Ok();
                        //return BadRequest("One or more required fields are missing or null.");
                    }

                    var kioskConfig = await _zones.CheckKioskZone(scanInfo.Data.Transactions.FirstOrDefault().DeviceID.ToString());

                    if (scanInfo.Data.Transactions.FirstOrDefault().DeviceID.ToString() != null && kioskConfig.IsFound)
                    {
                        await _hubContext.Clients.Group("CRS").SendAsync("epacScan",
                         new
                         {
                             kioskId = kioskConfig.KioskId,
                             kioskName = kioskConfig.KioskName,
                             kioskNumber = kioskConfig.KioskNumber,
                             deviceId = scanInfo.Data.Transactions.FirstOrDefault().DeviceID.ToString(),
                             id = scanInfo.Data.Transactions.FirstOrDefault().CardholderData.EIN
                         },
                         CancellationToken.None);
                    }
                    else
                    {

                        _logger.LogInformation($"Device Id {scan["deviceId"]}");
                    }

                }
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
    }
}

