namespace EIR_9209_2.Utilities
{
    public interface IResetApplication
    {
        Task<bool> GetNewSiteInfo(string? value);
        Task<bool> Reset();
        Task<bool> Setup();
    }
}