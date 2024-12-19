using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

public interface IInMemoryEmployeesRepository
{
    Task LoadEmployees(JToken data);
    Task<IEnumerable<EmployeeInfo>> GetAll(); 
    Task<bool> Reset();
    Task<bool> Setup();
    Task<bool> LoadHECSEmployees(Hces result, CancellationToken stoppingToken);
    Task<bool> LoadSMSEmployeeInfo(List<SMSWrapperEmployeeInfo> result, CancellationToken stoppingToken);
    Task<EmployeeInfo?> GetEmployeeByBLE(string id);
    Task<EmployeeInfo?> GetEmployeeByEIN(string id);
    Task<object?> GetEmployeeByCode(string code);
    Task<List<string?>> GetDistinctEmployeeIdList();
}