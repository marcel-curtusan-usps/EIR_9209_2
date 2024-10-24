using Newtonsoft.Json.Linq;

namespace EIR_9209_2.DataStore
{
    public interface IInMemoryEmployeesSchedule
    {
        Task LoadEmpSchedule(JToken data);
        void RunEmpScheduleReport();
        Task<List<string>> GetPayWeeks();
        Task<List<ScheduleReport>> GetEmployeesForPayWeek(string payWeek);
        Task<bool> ResetScheduleList();
    }
}