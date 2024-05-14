using EIR_9209_2.Models;
using MongoDB.Bson;
using MongoDB.Driver;

public class TagsRepository : ITagsRepository
{
    private readonly IMongoCollection<TagGeoJson> _tags;
    public TagsRepository(MongoDBContext context)
    {
        _tags = context.Database.GetCollection<TagGeoJson>("tags");
    }
    public async Task Add(TagGeoJson tag)
    {
        await _tags.InsertOneAsync(tag);
    }

    public async Task Delete(string id)
    {
        var filter = Builders<TagGeoJson>.Filter.Eq("_id", new ObjectId(id));
        await _tags.DeleteOneAsync(filter);
    }

    public async Task<TagGeoJson> Get(string id)
    {
        var filter = Builders<TagGeoJson>.Filter.Eq("id", new ObjectId(id));
        return await _tags.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<TagGeoJson>> GetAllPersonTag()
    {
        return await _tags.Find(_ => true).ToListAsync();
    }

    public async Task Update(TagGeoJson tag)
    {
        var filter = Builders<TagGeoJson>.Filter.Eq("_id", new ObjectId(tag.Properties.Id));
        await _tags.ReplaceOneAsync(filter, tag);
    }
}