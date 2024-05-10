using EIR_9209_2.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class BackgroundImageRepository : IBackgroundImageRepository
{
    private readonly IMongoCollection<BackgroundImage> _backgroundImages;

    public BackgroundImageRepository(MongoDBContext context)
    {
        _backgroundImages = context.Database.GetCollection<BackgroundImage>("backgroundImages");
    }
    public async Task Add(BackgroundImage image)
    {
        await _backgroundImages.InsertOneAsync(image);
    }

    public async Task Delete(string id)
    {
        var filter = Builders<BackgroundImage>.Filter.Eq("_id", new ObjectId(id));
        await _backgroundImages.DeleteOneAsync(filter);
    }

    public async Task<BackgroundImage> Get(string id)
    {
        var filter = Builders<BackgroundImage>.Filter.Eq("_id", new ObjectId(id));
        return await _backgroundImages.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<BackgroundImage>> GetAll()
    {
        return await _backgroundImages.Find(_ => true).ToListAsync();
    }

    public async Task Update(BackgroundImage image)
    {
        var filter = Builders<BackgroundImage>.Filter.Eq("_id", new ObjectId(image.id));
        await _backgroundImages.ReplaceOneAsync(filter, image);
    }
}