using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IInMemoryEmpSchedulesRepository
{
    void LoadEmpInfo(JToken data);
    void LoadEmpSchedule(JToken data);
    IEnumerable<EmployeeInfo> GetAll();
    object getEmpSchedule();
    void UpdateEmpScheduleSels();
    void RunEmpScheduleReport();

}