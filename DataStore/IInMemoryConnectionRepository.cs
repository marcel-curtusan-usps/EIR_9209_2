using EIR_9209_2.Models;

public interface IInMemoryConnectionRepository
{
    //Connection 
    Connection Get(string id);
    Connection Add(Connection connection);
    Connection Remove(string connectionId);
    Connection Update(Connection connection);
    IEnumerable<Connection> GetAll();
    IEnumerable<Connection> GetbyType(string type);


    //Connection types
    void AddType(ConnectionType connection);
    void RemoveType(string connectionId);
    ConnectionType GetType(string id);
    IEnumerable<ConnectionType> GetTypeAll();
    IEnumerable<ConnectionType> GetbyNameType(string type);
    void UpdateType(ConnectionType connection);

}