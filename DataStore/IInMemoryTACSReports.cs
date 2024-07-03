using EIR_9209_2.Models;

namespace EIR_9209_2.DataStore
{
    public interface IInMemoryTACSReports
    {
        void AddEmployeePayPeirods(List<TACSEmployeePayPeirod> employeePayPeirods);
    }
}