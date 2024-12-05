using EIR_9209_2.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;
namespace EIR_9209_2.DataStore
{
    public class InMemoryInventoryRepository : IInMemoryInventoryRepository
    {
        private readonly ConcurrentDictionary<string, Inventory> _inventoryList = new();
        private readonly ConcurrentDictionary<string, InventoryCategory> _inventoryCategoryList = new();
        private readonly ConcurrentDictionary<string, InventoryTracking> _inventoryTrackingList = new();
        private readonly ILogger<InMemoryInventoryRepository> _logger;
        private readonly IConfiguration _configuration;
        private readonly IFileService _fileService;
        private readonly string fileName = "Inventory.json";
        private readonly string categoryfileName = "InventoryCategoryCode.json";
        private readonly string trackingfileName = "InventoryTracking.json";
        public InMemoryInventoryRepository(ILogger<InMemoryInventoryRepository> logger, IConfiguration configuration, IFileService fileService)
        {
            _fileService = fileService;
            _logger = logger;
            _configuration = configuration;
            // Load inventory data from the first file into the first collection
            LoadDataFromFile().Wait();
            // Load inventory Category data from the first file into the first collection
            LoadCategoryCodeDataFromFile().Wait();
            // Load inventory Tracking data from the first file into the first collection
            LoadInventoryTrackingFromFile().Wait();
        }
        #region    
        private async Task LoadInventoryTrackingFromFile()
        {
            try
            {
                // Read data from file
                var fileContent = await _fileService.ReadFile(trackingfileName);
                if (!string.IsNullOrEmpty(fileContent))
                {
                    // Parse the file content to get the data. This depends on the format of your file.
                    // Here's an example if your file was in JSON format and contained an array of T objects:
                    List<InventoryTracking>? data = JsonConvert.DeserializeObject<List<InventoryTracking>>(fileContent);

                    // Insert the data into the MongoDB collection
                    if (data != null && data.Count != 0)
                    {
                        foreach (InventoryTracking item in data)
                        {
                            _inventoryTrackingList.TryAdd(item.Id, item);
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
        public async Task<List<InventoryTracking>> GetInventoryTrackingList()
        {
            //return all Inventory
            return _inventoryTrackingList.Select(y => y.Value).ToList();
        }
        public async Task<InventoryTracking?> AddTracking(InventoryTracking inventoryItem)
        {
            //add to email and also save to file
            bool saveToFile = false;
            try
            {
                if (_inventoryTrackingList.TryAdd(inventoryItem.Id, inventoryItem))
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
                    await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_inventoryTrackingList.Values, Formatting.Indented));
                }
            }
        }
        public async Task<InventoryTracking?> DeleteTracking(string id)
        {
            bool saveToFile = false;
            try
            {
                //delete from email and also save to file
                if (_inventoryTrackingList.TryRemove(id, out InventoryTracking inventoryItem))
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
                    await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_inventoryTrackingList.Values, Formatting.Indented));
                }
            }
        }
        public async Task<InventoryTracking?> UpdateTracking(InventoryTracking inventoryItem)
        {
            bool saveToFile = false;
            try
            {
                if (_inventoryTrackingList.TryGetValue(inventoryItem.Id, out InventoryTracking? currentInventoryItem) && _inventoryTrackingList.TryUpdate(inventoryItem.Id, inventoryItem, currentInventoryItem))
                {
                    saveToFile = true;
                    return currentInventoryItem;
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
                    await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_inventoryTrackingList.Values, Formatting.Indented));
                }
            }
        }
        #endregion

        #region
        /// <summary>
        /// Inventory
        /// </summary>
        /// <returns></returns>
        public async Task<List<Inventory>> GetInventoryList()
        {
            //return all Inventory
            return _inventoryList.Select(y => y.Value).ToList();
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
                if (_inventoryList.TryRemove(id, out Inventory inventoryItem))
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
        public async Task<Inventory?> Update(Inventory inventoryItem)
        {
            bool saveToFile = false;
            try
            {
                if (_inventoryList.TryGetValue(inventoryItem.Id, out Inventory? currentInventoryItem) && _inventoryList.TryUpdate(inventoryItem.Id, inventoryItem, currentInventoryItem))
                {
                    if (_inventoryList.TryGetValue(inventoryItem.Id, out Inventory inven))
                    {
                        saveToFile = true;
                        return inven;
                    }
                    return null;
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
        #endregion

        #region 
        /// <summary>
        /// InventoryCategory
        /// </summary>
        /// <returns></returns>
        public async Task<List<InventoryCategory>> GetInventoryCategoryList()
        {
            //return all emails
            return _inventoryCategoryList.Select(y => y.Value).ToList();
        }
        //public async Task<InventoryCategory> GetInventoryCategory(string code)
        //{
        //    //return one InventoryCategory
            
        //}
        public async Task<InventoryCategory?> AddCategory(InventoryCategory category)
        {
            //add to email and also save to file
            bool saveToFile = false;
            try
            {
                if (_inventoryCategoryList.TryAdd(category.Code, category))
                {
                    saveToFile = true;
                    return category;

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
                    await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_inventoryCategoryList.Values, Formatting.Indented));
                }
            }
        }
        public async Task<InventoryCategory?> DeleteCategory(string id)
        {
            bool saveToFile = false;
            try
            {
                //delete from email and also save to file
                if (_inventoryCategoryList.TryRemove(id, out InventoryCategory currentCategory))
                {
                    saveToFile = true;
                    return currentCategory;

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
                    await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_inventoryCategoryList.Values, Formatting.Indented));
                }
            }
        }
        public async Task<InventoryCategory?> UpdateCategory(InventoryCategory category)
        {
            bool saveToFile = false;
            try
            {
                if (_inventoryCategoryList.TryGetValue(category.Code, out InventoryCategory? currentCategory) && _inventoryCategoryList.TryUpdate(category.Code, category, currentCategory))
                {
                    saveToFile = true;
                    return currentCategory;
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
                    await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_inventoryCategoryList.Values, Formatting.Indented));
                }
            }
        }
        private async Task LoadCategoryCodeDataFromFile()
        {
            try
            {
                // Read data from file
                var fileContent = await _fileService.ReadFile(categoryfileName);
                if (!string.IsNullOrEmpty(fileContent))
                {
                    // Parse the file content to get the data. This depends on the format of your file.
                    // Here's an example if your file was in JSON format and contained an array of T objects:
                    List<InventoryCategory>? data = JsonConvert.DeserializeObject<List<InventoryCategory>>(fileContent);

                    // Insert the data into the MongoDB collection
                    if (data != null && data.Count != 0)
                    {
                        foreach (InventoryCategory item in data)
                        {
                            _inventoryCategoryList.TryAdd(item.Code, item);
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
        #endregion
        public Task<bool> ResetInventoryList()
        {
            try
            {
                _inventoryList.Clear();
                _inventoryCategoryList.Clear();
                _inventoryTrackingList.Clear();
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Task.FromResult(true);
            }
        }

        public Task<bool> SetupInventoryList()
        {
            try
            {
                // Load inventory data from the first file into the first collection
                LoadDataFromFile().Wait();
                // Load inventory Category data from the first file into the first collection
                LoadCategoryCodeDataFromFile().Wait();
                // Load inventory Tracking data from the first file into the first collection
                LoadInventoryTrackingFromFile().Wait();

                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Task.FromResult(true);
            }
        }
    }
}