namespace EIR_9209_2.DataStore
{
    public interface IInMemoryApplicationRepository
    {
        Task<bool> Update(string key, string value, string section);
    }
}