using EIR_9209_2.Models;
using MongoDB.Driver;

public class GeoZonesRepository : IGeoZonesRepository
{
    private readonly IMongoCollection<GeoZone> _connection;
    public GeoZonesRepository(MongoDBContext context)
    {
        _connection = context.Database.GetCollection<GeoZone>("geoZoneList");
    }
    public async Task Add(GeoZone zone)
    {
        await _connection.InsertOneAsync(zone);
    }

    public async Task Delete(string id)
    {
        var filter = Builders<GeoZone>.Filter.Eq("Id", id);
        await _connection.DeleteOneAsync(filter);
    }

    public async Task<GeoZone> Get(string id)
    {
        var filter = Builders<GeoZone>.Filter.Eq("Id", id);
        return await _connection.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<List<GeoZone>> GetAll()
    {
        return await _connection.Find(_ => true).ToListAsync().ConfigureAwait(false);
    }

    public async Task Update(GeoZone zone)
    {
        var filter = Builders<GeoZone>.Filter.Eq("Id", zone.Properties.Id);
        await _connection.ReplaceOneAsync(filter, zone).ConfigureAwait(false);
    }
}