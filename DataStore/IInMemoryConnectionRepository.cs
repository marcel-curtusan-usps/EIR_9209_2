using EIR_9209_2.Models;

public interface IInMemoryConnectionRepository
{
    //Connection 
    Connection Get(string id);
    Task<Connection> Add(Connection connection);
    Task<Connection> Remove(string connectionId);
    Task<Connection> Update(Connection connection);
    IEnumerable<Connection> GetAll();
    Task<IEnumerable<Connection>> GetbyType(string type);


    //Connection types
    Task<ConnectionType> AddType(ConnectionType connection);
    Task<ConnectionType> RemoveType(string connectionId);
    Task<ConnectionType> GetType(string id);
    IEnumerable<ConnectionType> GetTypeAll();
    IEnumerable<ConnectionType> GetbyNameType(string type);
    Task<ConnectionType> UpdateType(ConnectionType connection);
    Task<Messagetype> AddSubType(string connectionId, Messagetype connection);
    Task<Messagetype> UpdateSubType(string connectionId, Messagetype connection);
    Task<Messagetype> RemoveSubType(string connectionId, string subId);
    Task<bool> ResetConnectionsList();
    Task<bool> SetupConnectionsList();
}