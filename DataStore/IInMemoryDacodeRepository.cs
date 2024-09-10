using EIR_9209_2.Models;

public interface IInMemoryDacodeRepository
{
    //Connection 
    DesignationActivityToCraftType Get(string id);
    Task<DesignationActivityToCraftType> Add(DesignationActivityToCraftType dacode);
    Task<DesignationActivityToCraftType> Remove(string dacodeId);
    Task<DesignationActivityToCraftType> Update(DesignationActivityToCraftType dacode);
    IEnumerable<DesignationActivityToCraftType> GetAll();

}