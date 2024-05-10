using EIR_9209_2.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class MongoDBContext
{
    public IMongoDatabase Database { get; }

    public MongoDBContext(IOptions<MongoDBSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        Database = (IMongoDatabase?)client.GetDatabase(settings.Value.DatabaseName);
    }
}
