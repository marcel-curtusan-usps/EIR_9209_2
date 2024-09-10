using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using static EIR_9209_2.DataStore.InMemoryCamerasRepository;
using static EIR_9209_2.Models.GeoMarker;
using static EIR_9209_2.Models.VehicleGeoMarker;

namespace EIR_9209_2.DataStore
{
    public class InMemoryTagsRepository : IInMemoryTagsRepository
    {
        private readonly ConcurrentDictionary<string, GeoMarker> _tagList = new();
        private readonly ConcurrentDictionary<string, VehicleGeoMarker> _vehicleTagList = new();
        private readonly ConcurrentDictionary<DateTime, List<TagTimeline>> _QRETagTimelineResults = new();
        private readonly ILogger<InMemoryTagsRepository> _logger;
        protected readonly IHubContext<HubServices> _hubServices;
        private readonly IConfiguration _configuration;
        private readonly IFileService _fileService;
        private readonly IInMemoryDacodeRepository _dacode;
        private readonly string filePath = "";
        private readonly string fileName = "Tags.json";
        private readonly string fileTagVehiclePath = "";
        private readonly string vehicleFileName = "VehicelTags.json";
        private readonly string fileTagTimelinePath = "";
        private readonly string fileTagTimeline = "TagTimeline.json";
        public InMemoryTagsRepository(ILogger<InMemoryTagsRepository> logger, IHubContext<HubServices> hubServices, IConfiguration configuration, IInMemoryDacodeRepository dacode, IFileService fileService)
        {
            _fileService = fileService;
            _logger = logger;
            _configuration = configuration;
            _hubServices = hubServices;
            _dacode = dacode;
            filePath = Path.Combine(_configuration[key: "ApplicationConfiguration:BaseDrive"],
                _configuration[key: "ApplicationConfiguration:BaseDirectory"],
                _configuration[key: "ApplicationConfiguration:NassCode"],
                _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"],
                $"{fileName}");
            // Load data from the first file into the first collection
            _ = LoadDataFromFile(filePath);

            fileTagTimelinePath = Path.Combine(_configuration[key: "ApplicationConfiguration:BaseDrive"],
                _configuration[key: "ApplicationConfiguration:BaseDirectory"],
                _configuration[key: "ApplicationConfiguration:NassCode"],
                _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"],
                $"{fileTagTimeline}");
            _ = LoadTagTimelineFromFile(fileTagTimelinePath);

            fileTagVehiclePath = Path.Combine(_configuration[key: "ApplicationConfiguration:BaseDrive"],
              _configuration[key: "ApplicationConfiguration:BaseDirectory"],
              _configuration[key: "ApplicationConfiguration:NassCode"],
              _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"],
              $"{vehicleFileName}");
            _ = LoadTagVehicleFromFile(fileTagVehiclePath);
        }

  

        public async Task Add(GeoMarker tag)
        {
            bool saveToFile = false;
            try
            {
                if (_tagList.TryAdd(tag.Properties.Id, tag))
                {
                    await _hubServices.Clients.Group(tag.Properties.TagType).SendAsync($"add{tag.Properties.TagType}TagInfo", tag);
                    saveToFile = true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
            finally
            {
                if (saveToFile)
                {
                    await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
                }
            }
        }
        public async Task Delete(string connectionId)
        {
            bool saveToFile = false;
            try
            {
                if (_tagList.TryRemove(connectionId, out GeoMarker? tag))
                {
                    await _hubServices.Clients.Group(tag.Properties.TagType).SendAsync($"delete{tag.Properties.TagType}TagInfo", tag);
                    saveToFile = true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
            finally
            {
                if (saveToFile)
                {
                    await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
                }
            }

        }

        public object Get(string id)
        {
            if (_tagList.ContainsKey(id) && _tagList.TryGetValue(id, out GeoMarker tag))
            {
                return tag;
            }
            else if (_vehicleTagList.ContainsKey(id) && _vehicleTagList.TryGetValue(id, out VehicleGeoMarker vtag))
            {
                return vtag;
            }
            else
            {
                return new JObject { ["Message"] = "Tag not Found" };
            }
        }
        public List<string> GetTagByType(string tagType)
        {
            return _tagList.Values.Where(r => r.Properties.TagType == tagType).Select(y => y.Properties.Name).ToList();
        }
        public List<GeoMarker> GetAll()
        {
            return _tagList.Values.Where(r => r.Properties.Visible).Select(y => y).ToList();
        }
        public List<GeoMarker> GetTagsType(string type)
        {
            return _tagList.Values.Where(r => r.Properties.TagType == type).Select(y => y).ToList();
        }
        public List<VehicleGeoMarker> GetAllPIV()
        {
            return _vehicleTagList.Values.Where(r => r.Properties.Type.StartsWith("Vehicle")).Select(y => y).ToList();
        }
        public List<VehicleGeoMarker> GetAllAGV()
        {
            return _vehicleTagList.Values.Where(r => r.Properties.Type.StartsWith("Autonomous")).Select(y => y).ToList();
        }
        public async void UpdateEmployeeInfo(JToken result)
        {
            bool savetoFile = false;

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

                        currentTag.Properties.TagType = "Badge";
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
                            if (!Regex.IsMatch(currentTag.Properties.EmpFirstName, $"^{Regex.Escape(empData["firstName"].ToString())}$", RegexOptions.IgnoreCase))
                            {
                                currentTag.Properties.EmpFirstName = empData["firstName"].ToString();
                                savetoFile = true;
                            }
                        }
                        //check LastName value is not null and update the LastName value
                        if (empData.ContainsKey("lastName"))
                        {
                            if (!Regex.IsMatch(currentTag.Properties.EmpLastName, $"^{Regex.Escape(empData["lastName"].ToString())}$", RegexOptions.IgnoreCase))
                            {
                                currentTag.Properties.EmpLastName = empData["lastName"].ToString();
                                savetoFile = true;
                            }
                        }
                        //check title value is not null and update the title value
                        if (empData.ContainsKey("title"))
                        {
                            if (!Regex.IsMatch(currentTag.Properties.Title, $"^{Regex.Escape(empData["title"].ToString())}$", RegexOptions.IgnoreCase))
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
                                if (string.IsNullOrEmpty(currentTag.Properties.DesignationActivity))
                                {
                                    currentTag.Properties.DesignationActivity = empData["designationActivity"].ToString();
                                    savetoFile = true;
                                }

                                var daCode = _dacode.Get(empData["designationActivity"].ToString());
                                if (daCode != null)
                                {
                                    currentTag.Properties.CraftName = daCode.CraftType;
                                    savetoFile = true;
                                }


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
                    await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
                }
            }
        }
        private async Task LoadTagTimelineFromFile(string filePath)
        {
            try
            {
                //// Read data from file
                //var fileContent = await FileService.ReadFile(filePath);
                //var data = JsonConvert.DeserializeObject<List<TagTimeline>[]>(fileContent);

                //if (data!.Length > 0)
                //{
                //    DateTime Max = DateTime.MinValue;
                //    foreach (List<TagTimeline> curdata in data)
                //    {
                //        DateTime hour = curdata!.Select(i => i.Hour).FirstOrDefault();
                //        if (curdata!.Count > 0)
                //        {
                //            if (hour > Max)
                //            {
                //                Max = hour;
                //            }
                //            _QRETagTimelineResults.TryAdd(hour, curdata);
                //        }
                //    }
                //    //Remove last hour in case not complete hour data
                //    _QRETagTimelineResults.TryRemove(Max, out var remove);
                //}

            }
            catch (FileNotFoundException ex)
            {
                // Handle the FileNotFoundException here
                _logger.LogError($"File not found: {ex.Message}");
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
        private async Task LoadTagVehicleFromFile(string filePath)
        {
            try
            {
                // Read data from file
                var fileContent = await _fileService.ReadFile(filePath);
                List<VehicleGeoMarker> data = JsonConvert.DeserializeObject<List<VehicleGeoMarker>>(fileContent);

                if (data.Count > 0)
                {
                    foreach (VehicleGeoMarker curdata in data.Select(r => r).ToList())
                    {
                        curdata.Properties.Visible = true;
                        curdata.Properties.isPosition = false;
                        _vehicleTagList.TryAdd(curdata.Properties.Id, curdata);

                    }
                }

            }
            catch (FileNotFoundException ex)
            {
                // Handle the FileNotFoundException here
                _logger.LogError($"File not found: {ex.Message}");
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
        private async Task LoadDataFromFile(string filePath)
        {
            try
            {
                // Read data from file
                var fileContent = await _fileService.ReadFile(filePath);

                // Parse the file content to get the data. This depends on the format of your file.
                // Here's an example if your file was in JSON format and contained an array of T objects:
                List<GeoMarker> data = JsonConvert.DeserializeObject<List<GeoMarker>>(fileContent);

                // Insert the data into the MongoDB collection
                if (data?.Count != 0)
                {
                    foreach (GeoMarker item in data.Select(r => r).ToList())
                    {
                        item.Properties.Visible = false;
                        item.Properties.LocationMovementStatus = "noData";
                        item.Properties.isPosition = false;
                        item.Properties.posAge = 0;
                        item.Properties.Zones = [];
                        item.Properties.ZonesNames = "";
                        if (string.IsNullOrEmpty(item.Properties.TagType) || item.Properties.TagType == "Person")
                        {
                            item.Properties.TagType = "Badge";
                        }

                        _tagList.TryAdd(item.Properties.Id, item);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                // Handle the FileNotFoundException here
                _logger.LogError($"File not found: {ex.Message}");
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
        public bool ExistingTagTimeline(DateTime hour)
        {
            return _QRETagTimelineResults.ContainsKey(hour);

        }
        public List<TagTimeline> GetTagTimeline(DateTime hour)
        {
            return _QRETagTimelineResults[hour];
        }
        public void UpdateTagTimeline(DateTime hour, List<TagTimeline> newValue, List<TagTimeline> currentValue)
        {
            bool savetoFile = false;
            try
            {
                if (_QRETagTimelineResults.TryUpdate(hour, newValue, currentValue))
                {
                    savetoFile = true;
                }
               
            }
            catch (Exception e)
            {
                _logger.LogError($"Error updating TagTimeLine {e.Message}");
            }
            finally
            {
                //if (savetoFile)
                //{
                //    FileService.WriteFile(fileTagTimeline, JsonConvert.SerializeObject(_QRETagTimelineResults.Values, Formatting.Indented));
                //}
            }
        }

        public void AddTagTimeline(DateTime hour, List<TagTimeline> newValue)
        {
            bool savetoFile = false;
            try
            {

                if (_QRETagTimelineResults.TryAdd(hour, newValue))
                {
                    savetoFile = true;
                }
                
            }
            catch (Exception e)
            {
                _logger.LogError($"Error adding TagTimeLine {e.Message}");
            }
            finally
            {
                //if (savetoFile)
                //{
                //    FileService.WriteFile(fileTagTimeline, JsonConvert.SerializeObject(_QRETagTimelineResults.Values, Formatting.Indented));
                //}
            }
        }
        public void RemoveTagTimeline(DateTime hour)
        {
            _QRETagTimelineResults.Where(r => r.Key < hour).Select(l => l.Key).ToList().ForEach(key =>
            {
                _QRETagTimelineResults.TryRemove(key, out var remove);
            });
        }
        //public List<TagTimeline> GetTagTimelineList(string EIN)
        //{
        //    List<TagTimeline> tagTimeline = new List<TagTimeline>();
        //    _QRETagTimelineResults.Where(r => r.Key >= DateTime.Now.AddDays(-7)).Select(l => l.Value).ToList().ForEach(value =>
        //    //_QRETagTimelineResults.Select(l => l.Value).ToList().ForEach(value =>
        //    {
        //        foreach (TagTimeline timeline in value)
        //        {
        //            if (timeline.Ein == EIN)
        //            {
        //                tagTimeline.Add(timeline);
        //            }
        //        }
        //    });
        //    var ReturnList = tagTimeline.OrderBy(x => x.Start).ThenBy(x => x.End).ToList();
        //    return ReturnList;
        //}

  
        public async void UpdateBadgeTransactionScan(JObject transaction)
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
                    await _fileService.WriteFileAsync(filePath, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
                }
            }
        }
        public string GetCraftType(string tagId)
        {
            return _tagList.Where(r => r.Key == tagId).Select(y => y.Value.Properties.CraftName).FirstOrDefault();
        }
        public async void UpdateTagDesignationActivity(DesignationActivityToCraftType daCode)
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
                    await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
                }
            }
        }
        public async Task<object> UpdateTagUIInfo(JObject tagInfo)
        {
            //update DasigbationActivity to CraftType
            bool savetoFile = false;
            try
            {
                if (tagInfo.ContainsKey("tagId"))
                {
                    //find tag id and update proprieties
                    GeoMarker? TagData = null;
                    if (_tagList.TryGetValue(tagInfo["tagId"].ToString(), out TagData))
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
                        if (tagInfo.ContainsKey("craftName"))
                        {
                            TagData.Properties.CraftName = tagInfo["craftName"].ToString();
                            savetoFile = true;
                        }
                        if (tagInfo.ContainsKey("empPayLocation"))
                        {
                            TagData.Properties.EmpPayLocation = tagInfo["empPayLocation"].ToString();
                            savetoFile = true;
                        }
                        if (tagInfo.ContainsKey("tagType"))
                        {
                            string newTagType = tagInfo["tagType"].ToString();
                            if (newTagType.Contains("vehicle", StringComparison.OrdinalIgnoreCase))
                            {
                                // Remove from _tagList and add to _vehicleTagList
                                if (_tagList.TryRemove(tagInfo["tagId"].ToString(), out TagData))
                                {
                                    await _hubServices.Clients.Group(TagData.Properties.TagType).SendAsync($"delete{TagData.Properties.TagType}TagInfo", TagData);
                                    var serializedTagData = JsonConvert.SerializeObject(TagData);
                                    var vehicleTagData = JsonConvert.DeserializeObject<VehicleGeoMarker>(serializedTagData);
                                    vehicleTagData.Properties.Type = tagInfo["tagType"].ToString();

                                    _vehicleTagList.TryAdd(tagInfo["tagId"].ToString(), vehicleTagData);
                                    savetoFile = true;
                                }
                            }
                            else
                            {
                                TagData.Properties.TagType = newTagType;
                                savetoFile = true;
                            }
                        }
                        if (tagInfo.ContainsKey("designationActivity"))
                        {
                            TagData.Properties.DesignationActivity = tagInfo["designationActivity"].ToString();
                            var daCode = _dacode.Get(tagInfo["designationActivity"].ToString());
                            if (daCode != null)
                            {
                                TagData.Properties.DesignationActivity = daCode.DesignationActivity;
                                TagData.Properties.CraftName = daCode.CraftType;
                                savetoFile = true;
                            }
                        }

                        await _hubServices.Clients.Group(TagData.Properties.TagType).SendAsync($"update{TagData.Properties.TagType}TagInfo", TagData);

                        return Task.FromResult(TagData);
                    }
                    else
                    {
                        return Task.FromResult(new JObject { ["Message"] = $"Tag: {tagInfo} not Found" });
                    }
                }
                else
                {
                    return Task.FromResult(new JObject { ["Message"] = $"TagId Parameters missing" });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Task.FromResult(new JObject { ["Error"] = $"{e.Message}" });
            }
            finally
            {
                if (savetoFile)
                {
                    //save date to local file
                    await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
                    await _fileService.WriteFileAsync(vehicleFileName, JsonConvert.SerializeObject(_vehicleTagList.Values, Formatting.Indented));
                }
            }
        }
        public IEnumerable<JObject> SearchTag(string searchValue)
        {
            var badgeQuery = _tagList.Where(sl =>
                Regex.IsMatch(sl.Value.Properties.Id, "(" + searchValue + ")", RegexOptions.IgnoreCase)
                || Regex.IsMatch(sl.Value.Properties.EIN, "(" + searchValue + ")", RegexOptions.IgnoreCase)
                || Regex.IsMatch(sl.Value.Properties.EncodedId, "(" + searchValue + ")", RegexOptions.IgnoreCase)
                || Regex.IsMatch(sl.Value.Properties.CraftName, "(" + searchValue + ")", RegexOptions.IgnoreCase)
                || Regex.IsMatch(sl.Value.Properties.EmpFirstName, "(" + searchValue + ")", RegexOptions.IgnoreCase)
                || Regex.IsMatch(sl.Value.Properties.EmpLastName, "(" + searchValue + ")", RegexOptions.IgnoreCase)
              ).Select(r => r.Value.Properties).ToList();
            var badgeSearchReuslt = (from sr in badgeQuery
                                     select new JObject
                                     {
                                         ["id"] = sr.Id,
                                         ["ein"] = sr.EIN,
                                         ["tagType"] = sr.TagType,
                                         ["name"] = sr.Name,
                                         ["encodedId"] = sr.EncodedId,
                                         ["empFirstName"] = sr.EmpFirstName,
                                         ["empLastName"] = sr.EmpLastName,
                                         ["craftName"] = sr.CraftName,
                                         ["presence"] = sr.isPosition,
                                         ["payLocation"] = sr.PayLocation,
                                         ["designationActivity"] = sr.DesignationActivity,
                                         ["color"] = sr.Color
                                     }).ToList();
            
            var vehicelQuery = _vehicleTagList.Where(sl =>
           Regex.IsMatch(sl.Value.Properties.Id, "(" + searchValue + ")", RegexOptions.IgnoreCase)
           || Regex.IsMatch(sl.Value.Properties.Name, "(" + searchValue + ")", RegexOptions.IgnoreCase)
         ).Select(r => r.Value.Properties).ToList();

            var vehicelSearchReuslt = (from sr in vehicelQuery
                                       select new JObject
                                       {
                                           ["id"] = sr.Id,
                                           ["ein"] = "",
                                           ["tagType"] = sr.Type,
                                           ["name"] = sr.Name,
                                           ["encodedId"] = "",
                                           ["empFirstName"] = sr.Name,
                                           ["empLastName"] = "",
                                           ["craftName"] = "",
                                           ["payLocation"] = "",
                                           ["presence"] = sr.isPosition,
                                           ["designationActivity"] = "",
                                           ["color"] = sr.Color
                                       }).ToList();

            var finalReuslt = badgeSearchReuslt.Concat(vehicelSearchReuslt);
            return finalReuslt;
        }
        public async Task UpdateTagQPEInfo(List<Tags> tags, long responseTS)
        {
            bool saveToFile = false;
            bool saveVehicleToFile = false;
            try
            {
                foreach (Tags qtitem in tags)
                {
                    if (_tagList.ContainsKey(qtitem.TagId))
                    {
                        saveToFile = await Task.Run(() => ProcessQPEBadgeTag(qtitem)).ConfigureAwait(false);
                    }
                    else if (_vehicleTagList.ContainsKey(qtitem.TagId))
                    {
                        saveVehicleToFile = await Task.Run(() => ProcessQPEVehicleTag(qtitem)).ConfigureAwait(false);
                    }
                    else
                    {
                        saveToFile = await Task.Run(() => ProcessQPEBadgeTag(qtitem)).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
            finally
            {
                if (saveToFile)
                {
                    //save date to local file
                    await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
                }
                if (saveVehicleToFile)
                {
                    //save date to local file
                    await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_vehicleTagList.Values, Formatting.Indented));
                }
            }

        }

        private async Task<bool> ProcessQPEVehicleTag(Tags qtitem)
        {
            bool savetoFile = false;
            try
            {
                VehicleGeoMarker? VTagData = null;
                long posAge = -1;
                bool visable = false;
                List<double> positionLocation = [0, 0];
                lock (_vehicleTagList)
                {
                    _vehicleTagList.TryGetValue(qtitem.TagId, out VTagData);
                    qtitem.ServerTS = qtitem.LocationTS;
                    if (qtitem.Location.Any())
                    {
                        positionLocation = [qtitem.Location[0], qtitem.Location[1]];
                    }
                    if (qtitem.LocationTS == 0)
                    {
                        posAge = -1;
                    }
                    else
                    {
                        posAge = qtitem.ServerTS - qtitem.LocationTS;
                    }
                    visable = posAge >= 0 && posAge < 86400000 ? true : false;
                   
                    if (VTagData != null)
                    {
                        VTagData.Geometry.Coordinates = positionLocation;
                        VTagData.Properties.LocationMovementStatus = qtitem.LocationMovementStatus;
                        VTagData.Properties.Visible = visable;
                        VTagData.Properties.PositionTS = qtitem.LocationTS;
                        VTagData.Properties.ServerTS = qtitem.LocationTS;
                        if (string.IsNullOrEmpty(VTagData.Properties.Type))
                        {
                            VTagData.Properties.Type = "Vehicle";
                            savetoFile = true;
                        }
                        if (!string.IsNullOrEmpty(qtitem.TagName))
                        {

                        }
                    }
                    else
                    {
                        VTagData = new VehicleGeoMarker
                        {
                            Geometry = new VehicleMarkerGeometry
                            {
                                Coordinates = positionLocation,
                                Type = "Point"
                            },
                            Properties = new Vehicle
                            {
                                Id = qtitem.TagId,
                                Name = qtitem.TagName,
                                Color = qtitem.Color,
                                LocationMovementStatus = qtitem.LocationMovementStatus,
                                ServerTS = qtitem.ServerTS,
                                PositionTS = qtitem.LocationTS,
                                Type = "Vehicle",
                                isPosition = visable,
                                Visible = true
                            }
                        };


                        if (_vehicleTagList.TryAdd(qtitem.TagId, VTagData))
                        {
                            savetoFile = true;
                        }

                    }
                }
                if (qtitem.Location.Any())
                {
                  await  _hubServices.Clients.Group(VTagData?.Properties.Type).SendAsync($"update{VTagData?.Properties.Type}TagPosition", positionLocationVehicleMarkersUpdate(VTagData));
                }
                return await Task.FromResult(savetoFile);

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return await Task.FromResult(savetoFile);
            }
           
        }

        private async Task<bool> ProcessQPEBadgeTag(Tags qtitem)
        {
            bool savetoFile = false;
            try
            {
                GeoMarker? TagData = null;
                long posAge = -1;
                bool visable = false;
                List<double> positionLocation = [0, 0];
                lock (_tagList)
                {
                    _tagList.TryGetValue(qtitem.TagId, out TagData);
                    if (qtitem.Location.Any())
                    {
                        positionLocation = [qtitem.Location[0], qtitem.Location[1]];
                    }
                    qtitem.ServerTS = qtitem.LocationTS;
                    if (qtitem.LocationTS == 0)
                    {
                        posAge = -1;
                    }
                    else
                    {
                        posAge = qtitem.ServerTS - qtitem.LocationTS;
                    }
                    visable = posAge >= 0 && posAge < 150000 ? true : false;
                    if (qtitem.LocationType == "presence" || qtitem.LocationType == "proximity" || qtitem.LocationType == "hidden")
                    {
                        visable = false;
                    }
                    if (qtitem.LocationMovementStatus == "hidden" || qtitem.LocationMovementStatus == "noData")
                    {
                        visable = false;
                    }
                    if (TagData != null)
                    {
                        TagData.Properties.Color = qtitem.Color;
                        TagData.Properties.Zones = qtitem.LocationZoneIds;
                        TagData.Geometry.Coordinates = positionLocation;
                        TagData.Properties.LocationMovementStatus = qtitem.LocationMovementStatus;
                        TagData.Properties.Visible = visable;
                        if (string.IsNullOrEmpty(TagData.Properties.TagType))
                        {
                            TagData.Properties.TagType = "Badge";
                            savetoFile = true;
                        }
                        if (!string.IsNullOrEmpty(qtitem.TagName))
                        {

                        }
                    }
                    else
                    {
                        TagData = new GeoMarker
                        {
                            Geometry = new MarkerGeometry
                            {
                                Coordinates = positionLocation,
                                Type = "Point"
                            },
                            Properties = new Marker
                            {
                                Id = qtitem.TagId,
                                Name = qtitem.TagName,
                                Color = qtitem.Color,
                                Zones = qtitem.LocationZoneIds,
                                LocationMovementStatus = qtitem.LocationMovementStatus,
                                ServerTS = qtitem.ServerTS,
                                PositionTS = qtitem.LocationTS,
                                TagType = "Badge",
                                Visible = visable,
                                isPosition = visable
                            }
                        };


                        if (_tagList.TryAdd(qtitem.TagId, TagData))
                        {
                            savetoFile = true;
                        }

                    }
                }
                if (qtitem.Location.Any())
                {
                   await  _hubServices.Clients.Group(TagData?.Properties.TagType).SendAsync($"update{TagData?.Properties.TagType}TagPosition", positionLocationMarkersUpdate(TagData));
                }
                return await Task.FromResult(savetoFile);

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return await Task.FromResult(savetoFile);
            }
           

        }
        private GeoMarker positionLocationMarkersUpdate(GeoMarker marker)
        {
            try
            {
                var jsonResolver = new PropertyRenameAndIgnoreSerializerContractResolver();
                jsonResolver.IgnoreProperty(typeof(Marker), "BadgeScan");
                jsonResolver.IgnoreProperty(typeof(Marker), "LocationType");
                jsonResolver.IgnoreProperty(typeof(Marker), "ZonesNames");
                jsonResolver.IgnoreProperty(typeof(Marker), "NotificationId");
                jsonResolver.IgnoreProperty(typeof(Marker), "Source");
                jsonResolver.IgnoreProperty(typeof(Marker), "DaysOff");
                jsonResolver.IgnoreProperty(typeof(Marker), "ReqDate");
                jsonResolver.IgnoreProperty(typeof(Marker), "TourNumber");
                jsonResolver.IgnoreProperty(typeof(Marker), "Edate");
                jsonResolver.IgnoreProperty(typeof(Marker), "Elunch");
                jsonResolver.IgnoreProperty(typeof(Marker), "Blunch");
                jsonResolver.IgnoreProperty(typeof(Marker), "Bdate");
                jsonResolver.IgnoreProperty(typeof(Marker), "EncodedId");
                jsonResolver.IgnoreProperty(typeof(Marker), "EmpPayLocation");
                jsonResolver.IgnoreProperty(typeof(Marker), "DesignationActivity");
                jsonResolver.IgnoreProperty(typeof(Marker), "Title");
                jsonResolver.IgnoreProperty(typeof(Marker), "EmpLastName");
                jsonResolver.IgnoreProperty(typeof(Marker), "EmpFirstName");
                jsonResolver.IgnoreProperty(typeof(Marker), "PayLocation");
                jsonResolver.IgnoreProperty(typeof(Marker), "LDC");
                jsonResolver.IgnoreProperty(typeof(Marker), "EIN");
                jsonResolver.IgnoreProperty(typeof(Marker), "CardHolderId");
                jsonResolver.IgnoreProperty(typeof(Marker), "isePacs");
                jsonResolver.IgnoreProperty(typeof(Marker), "isTacs");
                jsonResolver.IgnoreProperty(typeof(Marker), "isSch");
                jsonResolver.IgnoreProperty(typeof(Marker), "ServerTS");
                jsonResolver.IgnoreProperty(typeof(Marker), "ServerTS_txt");
                jsonResolver.IgnoreProperty(typeof(Marker), "PositionTS");
                jsonResolver.IgnoreProperty(typeof(Marker), "Name");
                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.ContractResolver = jsonResolver;
                return marker;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }
        private VehicleGeoMarker positionLocationVehicleMarkersUpdate(VehicleGeoMarker marker)
        {
            try
            {
                var jsonResolver = new PropertyRenameAndIgnoreSerializerContractResolver();
                jsonResolver.IgnoreProperty(typeof(Marker), "NotificationId");
                jsonResolver.IgnoreProperty(typeof(Marker), "Mission");
                jsonResolver.IgnoreProperty(typeof(Marker), "Vehicle_Status_Data");
                jsonResolver.IgnoreProperty(typeof(Marker), "ServerTS");
                jsonResolver.IgnoreProperty(typeof(Marker), "ServerTS_txt");
                jsonResolver.IgnoreProperty(typeof(Marker), "PositionTS");
                jsonResolver.IgnoreProperty(typeof(Marker), "Name");
                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.ContractResolver = jsonResolver;
                return marker;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public async Task UpdateTagCiscoSpacesClientInfo(JToken result)
        {
            bool savetoFile = false;
            try
            {
                //loop through the result and update the tag position
                if (result is not null && ((JObject)result).ContainsKey("features"))
                {
                    foreach (var item in result.SelectToken("features"))
                    {
                        string tagId = item.SelectToken("properties.macAddress")?.ToString();
                        if (!string.IsNullOrEmpty(tagId))
                        {
                            GeoMarker? TagData = null;
                            _tagList.TryGetValue(tagId, out TagData);
                            if (TagData != null)
                            {
                                await _hubServices.Clients.Group(TagData?.Properties.TagType).SendAsync($"update{TagData?.Properties.TagType}TagPosition", "");
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
                    await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
                }

            }
        }
        public async Task UpdateTagCiscoSpacesBLEInfo(JToken result)
        {
            bool savetoFile = false;
            try
            {
                //loop through the result and update the tag position
                if (result is not null && ((JObject)result).ContainsKey("features"))
                {
                    foreach (var item in result.SelectToken("features"))
                    {
                        string tagId = item.SelectToken("properties.macAddress")?.ToString();
                        if (!string.IsNullOrEmpty(tagId))
                        {
                            GeoMarker? TagData = null;
                            _tagList.TryGetValue(tagId, out TagData);
                            if (TagData != null)
                            {
                                await _hubServices.Clients.Group(TagData?.Properties.TagType).SendAsync($"update{TagData?.Properties.TagType}TagPosition", "");
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
                    await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
                }

            }
        }

        public async Task UpdateTagCiscoSpacesAPInfo(JToken result)
        {
            bool savetoFile = false;
            try
            {
                //loop through the result and update the tag position

                if (result is not null)
                {
                    foreach (var item in result)
                    {
                        GeoMarker? TagData = null;
                        JObject PositionGeoJson = new JObject
                        {
                            ["type"] = "Feature",
                            ["geometry"] = new JObject
                            {
                                ["type"] = "Point",
                                ["coordinates"] = new JArray((double)item["x"], (double)item["y"])
                            },
                            ["properties"] = new JObject
                            {
                                ["id"] = item["macAddress"].ToString(),
                                ["floorId"] = item["macAddress"].ToString(),
                                ["name"] = item["name"].ToString(),
                                ["posAge"] = 0,
                                ["visible"] = true,
                                ["zones"] = "",
                                ["locationMovementStatus"] = "",
                                ["positionTS_txt"] = DateTime.Now.ToString(),
                                ["craftName"] = item["make"].ToString(),
                                ["tagType"] = item["level"].ToString()
                            }
                        };
                        _tagList.TryGetValue(item["macAddress"].ToString(), out TagData);
                        if (TagData != null)
                        {
                            if (TagData.Properties.Name != item["displayName"].ToString())
                            {
                                TagData.Properties.Name = item["displayName"].ToString();
                                savetoFile = false;
                            }
                            if (TagData.Properties.CraftName != item["make"].ToString())
                            {
                                TagData.Properties.CraftName = item["make"].ToString();
                                savetoFile = false;
                            }
                            if (TagData.Properties.TagType != item["level"].ToString())
                            {
                                TagData.Properties.TagType = item["level"].ToString();
                                savetoFile = false;
                            }
                            if (TagData.Properties.EmpFirstName != item["model"].ToString())
                            {
                                TagData.Properties.EmpFirstName = item["model"].ToString();
                                savetoFile = false;
                            }
                            if (TagData.Properties.EmpLastName != item["ipAddress"].ToString())
                            {
                                TagData.Properties.EmpLastName = item["ipAddress"].ToString();
                                savetoFile = false;
                            }

                            if (TagData.Geometry.Coordinates != new List<double> { (double)item["x"], (double)item["y"] })
                            {
                                TagData.Geometry.Coordinates = new List<double> { (double)item["x"], (double)item["y"] };
                                await _hubServices.Clients.Group(TagData?.Properties.TagType).SendAsync($"update{TagData?.Properties.TagType}Position", PositionGeoJson);
                            }

                        }
                        else
                        {
                            TagData = new GeoMarker
                            {
                                Geometry = new MarkerGeometry
                                {
                                    Coordinates = new List<double> { (double)item["x"], (double)item["y"] },
                                    Type = "Point"
                                },
                                Properties = new Marker
                                {
                                    Id = item["macAddress"].ToString(),
                                    Name = item["name"].ToString(),
                                    TagType = item["level"].ToString(),
                                    EmpFirstName = item["model"].ToString(),
                                    EmpLastName = item["ipAddress"].ToString(),
                                    Visible = true
                                }
                            };
                            if (_tagList.TryAdd(item["macAddress"].ToString(), TagData))
                            {
                                await _hubServices.Clients.Group(TagData?.Properties.TagType).SendAsync($"update{TagData?.Properties.TagType}Position", PositionGeoJson);
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
                 await _fileService.WriteFileAsync(fileName, JsonConvert.SerializeObject(_tagList.Values, Formatting.Indented));
                }

            }
        }
    }
}
