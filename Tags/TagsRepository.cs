using EIR_9209_2.Models;
using MongoDB.Bson;
using MongoDB.Driver;

public class TagsRepository : ITagsRepository
{
    private readonly IMongoCollection<GeoMarker> _tags;
    public TagsRepository(MongoDBContext context)
    {
        _tags = context.Database.GetCollection<GeoMarker>("tagsList");
    }
    public async Task Add(GeoMarker tag)
    {
        await _tags.InsertOneAsync(tag).ConfigureAwait(false);
    }

    public async Task Delete(string id)
    {
        var filter = Builders<GeoMarker>.Filter.Eq("Id", new ObjectId(id));
        await _tags.DeleteOneAsync(filter).ConfigureAwait(false); ;
    }

    public async Task<GeoMarker> Get(string id)
    {
        var filter = Builders<GeoMarker>.Filter.Eq(x => x.Properties.Id, id);
        return await _tags.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<List<GeoMarker>> GetAllPersonTag()
    {
        return await _tags.Find(_ => true).ToListAsync().ConfigureAwait(false);
    }

    public async Task Update(GeoMarker tag)
    {
        var filter = Builders<GeoMarker>.Filter.Eq(x => x.Properties.Id, tag._id);
        await _tags.ReplaceOneAsync(filter, tag).ConfigureAwait(false);
    }
}