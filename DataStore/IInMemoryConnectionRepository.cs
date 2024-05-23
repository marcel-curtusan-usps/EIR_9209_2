using EIR_9209_2.Models;

public interface IInMemoryConnectionRepository
{
    void Add(Connection connection);
    void Remove(string connectionId);
    Connection Get(string id);
    IEnumerable<Connection> GetAll();
    IEnumerable<Connection> GetbyType(string type);
    void Update(Connection connection);
    void AddType(ConnectionType connection);
    void RemoveType(string connectionId);
    ConnectionType GetType(string id);
    IEnumerable<ConnectionType> GetTypeAll();
    IEnumerable<ConnectionType> GetbyNameType(string type);
    void UpdateType(ConnectionType connection);

}