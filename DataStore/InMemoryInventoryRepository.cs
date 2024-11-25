using EIR_9209_2.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;

public class InMemoryInventoryRepository : IInMemoryInventoryRepository
{
    private readonly ConcurrentDictionary<string, Inventory> _inventoryList = new();
    private readonly ILogger<InMemoryInventoryRepository> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFileService _fileService;
    private readonly string fileName = "Inventory.json";
    public InMemoryInventoryRepository(ILogger<InMemoryInventoryRepository> logger, IConfiguration configuration, IFileService fileService)
    {
        _fileService = fileService;
        _logger = logger;
        _configuration = configuration;
        // Load Connection data from the first file into the first collection
       LoadDataFromFile().Wait();
    }

    public async Task<Inventory?> Add(Inventory inventoryItem)
    {
        //add to email and also save to file
        bool saveToFile = false;
        try
        {
            if (_inventoryList.TryAdd(inventoryItem.Id, inventoryItem))
            {
                saveToFile = true;
                return inventoryItem;

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
                await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_inventoryList.Values, Formatting.Indented));
            }
        }
    }

    public async Task<Inventory?> Delete(string id)
    {
        bool saveToFile = false;
        try
        {
            //delete from email and also save to file
            if (_inventoryList.TryRemove(id, out Inventory currentEmail))
            {
                saveToFile = true;
                return currentEmail;

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
                await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_inventoryList.Values, Formatting.Indented));
            }
        }
    }
    public async Task<Inventory?> Update(string id, Inventory email)
    {
        bool saveToFile = false;
        try
        {
            if (_inventoryList.TryGetValue(id, out Inventory? currentEmail) && _inventoryList.TryUpdate(id, email, currentEmail))
            {
                saveToFile = true;
                return currentEmail;
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
                await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_inventoryList.Values, Formatting.Indented));
            }
        }
    }

    public IEnumerable<Inventory> GetAll()
    {
        //return all emails
        return _inventoryList.Values;
    }
    /// <summary>
    /// Updates a email in the in-memory email repository.
    /// </summary>
    private async Task LoadDataFromFile()
    {
        try
        {
            // Read data from file
            var fileContent = await _fileService.ReadFile(fileName);
            if (!string.IsNullOrEmpty(fileContent))
            {
                // Parse the file content to get the data. This depends on the format of your file.
                // Here's an example if your file was in JSON format and contained an array of T objects:
                List<Inventory>? data = JsonConvert.DeserializeObject<List<Inventory>>(fileContent);

                // Insert the data into the MongoDB collection
                if (data != null && data.Count != 0)
                {
                    foreach (Inventory item in data)
                    {
                        _inventoryList.TryAdd(item.Id, item);
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

    public Task<bool> ResetEmailsList()
    {
        try
        {
            _inventoryList.Clear();
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Task.FromResult(true);
        }
    }

    public Task<bool> SetupEmailsList()
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