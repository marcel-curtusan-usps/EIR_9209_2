using EIR_9209_2.Models;
using Humanizer;
using Microsoft.Build.Execution;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Ocsp;
using System.Collections.Concurrent;
using System.Data;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Transactions;
using static EIR_9209_2.Models.GeoMarker;

namespace EIR_9209_2.InMemory
{
    public class InMemoryTagsRepository : IInMemoryTagsRepository
    {
        private static readonly ConcurrentDictionary<string, GeoMarker> _tagList = new();
        private readonly ILogger<InMemoryTagsRepository> _logger;
        private readonly IConfiguration _configuration;
        private readonly IFileService FileService;
        private readonly IInMemoryDacodeRepository _dacode;
        private readonly string filePath = "";
        private readonly string fileName = "";
        public InMemoryTagsRepository(ILogger<InMemoryTagsRepository> logger, IConfiguration configuration, IInMemoryDacodeRepository dacode, IFileService fileService)
        {
            FileService = fileService;
            _logger = logger;
            _configuration = configuration;
            _dacode = dacode;
            fileName = $"{_configuration[key: "InMemoryCollection:CollectionTags"]}.json";
            filePath = Path.Combine(_configuration[key: "ApplicationConfiguration:BaseDrive"],
                _configuration[key: "ApplicationConfiguration:BaseDirectory"],
                _configuration[key: "SiteIdentity:NassCode"],
                _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"],
                $"{fileName}");
            // Load data from the first file into the first collection
            _ = LoadDataFromFile(filePath);

        }

        public void Add(GeoMarker tag)
        {
            if (_tagList.TryAdd(tag.Properties.Id, tag))
            {
                FileService.WriteFile(fileName, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
            }
        }
        public void LocalAdd(GeoMarker tag)
        {
            _tagList.TryAdd(tag.Properties.Id, tag);
        }

        public void Remove(string connectionId)
        {
            if (_tagList.TryRemove(connectionId, out _))
            {
                FileService.WriteFile(fileName, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
            }

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
        public List<GeoMarker> GetAllPerson()
        {
            return _tagList.Values.Where(r => r.Properties.posAge > 1 && r.Properties.posAge < 100000 && r.Properties.Visible && r.Properties.TagType == "Person").Select(y => y).ToList();
        }
        public List<GeoMarker> GetAllPIV()
        {
            return _tagList.Values.Where(r => r.Properties.TagType.StartsWith("Vehicle")).Select(y => y).ToList();
        }
        public List<GeoMarker> GetAllAGV()
        {
            return _tagList.Values.Where(r => r.Properties.TagType.StartsWith("Autonomous")).Select(y => y).ToList();
        }

        public void Update(GeoMarker tag)
        {
            if (_tagList.TryGetValue(tag.Properties.Id, out GeoMarker currentTag) && _tagList.TryUpdate(tag.Properties.Id, tag, currentTag))
            {
                FileService.WriteFile(fileName, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
            }
        }
        //area Dwell

        public void UpdateEmployeeInfo(JToken result)
        {
            bool savetoFile = false;
            bool DesignationActivitysavetoFile = false;
            try
            {
                foreach (JObject empData in result.OfType<JObject>())
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

                    if (TagData != null && _tagList.TryGetValue(TagData.Properties.Id, out GeoMarker currentTag))
                    {
                        //check if tag type is not null and update the tag type

                        currentTag.Properties.TagType = "Person";
                        savetoFile = true;


                        //check EIN value is not null and update the EIN value
                        if (empData.ContainsKey("ein"))
                        {
                            if (!string.IsNullOrEmpty(empData["ein"].ToString()) && currentTag.Properties.EIN != empData["ein"].ToString())
                            {
                                currentTag.Properties.EIN = empData["ein"].ToString();

                                savetoFile = true;
                            }
                        }
                        //check FirstName value is not null and update the FirstName value
                        if (empData.ContainsKey("firstName"))
                        {
                            if (!string.IsNullOrEmpty(empData["firstName"].ToString()) && currentTag.Properties.EmpFirstName != empData["firstName"].ToString())
                            {
                                currentTag.Properties.EmpFirstName = empData["firstName"].ToString();
                                savetoFile = true;
                            }
                        }
                        //check LastName value is not null and update the LastName value
                        if (empData.ContainsKey("lastName"))
                        {
                            if (!string.IsNullOrEmpty(empData["lastName"].ToString()) && currentTag.Properties.EmpLastName != empData["lastName"].ToString())
                            {
                                currentTag.Properties.EmpLastName = empData["lastName"].ToString();
                                savetoFile = true;
                            }
                        }
                        //check title value is not null and update the title value
                        if (empData.ContainsKey("title"))
                        {
                            if (!string.IsNullOrEmpty(empData["title"].ToString()) && currentTag.Properties.Title != empData["title"].ToString())
                            {
                                currentTag.Properties.Title = empData["title"].ToString();
                                savetoFile = true;
                            }
                        }
                        //check encodedId value is not null and update the title value
                        if (empData.ContainsKey("encodedId"))
                        {
                            if (!string.IsNullOrEmpty(empData["encodedId"].ToString()) && currentTag.Properties.EncodedId != empData["encodedId"].ToString())
                            {
                                currentTag.Properties.EncodedId = empData["encodedId"].ToString();
                                savetoFile = true;
                            }
                        }

                        //check designationActivity value is not null and update the designationActivity value
                        if (empData.ContainsKey("designationActivity"))
                        {
                            if (!string.IsNullOrEmpty(empData["designationActivity"].ToString()) && currentTag.Properties.DesignationActivity != empData["designationActivity"].ToString())
                            {
                                var daCode = _dacode.Get(empData["designationActivity"].ToString());
                                if (daCode != null)
                                {
                                    currentTag.Properties.DesignationActivity = daCode.DesignationActivity;
                                    currentTag.Properties.CraftName = daCode.CraftType;

                                }

                                savetoFile = true;
                            }


                        }
                        //check paylocation value is not null and update the paylocation value
                        if (empData.ContainsKey("paylocation"))
                        {
                            if (!string.IsNullOrEmpty(empData["paylocation"].ToString()) && currentTag.Properties.EmpPayLocation != empData["paylocation"].ToString())
                            {
                                currentTag.Properties.EmpPayLocation = empData["paylocation"].ToString();
                                savetoFile = true;
                            }
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
                    FileService.WriteFile(fileName, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
                }
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
                List<GeoMarker> data = JsonConvert.DeserializeObject<List<GeoMarker>>(fileContent);

                // Insert the data into the MongoDB collection
                if (data.Count != 0)
                {
                    foreach (GeoMarker item in data.Select(r => r).ToList())
                    {
                        item.Properties.Visible = false;
                        item.Properties.LocationMovementStatus = "noData";
                        item.Properties.isPosition = false;
                        item.Properties.posAge = 0;
                        item.Properties.Zones = [];
                        item.Properties.ZonesNames = "";
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
        public bool UpdateTagDesignationActivity(DesignationActivityToCraftType daCode)
        {
            //update DasigbationActivity to CraftType
            bool savetoFile = false;
            try
            {
                foreach (var item in _tagList.Values)
                {
                    if (item.Properties.DesignationActivity == daCode.DesignationActivity)
                    {
                        item.Properties.CraftName = daCode.CraftType;
                        savetoFile = true;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return false;
            }
            finally
            {
                if (savetoFile)
                {
                    //save date to local file
                    FileService.WriteFile(fileName, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
                }
            }
        }
        public void UpdateTagQPEInfo(List<Tags> tag)
        {
            bool savetoFile = false;
            try
            {
                foreach (Tags qtitem in tag)
                {
                    GeoMarker? TagData = null;
                    _tagList.TryGetValue(qtitem.TagId, out TagData);
                    if (TagData != null)
                    {
                        TagData.Properties.Color = qtitem.Color;
                        TagData.Properties.Zones = qtitem.LocationZoneIds;

                        TagData.Properties.LocationMovementStatus = qtitem.LocationMovementStatus;
                        if (!string.IsNullOrEmpty(qtitem.TagName))
                        {

                        }
                    }
                    else
                    {
                        //add tag to taglist
                        GeoMarker tagData = new GeoMarker
                        {
                            Geometry = new MarkerGeometry
                            {
                                Coordinates = [qtitem.Location[0], qtitem.Location[1]],
                                Type = "Point"
                            },
                            Properties = new Marker
                            {
                                Id = qtitem.TagId,
                                Name = qtitem.TagName,
                                Color = qtitem.Color,
                                Zones = qtitem.LocationZoneIds,
                                LocationMovementStatus = qtitem.LocationMovementStatus
                            }
                        };
                        _tagList.TryAdd(qtitem.TagId, tagData);
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
                    FileService.WriteFile(fileName, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
                }

            }
        }

        public object UpdateTagInfo(JObject tagInfo)
        {
            //update DasigbationActivity to CraftType
            bool savetoFile = false;
            try
            {
                if (tagInfo.ContainsKey("tagId"))
                {
                    //find tag id and update proprieties
                    GeoMarker? TagData = null;
                    _tagList.TryGetValue(tagInfo["tagId"].ToString(), out TagData);
                    if (TagData != null)
                    {
                        if (tagInfo.ContainsKey("ein"))
                        {
                            TagData.Properties.EIN = tagInfo["ein"].ToString();
                            savetoFile = true;
                        }
                        if (tagInfo.ContainsKey("empFirstName"))
                        {
                            TagData.Properties.EmpFirstName = tagInfo["empFirstName"].ToString();
                            savetoFile = true;
                        }
                        if (tagInfo.ContainsKey("empLastName"))
                        {
                            TagData.Properties.EmpLastName = tagInfo["empLastName"].ToString();
                            savetoFile = true;
                        }
                        if (tagInfo.ContainsKey("encodedId"))
                        {
                            TagData.Properties.EncodedId = tagInfo["encodedId"].ToString();
                            savetoFile = true;
                        }
                        if (tagInfo.ContainsKey("name"))
                        {
                            TagData.Properties.Name = tagInfo["name"].ToString();
                            savetoFile = true;
                        }
                        if (tagInfo.ContainsKey("empPayLocation"))
                        {
                            TagData.Properties.EmpPayLocation = tagInfo["empPayLocation"].ToString();
                            savetoFile = true;
                        }
                        if (tagInfo.ContainsKey("tagType"))
                        {
                            TagData.Properties.TagType = tagInfo["tagType"].ToString();
                            savetoFile = true;
                        }
                        if (tagInfo.ContainsKey("designationActivity"))
                        {
                            var daCode = _dacode.Get(tagInfo["designationActivity"].ToString());
                            if (daCode != null)
                            {
                                TagData.Properties.DesignationActivity = daCode.DesignationActivity;
                                TagData.Properties.CraftName = daCode.CraftType;
                                savetoFile = true;
                            }
                        }
                        return TagData;
                    }
                    else
                    {
                        return new JObject { ["Message"] = $"Tag: {tagInfo} not Found" };
                    }
                }
                else
                {
                    return new JObject { ["Message"] = $"TagId Parameters missing" };
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new JObject { ["Error"] = $"{e.Message}" };
            }
            finally
            {
                if (savetoFile)
                {
                    //save date to local file
                    FileService.WriteFile(fileName, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
                }
            }
        }
        public List<Marker> SearchTag(string searchValue)
        {
            return _tagList.Where(sl =>
                Regex.IsMatch(sl.Value.Properties.Id, "(" + searchValue + ")", RegexOptions.IgnoreCase)
                || Regex.IsMatch(sl.Value.Properties.EIN, "(" + searchValue + ")", RegexOptions.IgnoreCase)
                || Regex.IsMatch(sl.Value.Properties.EncodedId, "(" + searchValue + ")", RegexOptions.IgnoreCase)
                || Regex.IsMatch(sl.Value.Properties.CraftName, "(" + searchValue + ")", RegexOptions.IgnoreCase)
                || Regex.IsMatch(sl.Value.Properties.EmpFirstName, "(" + searchValue + ")", RegexOptions.IgnoreCase)
                || Regex.IsMatch(sl.Value.Properties.EmpLastName, "(" + searchValue + ")", RegexOptions.IgnoreCase)
              ).Select(r => r.Value.Properties).ToList();
        }
    }
}
