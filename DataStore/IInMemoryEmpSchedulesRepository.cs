using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IInMemoryEmpSchedulesRepository
{
    Task LoadEmpInfo(JToken data);
    Task LoadEmpSchedule(JToken data);
}