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
        [HttpGet]
        [Route("/EmpSchedule")]
        public async Task<object> GetEmpSchedule()
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(BadRequest(ModelState));
            }
            return _empsch.getEmpSchedule();
        }


    }
}
