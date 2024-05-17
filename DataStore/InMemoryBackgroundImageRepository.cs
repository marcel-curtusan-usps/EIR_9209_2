using EIR_9209_2.Models;
using System.Collections.Concurrent;

public class InMemoryBackgroundImageRepository : IInMemoryTagsBackgroundImageRepository
{
    private readonly static ConcurrentDictionary<string, BackgroundImage> _backgroundImages = new();

    public void Add(BackgroundImage backgroundImage)
    {
        _backgroundImages.TryAdd(backgroundImage.id, backgroundImage);
    }
    public void Remove(BackgroundImage backgroundImage) { _backgroundImages.TryRemove(backgroundImage.id, out _); }
    public BackgroundImage Get(string id)
    {
        _backgroundImages.TryGetValue(id, out BackgroundImage backgroundImage);
        return backgroundImage;
    }
    public IEnumerable<BackgroundImage> GetAll() => _backgroundImages.Values;
    public void Update(BackgroundImage backgroundImage)
    {
        if (_backgroundImages.TryGetValue(backgroundImage.id, out BackgroundImage currentBackgroundImage))
        {
            _backgroundImages.TryUpdate(backgroundImage.id, backgroundImage, currentBackgroundImage);
        }
    }
}