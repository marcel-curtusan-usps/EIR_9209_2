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
    public async Task Add(Connection image)
    {
        await _connection.InsertOneAsync(image);
    }

    public async Task Delete(string id)
    {
        var filter = Builders<Connection>.Filter.Eq("_id", new ObjectId(id));
        await _connection.DeleteOneAsync(filter);
    }

    public async Task<Connection> Get(string id)
    {
        var filter = Builders<Connection>.Filter.Eq("_id", new ObjectId(id));
        return await _connection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<Connection>> GetAll()
    {
        return await _connection.Find(_ => true).ToListAsync();
    }

    public async Task Update(Connection image)
    {
        var filter = Builders<Connection>.Filter.Eq("_id", new ObjectId(image.Id));
        await _connection.ReplaceOneAsync(filter, image);
    }
}