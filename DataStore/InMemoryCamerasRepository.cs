using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;

namespace EIR_9209_2.DataStore
{
    /// <summary>
    /// In-memory repository for managing camera geo markers and camera information.
    /// </summary>
    public class InMemoryCamerasRepository : IInMemoryCamerasRepository
    {
        private readonly ConcurrentDictionary<string, CameraGeoMarker> _cameraMarkers = new();
        private readonly ConcurrentDictionary<string, Cameras> _cameraList = new();
        private readonly ILogger<InMemoryCamerasRepository> _logger;
        protected readonly IHubContext<HubServices> _hubServices;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IFileService _fileService;
        private readonly string fileName = "Cameras.json";
        private readonly string cameraInfofileName = "CameraInfo.json";

        private readonly byte[] noimageresult;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="hubServices"></param>
        /// <param name="fileService"></param>
        /// <param name="hostEnvironment"></param>
        public InMemoryCamerasRepository(ILogger<InMemoryCamerasRepository> logger, IHubContext<HubServices> hubServices, IFileService fileService, IWebHostEnvironment hostEnvironment)
        {
            _fileService = fileService;
            _logger = logger;
            _hubServices = hubServices;
            _hostEnvironment = hostEnvironment;
            //No Image
            noimageresult = File.ReadAllBytes(Path.Combine(_hostEnvironment.WebRootPath, "css\\images", "NoImageFeed.jpg"));
            // Load data from the first file into the first collection
            LoadDataFromFile().Wait();
            LoadCameraInfoListDataFromFile().Wait();

        }

        /// <summary>
        ///  Adds a new camera geo marker to the repository.
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public async Task<CameraGeoMarker?> Add(CameraGeoMarker camera)
        {
            bool saveToFile = false;
            try
            {
                camera.Properties.Base64Image = "data:image/jpeg;base64," + Convert.ToBase64String(noimageresult);
                if (_cameraMarkers.TryAdd(camera.Properties.Id, camera))
                {
                    saveToFile = true;
                    return camera;
                }
                else
                {
                    _logger.LogError($"Camera was not added to list");
                    return null;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return null;
            }
            finally
            {
                if (saveToFile)
                {
                    await _fileService.WriteConfigurationFile(fileName, CameraMarkersOutPutdata(_cameraMarkers.Select(x => x.Value).ToList()));
                }
            }
        }
        /// <summary>
        /// Deletes a camera geo marker by its unique identifier.
        /// </summary>
        /// <param name="cameraId"></param>
        /// <returns></returns>
        public async Task<CameraGeoMarker?> Delete(string cameraId)
        {
            bool saveToFile = false;
            try
            {
                if (_cameraMarkers.ContainsKey(cameraId) && _cameraMarkers.TryRemove(cameraId, out CameraGeoMarker? camera))
                {
                    saveToFile = true;
                    return camera;
                }
                else
                {
                    _logger.LogError($"Unable to remove Camera from list.");
                    return null;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return null;
            }
            finally
            {
                if (saveToFile)
                {
                    await _fileService.WriteConfigurationFile(fileName, CameraMarkersOutPutdata(_cameraMarkers.Select(x => x.Value).ToList()));
                }
            }
        }
        /// <summary>
        /// Updates an existing camera geo marker in the repository.
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public async Task Update(CameraGeoMarker camera)
        {
            try
            {
                if (_cameraMarkers.ContainsKey(camera.Properties.Id) && _cameraMarkers.TryGetValue(camera.Properties.Id, out CameraGeoMarker cm) && _cameraMarkers.TryUpdate(camera.Properties.Id, camera, cm))
                {
                    await _hubServices.Clients.Group(camera.Properties.Type).SendAsync($"update{camera.Properties.Type}", cm);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
        /// <summary>
        /// Serializes a list of camera geo markers to a JSON string, excluding the Base64Image property.
        /// </summary>
        /// <param name="cameraMarkers"></param>
        /// <returns></returns>
        private string CameraMarkersOutPutdata(List<CameraGeoMarker> cameraMarkers)
        {
            try
            {
                var jsonResolver = new PropertyRenameAndIgnoreSerializerContractResolver();
                jsonResolver.IgnoreProperty(typeof(Cameras), "Base64Image");
                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.ContractResolver = jsonResolver;
                return JsonConvert.SerializeObject(cameraMarkers, Formatting.Indented, serializerSettings);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return "";
            }
        }
        /// <summary>
        ///  Retrieves a camera object by its unique identifier.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public object Get(string id)
        {
            if (_cameraMarkers.ContainsKey(id) && _cameraMarkers.TryGetValue(id, out CameraGeoMarker camera))
            {
                return camera;
            }
            else
            {
                return new JObject { ["Message"] = "Tag not Found" };
            }
        }
        /// <summary>
        /// Retrieves all camera geo markers from the repository.
        /// </summary>
        /// <returns></returns> <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<CameraGeoMarker>> GetAll()
        {
            return _cameraMarkers.Values.Select(y => y).ToList();
        }
        /// <summary>
        ///  Retrieves cameras by floor ID and type.
        /// </summary>
        /// <param name="floorId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<(bool, object?)> GetCameraByFloorId(string floorId, string type)
        {
            try
            {
                var cameras = _cameraMarkers.Values.Where(c => c.Properties.FloorId == floorId && c.Properties.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).Select(y => y).ToList();
                return (true, cameras);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return (false, null);
            }
        }
        /// <summary>
        /// Retrieves all camera properties from the repository.
        /// </summary>
        /// <returns></returns>
        public List<Cameras> GetCameraListAll()
        {
            return _cameraList.Values.OrderBy(y => y.Description).Select(y => y).ToList();
        }
        private async Task LoadCameraInfoListDataFromFile()
        {
            try
            {
                // Read data from file
                var fileContent = await _fileService.ReadFile(cameraInfofileName);
                if (!string.IsNullOrEmpty(fileContent))
                {
                    // Parse the file content to get the data. This depends on the format of your file.
                    // Here's an example if your file was in JSON format and contained an array of T objects:
                    List<Cameras>? data = JsonConvert.DeserializeObject<List<Cameras>>(fileContent);

                    // Insert the data into the MongoDB collection
                    if (data?.Count > 0)
                    {
                        foreach (Cameras item in data.Where(r => !string.IsNullOrEmpty(r.IP)))
                        {
                            if (!_cameraList.ContainsKey(item.IP))
                            {
                                _cameraList.TryAdd(item.IP, item);
                            }
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
        private async Task LoadDataFromFile()
        {
            try
            {
                // Read data from file
                var fileContent = await _fileService.ReadFile(fileName);
                if (!string.IsNullOrEmpty(fileContent))
                {
                    // Parse the file content to get the data. This depends on the format of your file.
                    List<CameraGeoMarker>? data = JsonConvert.DeserializeObject<List<CameraGeoMarker>>(fileContent);

                    // Insert the data into the MongoDB collection
                    if (data?.Count > 0)
                    {
                        foreach (CameraGeoMarker item in data)
                        {
                            item.Properties.Base64Image = "data:image/jpeg;base64," + Convert.ToBase64String(noimageresult);
                            _cameraMarkers.TryAdd(item.Properties.Id, item);
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
        /// <summary>
        /// Adds camera information to the repository.
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public async Task<Cameras?> AddCameraInfo(Cameras camera)
        {
            bool saveToFile = false;
            try
            {
                if (_cameraList.TryAdd(camera.Id, camera))
                {
                    saveToFile = true;
                    return camera;
                }
                else
                {
                    _logger.LogError($"Camera was not added to list");
                    return null;
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}");
                return null;
            }
            finally
            {
                if (saveToFile)
                {
                    await _fileService.WriteConfigurationFile(cameraInfofileName, JsonConvert.SerializeObject(_cameraList.Values, Formatting.Indented));
                }
            }
        }
        /// <summary>
        /// Updates camera information in the repository.
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public async Task<Cameras> UpdateCameraInfo(Cameras camera)
        {
            bool saveToFile = false;
            try
            {
                if (_cameraList.ContainsKey(camera.Id) && _cameraList.TryGetValue(camera.Id, out Cameras c) && _cameraList.TryUpdate(camera.Id, camera, c))
                {
                    saveToFile = true;
                    return c;
                }
                else
                {
                    _logger.LogError($"Camera was not added to list");
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
                    await _fileService.WriteConfigurationFile(cameraInfofileName, JsonConvert.SerializeObject(_cameraList.Values, Formatting.Indented));
                }
            }
        }
        /// <summary>
        /// Deletes camera information by its unique identifier.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Cameras> DeleteCameraInfo(string id)
        {
            bool saveToFile = false;
            try
            {
                if (_cameraList.TryRemove(id, out Cameras? camera))
                {
                    saveToFile = true;
                    return await Task.FromResult(camera);
                }
                else
                {
                    _logger.LogError($"Camera was not added to list");
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
                    await _fileService.WriteConfigurationFile(cameraInfofileName, JsonConvert.SerializeObject(_cameraList.Values, Formatting.Indented));
                }
            }
        }
        /// <summary>
        /// Loads a list of camera information into the repository.
        /// </summary>
        /// <param name="cameraList"></param>
        /// <returns></returns>
        public async Task LoadCameraData(List<Cameras> cameraList)
        {
            bool saveToFile = false;
            try
            {
                foreach (var camera in cameraList.Where(camera => !string.IsNullOrEmpty(camera.CameraName)))
                {
                    //update geo marker if exists
                    var exists = _cameraMarkers.Where(cm => cm.Value.Properties.CameraName == camera.CameraName || cm.Value.Properties.IP == camera.IP).Select(cm => cm.Key).FirstOrDefault();
                    if (exists != null && _cameraMarkers.TryGetValue(exists, out CameraGeoMarker? cmaker))
                    {
                        cmaker.Properties.CameraName = camera.CameraName;
                        cmaker.Properties.Description = camera.Description;
                        cmaker.Properties.IP = camera.IP;
                        cmaker.Properties.CameraId = camera.CameraId;
                        cmaker.Properties.Reachable = camera.Reachable;
                        _cameraMarkers.TryUpdate(exists, cmaker, cmaker);
                        await _hubServices.Clients.Group(cmaker.Properties.Type).SendAsync($"update{cmaker.Properties.Type}", cmaker);
                    }
                    var cameraListexists = _cameraList.Where(cm => cm.Value.CameraName == camera.CameraName || cm.Value.IP == camera.IP).Select(cm => cm.Key).FirstOrDefault();
                    if (cameraListexists != null && _cameraList.TryGetValue(cameraListexists, out Cameras? cl) && _cameraList.TryUpdate(cameraListexists, camera, cl))
                    {
                        saveToFile = true;
                    }
                    else
                    {
                        _cameraList.TryAdd(camera.IP, camera);
                        saveToFile = true;
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
                    await _fileService.WriteConfigurationFile(cameraInfofileName, JsonConvert.SerializeObject(_cameraList.Values, Formatting.Indented));
                }
            }
        }
        /// <summary>
        /// Loads camera still images into the repository.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task LoadCameraStills(byte[] result, string id)
        {
            try
            {

                if (_cameraMarkers.ContainsKey(id) && _cameraMarkers.TryGetValue(id, out CameraGeoMarker cm))
                {
                    string newImage = "";
                    if (result.Length == 0)
                    {
                        newImage = "data:image/jpeg;base64," + Convert.ToBase64String(noimageresult);
                        cm.Properties.Reachable = false;
                    }
                    else
                    {
                        newImage = "data:image/jpeg;base64," + Convert.ToBase64String(result);
                        cm.Properties.Reachable = true;
                    }
                    if (cm.Properties.Base64Image != newImage)
                    {
                        cm.Properties.Base64Image = newImage;
                        await _hubServices.Clients.Group($"{cm.Properties.Type}Still").SendAsync($"update{cm.Properties.Type}Still", cm);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
        /// <summary>
        /// Retrieves camera information by IP address.
        /// </summary>
        /// <param name="Ip"></param>
        /// <returns></returns>
        public async Task<Cameras> GetCameraListByIp(string Ip)
        {
            try
            {
                if (!string.IsNullOrEmpty(Ip))
                {
                    return _cameraList?.Values?.Where(y => y.IP == Ip || y.CameraName == Ip).Select(y => y).FirstOrDefault();
                }
                return null;
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}");
                return null;
            }
        }

        public Task<bool> ResetCamerasList()
        {
            try
            {
                _cameraMarkers.Clear();
                _cameraList.Clear();
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Task.FromResult(true);
            }
        }

        public Task<bool> SetupCamerasList()
        {
            try
            {
                // Load data from the first file into the first collection
                LoadDataFromFile().Wait();
                LoadCameraInfoListDataFromFile().Wait();
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Task.FromResult(true);
            }
        }
        /// <summary>
        /// Custom contract resolver to ignore and rename properties during JSON serialization.
        /// </summary>
        public class PropertyRenameAndIgnoreSerializerContractResolver : DefaultContractResolver
        {
            private readonly Dictionary<Type, HashSet<string>> _ignores;
            private readonly Dictionary<Type, Dictionary<string, string>> _renames;
            /// <summary>
            /// Initializes a new instance of the <see cref="PropertyRenameAndIgnoreSerializerContractResolver"/> class.
            /// </summary>
            public PropertyRenameAndIgnoreSerializerContractResolver()
            {
                _ignores = new Dictionary<Type, HashSet<string>>();
                _renames = new Dictionary<Type, Dictionary<string, string>>();
            }
            /// <summary>
            /// Ignores the specified JSON property names for the given type.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="jsonPropertyNames"></param>
            public void IgnoreProperty(Type type, params string[] jsonPropertyNames)
            {
                if (!_ignores.ContainsKey(type))
                {
                    _ignores[type] = new HashSet<string>();
                }

                foreach (var prop in jsonPropertyNames)
                {
                    _ = _ignores[type].Add(prop);
                }
            }
            /// <summary>
            /// Renames the specified property for the given type.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="propertyName"></param>
            /// <param name="newJsonPropertyName"></param>
            public void RenameProperty(Type type, string propertyName, string newJsonPropertyName)
            {
                if (!_renames.ContainsKey(type))
                {
                    _renames[type] = new Dictionary<string, string>();
                }

                _renames[type][propertyName] = newJsonPropertyName;
            }

            /// <summary>
            /// Creates a JSON property for the given member.
            /// </summary>
            /// <param name="member"></param>
            /// <param name="memberSerialization"></param>
            /// <returns></returns>
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                if (IsIgnored(property.DeclaringType, property.PropertyName))
                {
                    property.ShouldSerialize = i => false;
                    property.Ignored = true;
                }

                if (IsRenamed(property.DeclaringType, property.PropertyName, out var newJsonPropertyName))
                {
                    property.PropertyName = newJsonPropertyName;
                }

                return property;
            }
            /// <summary>
            /// Determines whether the specified property should be ignored for the given type.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="jsonPropertyName"></param>
            /// <returns></returns>
            private bool IsIgnored(Type type, string jsonPropertyName)
            {
                if (!_ignores.ContainsKey(type))
                {
                    return false;
                }

                return _ignores[type].Contains(jsonPropertyName);
            }
            /// <summary>
            /// Determines whether the specified property should be renamed for the given type.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="jsonPropertyName"></param>
            /// <param name="newJsonPropertyName"></param>
            /// <returns></returns>
            private bool IsRenamed(Type type, string jsonPropertyName, out string? newJsonPropertyName)
            {
                Dictionary<string, string> renames;

                if (!_renames.TryGetValue(type, out renames) || !renames.TryGetValue(jsonPropertyName, out newJsonPropertyName))
                {
                    newJsonPropertyName = null;
                    return false;
                }

                return true;
            }
        }
    }

}