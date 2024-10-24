using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

public interface IInMemoryEmployeesRepository
{
    Task LoadEmployees(JToken data);
    IEnumerable<EmployeeInfo> GetAll(); 
    Task<bool> ResetEmployeesList();
    Task<bool> SetupEmployeesList();
    Task<bool> LoadHECSEmployees(Hces result, CancellationToken stoppingToken);
    Task<bool> LoadSMSEmployeeInfo(List<SMSWrapperEmployeeInfo> result, CancellationToken stoppingToken);
    Task<EmployeeInfo> GetEmployeeByBLE(string id);
    ConcurrentDictionary<string, EmployeeInfo> GetEMPInfo();
}