using EIR_9209_2.Models;
using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Ocsp;
using System.Collections.Concurrent;
using System.Transactions;

namespace EIR_9209_2.InMemory
{
    public class InMemoryTagsRepository : IInMemoryTagsRepository
    {
        private readonly ConcurrentDictionary<string, GeoMarker> _tagList = new();
        private readonly ConcurrentDictionary<string, DesignationActivityToCraftType> _designationActivityToCraftType = new();
        private readonly ILogger<InMemoryTagsRepository> _logger;
        private readonly IConfiguration _configuration;
        private readonly IFileService FileService;
        public InMemoryTagsRepository(ILogger<InMemoryTagsRepository> logger, IConfiguration configuration, IFileService fileService)
        {
            FileService = fileService;
            _logger = logger;
            _configuration = configuration;
            string DACodeandCraftTypeFilePath = Path.Combine(Directory.GetCurrentDirectory(), _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"], $"{"DesignationActivityToCraftType"}.json");
            // Load ConnectionType data from the first file into the first collection
            _ = LoadDesignationActivityToCraftTypeDataFromFile(DACodeandCraftTypeFilePath);

            string BuildPath = Path.Combine(_configuration[key: "ApplicationConfiguration:BaseDrive"],
                _configuration[key: "ApplicationConfiguration:BaseDirectory"],
                _configuration[key: "SiteIdentity:NassCode"],
                _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"],
                $"{_configuration[key: "InMemoryCollection:CollectionTags"]}.json");
            // Load data from the first file into the first collection
            _ = LoadDataFromFile(BuildPath);


        }



        public void Add(GeoMarker tag)
        {
            _tagList.TryAdd(tag.Properties.Id, tag);
        }
        public void LocalAdd(GeoMarker tag)
        {
            _tagList.TryAdd(tag.Properties.Id, tag);
        }

        public void Remove(string connectionId)
        {
            _tagList.TryRemove(connectionId, out _);
        }

        public object Get(string id)
        {
            if (_tagList.ContainsKey(id) && _tagList.TryGetValue(id, out GeoMarker tag))
            {
                return tag;
            }
            else
            {
                return new JObject { ["Message"] = "Tag not Found" };
            }
        }
        public List<GeoMarker> GetAll()
        {
            return _tagList.Values.Where(r => r.Properties.posAge > 1 && r.Properties.posAge < 100000 && r.Properties.Visible).Select(y => y).ToList();
        }

        public void Update(GeoMarker tag)
        {
            if (_tagList.TryGetValue(tag.Properties.Id, out GeoMarker currentTag))
            {
                _tagList.TryUpdate(tag.Properties.Id, tag, currentTag);
            }
        }
        //area Dwell

        private async Task LoadDataFromFile(string filePath)
        {
            try
            {
                // Read data from file
                var fileContent = await FileService.ReadFile(filePath);

                // Parse the file content to get the data. This depends on the format of your file.
                // Here's an example if your file was in JSON format and contained an array of T objects:
                List<GeoMarker> data = JsonConvert.DeserializeObject<List<GeoMarker>>(fileContent);

                // Insert the data into the MongoDB collection
                if (data.Count != 0)
                {
                    foreach (GeoMarker item in data.Select(r => r).ToList())
                    {
                        item.Properties.CraftName = GetCraftName(item.Properties.DesignationActivity);
                        _tagList.TryAdd(item.Properties.Id, item);
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
        //dac
        private async Task LoadDesignationActivityToCraftTypeDataFromFile(string filePath)
        {
            try
            {
                // Read data from file
                var fileContent = await FileService.ReadFile(filePath);

                // Parse the file content to get the data. This depends on the format of your file.
                // Here's an example if your file was in JSON format and contained an array of T objects:
                List<DesignationActivityToCraftType> data = JsonConvert.DeserializeObject<List<DesignationActivityToCraftType>>(fileContent);

                // Insert the data into the MongoDB collection
                if (data.Count != 0)
                {
                    foreach (DesignationActivityToCraftType item in data.Select(r => r).ToList())
                    {
                        _designationActivityToCraftType.TryAdd(item.DesignationActivity, item);
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

        public void UpdateEmployeeInfo(JObject empData)
        {
            bool savetoFile = false;
            try
            {
                GeoMarker? TagData = null;
                if (empData.ContainsKey("tagId") && !string.IsNullOrEmpty(empData["tagId"].ToString()))
                {
                    _tagList.TryGetValue(empData["tagId"].ToString(), out TagData);

                }
                else if (empData.ContainsKey("ein") && !string.IsNullOrEmpty(empData["ein"].ToString()))
                {
                    TagData = _tagList.Where(r => r.Value.Properties.EIN == empData["ein"].ToString()).Select(y => y.Value).FirstOrDefault();
                }

                if (TagData != null)
                {
                    //check if tag type is not null and update the tag type

                    if (TagData.Properties.TagType != "Person")
                    {
                        TagData.Properties.TagType = "Person";
                        savetoFile = true;
                    }

                    //check EIN value is not null and update the EIN value
                    if (empData.ContainsKey("ein"))
                    {
                        if (TagData.Properties.EIN != empData["ein"].ToString())
                        {
                            TagData.Properties.EIN = empData["ein"].ToString();

                            savetoFile = true;
                        }
                    }
                    //check FirstName value is not null and update the FirstName value
                    if (empData.ContainsKey("firstName"))
                    {
                        if (TagData.Properties.EmpFirstName != empData["firstName"].ToString())
                        {
                            TagData.Properties.EmpFirstName = empData["firstName"].ToString();
                            savetoFile = true;
                        }
                    }
                    //check LastName value is not null and update the LastName value
                    if (empData.ContainsKey("lastName"))
                    {
                        if (TagData.Properties.EmpLastName != empData["lastName"].ToString())
                        {
                            TagData.Properties.EmpLastName = empData["lastName"].ToString();
                            savetoFile = true;
                        }
                    }
                    //check title value is not null and update the title value
                    if (empData.ContainsKey("title"))
                    {
                        if (TagData.Properties.Title != empData["title"].ToString())
                        {
                            TagData.Properties.Title = empData["title"].ToString();
                            savetoFile = true;
                        }
                    }
                    //check designationActivity value is not null and update the designationActivity value
                    if (empData.ContainsKey("designationActivity"))
                    {
                        if (TagData.Properties.DesignationActivity != empData["designationActivity"].ToString())
                        {
                            TagData.Properties.DesignationActivity = empData["designationActivity"].ToString();
                            TagData.Properties.CraftName = GetCraftName(empData["designationActivity"].ToString());
                            savetoFile = true;
                        }

                    }
                    //check paylocation value is not null and update the paylocation value
                    if (empData.ContainsKey("paylocation"))
                    {
                        if (TagData.Properties.EmpPayLocation != empData["paylocation"].ToString())
                        {
                            TagData.Properties.EmpPayLocation = empData["paylocation"].ToString();
                            savetoFile = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
            finally
            {
                if (savetoFile)
                {
                    //save date to local file
                    string BuildPath = Path.Combine(_configuration[key: "ApplicationConfiguration:BaseDrive"],
                                       _configuration[key: "ApplicationConfiguration:BaseDirectory"],
                                                      _configuration[key: "SiteIdentity:NassCode"],
                                                                     _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"],
                                                                                    $"{_configuration[key: "InMemoryCollection:CollectionTags"]}.json");
                    FileService.WriteFile(BuildPath, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
                }
            }
        }

        private string GetCraftName(string dac)
        {
            try
            {
                if (_designationActivityToCraftType.ContainsKey(dac) && _designationActivityToCraftType.TryGetValue(dac, out DesignationActivityToCraftType Current))
                {
                    return Current.CraftType;
                }
                else
                {
                    return "NA";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void UpdateBadgeTransactionScan(JObject transaction)
        {
            bool savetoFile = false;
            try
            {
                GeoMarker? TagData = null;
                if (transaction.ContainsKey("tagId") && !string.IsNullOrEmpty(transaction["tagId"].ToString()))
                {
                    _tagList.TryGetValue(transaction["tagId"].ToString(), out TagData);

                }
                else if (transaction.ContainsKey("ein") && !string.IsNullOrEmpty(transaction["ein"].ToString()))
                {
                    TagData = _tagList.Where(r => r.Value.Properties.EIN == transaction["ein"].ToString()).Select(y => y.Value).FirstOrDefault();
                }
                if (TagData != null)
                {
                    TagData.Properties.BadgeScan.Add(new ScanTransaction
                    {

                        controllerCaption = transaction["controllerCaption"].ToString(),
                        deviceCaption = transaction["deviceCaption"].ToString(),
                        scanDateTime = (DateTime)transaction["transactionOriginDateTime"],
                        deviceTypeCaption = transaction["deviceTypeCaption"].ToString(),
                        deviceID = transaction["deviceID"].ToString()
                    });
                    savetoFile = true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
            finally
            {
                if (savetoFile)
                {
                    //save date to loacl file
                    string BuildPath = Path.Combine(_configuration[key: "ApplicationConfiguration:BaseDrive"],
                                       _configuration[key: "ApplicationConfiguration:BaseDirectory"],
                                                      _configuration[key: "SiteIdentity:NassCode"],
                                                                     _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"],
                                                                                    $"{_configuration[key: "InMemoryCollection:CollectionTags"]}.json");
                    FileService.WriteFile(BuildPath, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
                }
            }
        }

        public string GetCraftType(string tagId)
        {
            return _tagList.Where(r => r.Key == tagId).Select(y => y.Value.Properties.CraftName).FirstOrDefault();
        }
    }
}
