using EIR_9209_2.Models;

public interface IInMemoryTagsBackgroundImageRepository
{
    void Add(BackgroundImage backgroundImage);
    void Remove(BackgroundImage backgroundImage);
    BackgroundImage Get(string id);
    IEnumerable<BackgroundImage> GetAll();
    void Update(BackgroundImage backgroundImage);
}