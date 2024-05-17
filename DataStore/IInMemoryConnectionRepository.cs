public interface IInMemoryConnectionRepository
{
    void Add(Connection connection);
    void Remove(string connectionId);
    Connection Get(string id);
    IEnumerable<Connection> GetAll();

    void Update(Connection connection);
}