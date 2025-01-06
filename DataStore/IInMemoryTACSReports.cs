using EIR_9209_2.Models;

namespace EIR_9209_2.DataStore
{
    public interface IInMemoryTACSReports
    {
        void AddEmployeePayPeirods(List<TACSEmployeePayPeirod> employeePayPeirods);
        Task<bool?> AddTacsRawRings(RawRings crsEvent);
        Task<List<RawRings>> GetTACSRawRings(string code);
        Task<List<string>> GetTopOpnCodes(string code);
        Task<bool> Reset();
        Task<bool> Setup();
    }
}