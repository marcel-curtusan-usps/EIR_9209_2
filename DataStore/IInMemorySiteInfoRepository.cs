namespace EIR_9209_2.DataStore
{
    public interface IInMemorySiteInfoRepository
    {
        void Add(SiteInformation site);
        void Remove(string id);
        SiteInformation Get(string id);
        List<SiteInformation> GetAll();
        void Update(SiteInformation site);
        SiteInformation GetByNASSCode(string id);
    }
}