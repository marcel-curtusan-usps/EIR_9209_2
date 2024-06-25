using EIR_9209_2.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Xml.Linq;

public class InMemoryBackgroundImageRepository : IInMemoryBackgroundImageRepository
{
    private readonly static ConcurrentDictionary<string, BackgroundImage> _backgroundImages = new();
    private readonly ILogger<InMemoryBackgroundImageRepository> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFileService FileService;
    private readonly string filePath = "";
    private readonly string fileName = "";
    public InMemoryBackgroundImageRepository(ILogger<InMemoryBackgroundImageRepository> logger, IConfiguration configuration, IFileService fileService)
    {
        FileService = fileService;
        _logger = logger;
        _configuration = configuration;
        fileName = $"{_configuration[key: "InMemoryCollection:CollectionBackgroundImages"]}.json";
        filePath = Path.Combine(_configuration[key: "ApplicationConfiguration:BaseDrive"],
            _configuration[key: "ApplicationConfiguration:BaseDirectory"],
            _configuration[key: "ApplicationConfiguration:NassCode"],
            _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"],
           $"{fileName}");
        // Load data from the first file into the first collection
        _ = LoadDataFromFile(filePath);

    }
    public void Add(BackgroundImage backgroundImage)
    {
        if (_backgroundImages.TryAdd(backgroundImage.id, backgroundImage))
        {
            FileService.WriteFile(fileName, JsonConvert.SerializeObject(_backgroundImages.Values, Formatting.Indented));
        }
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
        if (_backgroundImages.TryGetValue(backgroundImage.id, out BackgroundImage currentBackgroundImage) && _backgroundImages.TryUpdate(backgroundImage.id, backgroundImage, currentBackgroundImage))
        {
            FileService.WriteFile(fileName, JsonConvert.SerializeObject(_backgroundImages.Values, Formatting.Indented));
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

    public Task LoadBackgroundImage(BackgroundImage newImage)
    {
        try
        {
            var imageId = _backgroundImages.Where(x => x.Value.fileName == newImage.fileName).Select(x => x.Key).FirstOrDefault();
            if (imageId != null)
            {
                _backgroundImages[imageId] = newImage;
                FileService.WriteFile(fileName, JsonConvert.SerializeObject(_backgroundImages.Values, Formatting.Indented));
            }
            else
            {
                Add(newImage);
            }
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Task.FromResult(false);
        }
    }
}