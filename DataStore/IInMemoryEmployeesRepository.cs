using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IInMemoryEmployeesRepository
{
    Task LoadEmployees(JToken data);
    Task LoadEmpSchedule(JToken data);
    IEnumerable<EmployeeInfo> GetAll();
    void RunEmpScheduleReport();
    Task<List<string>> GetPayWeeks();
    Task<List<ScheduleReport>> GetEmployeesForPayWeek(string payWeek);
    Task<bool> ResetEmployeesList();
    Task<bool> SetupEmployeesList();
    Task<bool> LoadHECSEmployees(Hces result, CancellationToken stoppingToken);
    Task<bool> LoadSMSEmployeeInfo(List<SMSWrapperEmployeeInfo> result, CancellationToken stoppingToken);
    Task<EmployeeInfo> GetEmployeeByBLE(string id);
}