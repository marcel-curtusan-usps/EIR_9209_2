using EIR_9209_2.Models;

public interface IInMemoryConnectionRepository
{
    //Connection 
    Connection Get(string id);
    Connection Add(Connection connection);
    Connection Remove(string connectionId);
    Task Update(Connection connection);
    IEnumerable<Connection> GetAll();
    IEnumerable<Connection> GetbyType(string type);


    //Connection types
    ConnectionType AddType(ConnectionType connection);
    ConnectionType RemoveType(string connectionId);
    ConnectionType GetType(string id);
    IEnumerable<ConnectionType> GetTypeAll();
    IEnumerable<ConnectionType> GetbyNameType(string type);
    ConnectionType UpdateType(ConnectionType connection);
    Messagetype AddSubType(string connectionId, Messagetype connection);
    Messagetype UpdateSubType(string connectionId, Messagetype connection);
    Messagetype RemoveSubType(string connectionId, string subId);
}