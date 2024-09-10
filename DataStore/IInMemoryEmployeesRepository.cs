using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IInMemoryEmployeesRepository
{
    Task LoadEmployees(JToken data);
    Task LoadEmpSchedule(JToken data);
    IEnumerable<EmployeeInfo> GetAll();
    object getEmpSchedule();
    void RunEmpScheduleReport();

}