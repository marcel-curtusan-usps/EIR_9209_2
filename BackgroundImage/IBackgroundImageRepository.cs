using EIR_9209_2.Models;

public interface IBackgroundImageRepository
{
    Task<BackgroundImage> Get(string id);
    Task<List<BackgroundImage>> GetAll();
    Task Add(BackgroundImage image);
    Task Update(BackgroundImage image);
    Task Delete(string id);
}