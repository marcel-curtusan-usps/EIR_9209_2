

namespace EIR_9209_2.DataStore
{
    public interface IInMemorySiteInfoRepository
    {
        void Add(SiteInformation site);
        void Remove(string id);
        void Update(SiteInformation site);
        SiteInformation Get(string id);
        Task<SiteInformation> GetSiteInfo();
        Task<DateTime> GetCurrentTimeInTimeZone(DateTime now);
        Task<bool> ResetSiteInfoList();
        Task<bool> SetupSiteInfoList();
    }
}