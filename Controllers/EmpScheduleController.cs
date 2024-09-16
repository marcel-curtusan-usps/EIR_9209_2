using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpScheduleController(ILogger<EmpScheduleController> logger, IInMemoryEmployeesRepository empschRepository) : ControllerBase
    {
        private readonly IInMemoryEmployeesRepository _empsch = empschRepository;
        private readonly ILogger<EmpScheduleController> _logger = logger;

        // GET: api/<EmpScheduleController>
        /// <summary>
        /// this will provide the employee schedule for the pay week
        /// </summary>
        /// <param name="payWeek"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("EmployeesSchedule")]
        public async Task<object> GetEmpSchedule(string payWeek)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return await Task.FromResult(BadRequest(ModelState));
                }
                return await _empsch.GetEmployeesForPayWeek(payWeek);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }

        }
        /// <summary>
        /// This provide the a list of pay weeks
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        [Route("PayWeekList")]
        public async Task<object> GetPayWeek()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return await Task.FromResult(BadRequest(ModelState));
                }
                return await _empsch.GetPayWeeks();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
    }
}
