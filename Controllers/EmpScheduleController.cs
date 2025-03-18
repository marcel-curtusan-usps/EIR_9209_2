using EIR_9209_2.DataStore;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpScheduleController(ILogger<EmpScheduleController> logger, IInMemoryEmployeesRepository empRepository, IInMemoryEmployeesSchedule schedule) : ControllerBase
    {
        private readonly IInMemoryEmployeesRepository _emp = empRepository;
        private readonly ILogger<EmpScheduleController> _logger = logger;
        private readonly IInMemoryEmployeesSchedule _schedule = schedule;

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
                return await _schedule.GetEmployeesForPayWeek(payWeek);
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
                return await _schedule.GetPayWeeks();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        // GET: api/<EmpScheduleController>/Employees
        /// <summary>
        /// This provides a list of all employees
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("EmployeesList")]
        public async Task<object> GetAllEmployees()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return await Task.FromResult(BadRequest(ModelState));
                }
                return await _emp.GetEmployeesList();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        // GET: api/<EmpScheduleController>/Employees
        /// <summary>
        /// This provides a list of all employees
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("EmployeesData")]
        public async Task<object> GetEmployeeData(string ein)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return await Task.FromResult(BadRequest(ModelState));
                }
                return await _emp.GetEmployeeByEIN(ein);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

    }
}
