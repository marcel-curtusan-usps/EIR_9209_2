using EIR_9209_2.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;

public class InMemoryBackgroundImageRepository : IInMemoryBackgroundImageRepository
{
    private readonly static ConcurrentDictionary<string, BackgroundImage> _backgroundImages = new();
    private readonly ILogger<InMemoryBackgroundImageRepository> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFileService _fileService;
    private readonly string fileName = "BackgroundImages.json";
    public InMemoryBackgroundImageRepository(ILogger<InMemoryBackgroundImageRepository> logger, IConfiguration configuration, IFileService fileService)
    {
        _fileService = fileService;
        _logger = logger;
        _configuration = configuration;
        // Load data from the first file into the first collection
        LoadDataFromFile().Wait();

    }
    public async Task<BackgroundImage>? Add(BackgroundImage backgroundImage)
    {
        bool saveToFile = false;
        try
        {

            if (_backgroundImages.TryAdd(backgroundImage.id, backgroundImage))
            {
                saveToFile = true;
                return await Task.FromResult(backgroundImage);
            }
            else
            {
                _logger.LogError($"Zone File list was not saved...");
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_backgroundImages.Values, Formatting.Indented));
            }
        }
    }
    public async Task<BackgroundImage>? Remove(string id)
    {
        bool saveToFile = false;
        try
        {
            if (_backgroundImages.TryRemove(id, out BackgroundImage backgroundImage))
            {
                saveToFile = true;
                return await Task.FromResult(backgroundImage);
            }
            else
            {
                _logger.LogError($"Zone File list was not saved...");
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_backgroundImages.Values, Formatting.Indented));
            }
        }
    }
    public async Task<BackgroundImage>? Update(BackgroundImage backgroundImage)
    {
        if (_backgroundImages.TryGetValue(backgroundImage.id, out BackgroundImage currentBackgroundImage) && _backgroundImages.TryUpdate(backgroundImage.id, backgroundImage, currentBackgroundImage))
        {
            await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_backgroundImages.Values, Formatting.Indented));
        }

        bool saveToFile = false;
        try
        {
            if (_backgroundImages.TryGetValue(backgroundImage.id, out BackgroundImage? currentimage) && _backgroundImages.TryUpdate(backgroundImage.id, backgroundImage, currentimage))
            {
                saveToFile = true;
                if (_backgroundImages.TryGetValue(backgroundImage.id, out BackgroundImage? osl))
                {

                    return await Task.FromResult(osl);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_backgroundImages.Values, Formatting.Indented));
            }
        }
    }
    public BackgroundImage Get(string id)
    {
        _backgroundImages.TryGetValue(id, out BackgroundImage? backgroundImage);
        return backgroundImage;
    }
    public IEnumerable<BackgroundImage> GetAll() => _backgroundImages.Values;

    private async Task LoadDataFromFile()
    {
        try
        {
            // Read data from file
            var fileContent = await _fileService.ReadFile(fileName);
            if (!string.IsNullOrEmpty(fileContent))
            {
                // Parse the file content to get the data. This depends on the format of your file.
                var data = JsonConvert.DeserializeObject<List<BackgroundImage>>(fileContent) ?? new List<BackgroundImage>();
                if (data.Count > 0)
                {
                    foreach (BackgroundImage item in data.Select(r => r).ToList())
                    {
                        _backgroundImages.TryAdd(item.id, item);
                    }
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

    public Task<bool> ResetBackgroundImageList()
    {
        try
        {
            _backgroundImages.Clear();
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Task.FromResult(true);
        }
    }

    public Task<bool> SetupBackgroundImageList()
    {
        try
        {
            // Load data from the first file into the first collection
            LoadDataFromFile().Wait();
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Task.FromResult(true);
        }
    }
}