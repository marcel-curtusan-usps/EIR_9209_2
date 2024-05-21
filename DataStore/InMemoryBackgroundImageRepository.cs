using EIR_9209_2.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;

public class InMemoryBackgroundImageRepository : IInMemoryBackgroundImageRepository
{
    private readonly static ConcurrentDictionary<string, BackgroundImage> _backgroundImages = new();
    private readonly ILogger<InMemoryBackgroundImageRepository> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFileService FileService;
    public InMemoryBackgroundImageRepository(ILogger<InMemoryBackgroundImageRepository> logger, IConfiguration configuration, IFileService fileService)
    {
        FileService = fileService;
        _logger = logger;
        _configuration = configuration;
        string BuildConnectionPath = Path.Combine(configuration[key: "ApplicationConfiguration:BaseDrive"], configuration[key: "ApplicationConfiguration:BaseDirectory"], configuration[key: "SiteIdentity:NassCode"], configuration[key: "ApplicationConfiguration:ConfigurationDirectory"], $"{configuration[key: "InMemoryCollection:CollectionBackgroundImages"]}.json");
        // Load data from the first file into the first collection
        _ = LoadDataFromFile(BuildConnectionPath);

    }
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
    private async Task LoadDataFromFile(string filePath)
    {
        try
        {
            // Read data from file
            var fileContent = await FileService.ReadFile(filePath);

            // Parse the file content to get the data. This depends on the format of your file.
            // Here's an example if your file was in JSON format and contained an array of T objects:
            List<BackgroundImage> data = JsonConvert.DeserializeObject<List<BackgroundImage>>(fileContent);

            // Insert the data into the MongoDB collection
            if (data.Any())
            {
                foreach (BackgroundImage item in data.Select(r => r).ToList())
                {
                    _backgroundImages.TryAdd(item.id, item);
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            // Handle the FileNotFoundException here
            _logger.LogError($"File not found: {ex.FileName}");
            // You can choose to throw an exception or take any other appropriate action
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            _logger.LogError($"An error occurred when reading the file: {ex.Message}");
        }
        catch (JsonException ex)
        {
            // Handle errors when parsing the JSON
            _logger.LogError($"An error occurred when parsing the JSON: {ex.Message}");
        }
    }
}