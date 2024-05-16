using EIR_9209_2.Models;
using MongoDB.Bson;
using MongoDB.Driver;

public class ConnectionRepository : IConnectionRepository
{
    private readonly IMongoCollection<Connection> _connection;
    public ConnectionRepository(MongoDBContext context)
    {
        _connection = context.Database.GetCollection<Connection>("connectionList");
    }
    public async Task Add(Connection con)
    {
        await _connection.InsertOneAsync(con).ConfigureAwait(false);
    }

    public async Task Delete(string id)
    {
        var filter = Builders<Connection>.Filter.Eq("Id", id);
        await _connection.DeleteOneAsync(filter).ConfigureAwait(false);
    }

    public async Task<Connection> Get(string id)
    {
        var filter = Builders<Connection>.Filter.Eq("Id", id);
        return await _connection.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<List<Connection>> GetAll()
    {
        return await _connection.Find(_ => true).ToListAsync().ConfigureAwait(false);
    }

    public async Task Update(Connection con)
    {
        var filter = Builders<Connection>.Filter.Eq("Id", con.Id);
        await _connection.ReplaceOneAsync(filter, con).ConfigureAwait(false);
    }
}