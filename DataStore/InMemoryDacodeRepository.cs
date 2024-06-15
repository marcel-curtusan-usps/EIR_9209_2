using EIR_9209_2.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;

public class InMemoryDacodeRepository : IInMemoryDacodeRepository
{
    private readonly ConcurrentDictionary<string, DesignationActivityToCraftType> _dacodeList = new();
    private readonly ILogger<InMemoryDacodeRepository> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFileService _fileService;
    private readonly string dacodeTypeFilePath = "";
    private readonly string fileName = "DesignationActivityToCraftType.json";

    public InMemoryDacodeRepository(ILogger<InMemoryDacodeRepository> logger, IConfiguration configuration, IFileService fileService)
    {
        _fileService = fileService;
        _logger = logger;
        _configuration = configuration;

        dacodeTypeFilePath = Path.Combine(Directory.GetCurrentDirectory(),
            _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"],
            $"{fileName}");
        // Load ConnectionType data from the first file into the first collection
        _ = LoadDataFromFile(dacodeTypeFilePath);
    }
    public DesignationActivityToCraftType? Add(DesignationActivityToCraftType dacode)
    {
        if (_dacodeList.TryAdd(dacode.DesignationActivity, dacode))
        {
            if (_fileService.WriteFileInAppConfig(fileName, JsonConvert.SerializeObject(_dacodeList.Values, Formatting.Indented)))
            {
                return dacode;
            }
            else
            {
                _logger.LogError($"DesignationActivityToCraftType.json was not update");
                return null;
            }
        }
        else
        {
            return null;
        }
    }
    public DesignationActivityToCraftType? Remove(string dacodeId)
    {
        if (_dacodeList.TryRemove(dacodeId, out DesignationActivityToCraftType dacode))
        {
            if (_fileService.WriteFileInAppConfig(fileName, JsonConvert.SerializeObject(_dacodeList.Values, Formatting.Indented)))
            {
                return dacode;
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
    public DesignationActivityToCraftType? Update(DesignationActivityToCraftType dacode)
    {
        if (_dacodeList.TryGetValue(dacode.DesignationActivity, out DesignationActivityToCraftType? currentDacode) && _dacodeList.TryUpdate(dacode.DesignationActivity, dacode, currentDacode))
        {
            if (_fileService.WriteFileInAppConfig(fileName, JsonConvert.SerializeObject(_dacodeList.Values, Formatting.Indented)))
            {
                return Get(dacode.DesignationActivity);
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
    public DesignationActivityToCraftType Get(string id)
    {
        _dacodeList.TryGetValue(id, out DesignationActivityToCraftType dacode);
        return dacode;
    }

    public IEnumerable<DesignationActivityToCraftType> GetAll()
    {
        return _dacodeList.Values;
    }

    private async Task LoadDataFromFile(string dacodeTypeFilePath)
    {
        try
        {
            // Read data from file
            var fileContent = await _fileService.ReadFile(dacodeTypeFilePath);

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