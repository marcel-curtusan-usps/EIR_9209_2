public interface IConnectionRepository
{
    Task<Connection> Get(string id);
    Task<List<Connection>> GetAll();
    Task Add(Connection connection);
    Task Update(Connection connection);
    Task Delete(string id);
}