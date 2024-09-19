using EIR_9209_2.Models;

public interface IInMemoryBackgroundImageRepository
{
    Task<BackgroundImage> Add(BackgroundImage backgroundImage);
    Task<BackgroundImage> Update(BackgroundImage backgroundImage);
    Task<BackgroundImage> Remove(string id);
    BackgroundImage Get(string id);
    IEnumerable<BackgroundImage> GetAll();
    Task<bool> ResetBackgroundImageList();
    Task<bool> SetupBackgroundImageList();
}