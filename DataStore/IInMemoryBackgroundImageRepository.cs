using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IInMemoryBackgroundImageRepository
{
    Task<OSLImage> Add(OSLImage backgroundImage);
    Task<OSLImage> Update(OSLImage backgroundImage);
    Task<OSLImage> Remove(string id);
    OSLImage Get(string id);
    IEnumerable<OSLImage> GetAll();
    Task<bool> ResetBackgroundImageList();
    Task<bool> SetupBackgroundImageList();
    Task<bool> ProcessBackgroundImage(List<CoordinateSystem> coordinateSystems, CancellationToken stoppingToken);
    Task<bool> ProcessCiscoSpacesBackgroundImage(JToken? jToken, CancellationToken stoppingToken);
}
