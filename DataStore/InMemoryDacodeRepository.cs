using EIR_9209_2.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;

public class InMemoryDacodeRepository : IInMemoryDacodeRepository
{
    private readonly ConcurrentDictionary<string, DesignationActivityToCraftType> _dacodeList = new();
    private readonly ILogger<InMemoryDacodeRepository> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFileService _fileService;
    private readonly string filePath = "";
    private readonly string fileName = "";

    public InMemoryDacodeRepository(ILogger<InMemoryDacodeRepository> logger, IConfiguration configuration, IFileService fileService)
    {
        _fileService = fileService;
        _logger = logger;
        _configuration = configuration;
        fileName = $"{_configuration[key: "InMemoryCollection:CollectionDACode"]}.json";
        filePath = Path.Combine(Directory.GetCurrentDirectory(), _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"], $"{fileName}");

        _ = LoadDataFromFile(filePath);
    }
    public async Task<DesignationActivityToCraftType?> Add(DesignationActivityToCraftType dacode)
    {
        bool saveToFile = false;
        try
        {
            if (_dacodeList.TryAdd(dacode.DesignationActivity, dacode))
            {
                saveToFile = true;
                return dacode;
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
                await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_dacodeList.Values, Formatting.Indented));
            }
        }
    }
    public async Task<DesignationActivityToCraftType?> Remove(string dacodeId)
    {
        bool saveToFile = false;
        try
        {
            if (_dacodeList.TryRemove(dacodeId, out DesignationActivityToCraftType dacode))
            {
                saveToFile = true;
                return dacode;
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
                await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_dacodeList.Values, Formatting.Indented));
            }
        }
    }
    public async Task<DesignationActivityToCraftType?> Update(DesignationActivityToCraftType dacode)
    {
        bool saveToFile = false;
        try
        {
            if (_dacodeList.TryGetValue(dacode.DesignationActivity, out DesignationActivityToCraftType? currentDacode) && _dacodeList.TryUpdate(dacode.DesignationActivity, dacode, currentDacode))
            {
                saveToFile = true;
                return currentDacode;
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
                await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_dacodeList.Values, Formatting.Indented));
            }
        }
    }
    public DesignationActivityToCraftType Get(string id)
    {
        _dacodeList.TryGetValue(id, out DesignationActivityToCraftType? dacode);
        return dacode;
    }

    public IEnumerable<DesignationActivityToCraftType> GetAll()
    {
        return _dacodeList.Values;
    }

    private async Task LoadDataFromFile(string filePath)
    {
        try
        {
            // Read data from file
            var fileContent = await _fileService.ReadFile(filePath);

            // Parse the file content to get the data. This depends on the format of your file.
            // Here's an example if your file was in JSON format and contained an array of T objects:
            List<DesignationActivityToCraftType> data = JsonConvert.DeserializeObject<List<DesignationActivityToCraftType>>(fileContent);

            // Insert the data into the MongoDB collection
            if (data.Count != 0)
            {
                foreach (DesignationActivityToCraftType item in data.Select(r => r).ToList())
                {
                    _dacodeList.TryAdd(item.DesignationActivity, item);
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