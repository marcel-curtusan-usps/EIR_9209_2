using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

public class MongoDbHealthCheck : IHealthCheck
{
    private readonly IMongoDatabase _database;

    public MongoDbHealthCheck(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Ping the database. This will throw an exception if we can't connect to it.
            await _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}", cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to connect to MongoDB", ex);
        }
    }
}
