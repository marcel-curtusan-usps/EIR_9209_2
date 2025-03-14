using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.DataStore
{
    public interface IInMemoryEmployeesRepository
    {
        Task LoadEmployees(JToken data);
        Task<IEnumerable<EmployeeInfo>> GetAll();
        Task<bool> Reset();
        Task<bool> Setup();
        Task<bool> LoadHECSEmployees(Hces result, CancellationToken stoppingToken);
        Task<bool> LoadSMSEmployeeInfo(List<SMSWrapperEmployeeInfo> result, CancellationToken stoppingToken);
        void UpdateEmployeeInfoFromEPAC(JObject epac);
        Task<EmployeeInfo?> GetEmployeeByBLE(string id);
        Task<EmployeeInfo?> GetEmployeeByEIN(string id);
        Task<object?> GetEmployeeByCode(string code);
        Task<List<string?>> GetDistinctEmployeeIdList();
        Task<List<JObject>> SearchEmployee(string search);
        Task<object> GetEmployeesList();

    }
}