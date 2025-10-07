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
    public class InMemoryCamerasRepository : IInMemoryCamerasRepository
    {
        private readonly ConcurrentDictionary<string, CameraGeoMarker> _cameraMarkers = new();
        private readonly ConcurrentDictionary<string, Cameras> _cameraList = new();
        private readonly ILogger<InMemoryCamerasRepository> _logger;
        protected readonly IHubContext<HubServices> _hubServices;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IFileService _fileService;
        private readonly string fileName = "Cameras.json";
        private readonly string cameraInfofileName = "CameraInfo.json";
        private readonly string imgFilePath = "";
        private readonly byte[] noimageresult = Array.Empty<byte>();
        public InMemoryCamerasRepository(ILogger<InMemoryCamerasRepository> logger, IHubContext<HubServices> hubServices, IConfiguration configuration, IFileService fileService, IWebHostEnvironment hostEnvironment)
        {
            _fileService = fileService;
            _logger = logger;
            _configuration = configuration;
            _hubServices = hubServices;
            _hostEnvironment = hostEnvironment;
            //No Image
            imgFilePath = Path.Combine(_hostEnvironment.WebRootPath, "css\\images", "NoImageFeed.jpg");
            noimageresult = File.ReadAllBytes(imgFilePath);
            // Load data from the first file into the first collection
            LoadDataFromFile().Wait();
            LoadCameraInfoListDataFromFile().Wait();

        }


        public async Task<CameraGeoMarker>? Add(CameraGeoMarker camera)
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
        public async Task<CameraGeoMarker>? Delete(string cameraId)
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
        public List<CameraGeoMarker> GetAll()
        {
            return _cameraMarkers.Values.Select(y => y).ToList();
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
                        foreach (Cameras item in data.Select(r => r).ToList())
                        {
                            if (!_cameraList.ContainsKey(item.IP) && !string.IsNullOrEmpty(item.IP))
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
                        foreach (CameraGeoMarker item in data.Select(r => r).ToList())
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

        public async Task<Cameras>? AddCameraInfo(Cameras camera)
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

        public async Task<Cameras>? UpdateCameraInfo(Cameras camera)
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

        public async Task LoadCameraData(List<Cameras> cameraList)
        {
            bool saveToFile = false;
            try
            {
                foreach (var camera in cameraList)
                {
                    if (!string.IsNullOrEmpty(camera.CameraName))
                    {


                        if (_cameraList.ContainsKey(camera.IP) && _cameraList.TryGetValue(camera.IP, out Cameras cl) && _cameraList.TryUpdate(camera.IP, camera, cl))
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
                string newImage = "";
                if (result.Length == 0)
                {
                    newImage = "data:image/jpeg;base64," + Convert.ToBase64String(noimageresult);
                }
                else
                {
                    newImage = "data:image/jpeg;base64," + Convert.ToBase64String(result);
                }
                if (_cameraMarkers.ContainsKey(id) && _cameraMarkers.TryGetValue(id, out CameraGeoMarker cm))
                {
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

        public Task<Cameras> GetCameraListByIp(string ip)
        {
            return Task.FromResult(_cameraList.Values.Where(y => y.IP == ip).Select(y => y).ToList().FirstOrDefault());
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

        public class PropertyRenameAndIgnoreSerializerContractResolver : DefaultContractResolver
        {
            private readonly Dictionary<Type, HashSet<string>> _ignores;
            private readonly Dictionary<Type, Dictionary<string, string>> _renames;

            public PropertyRenameAndIgnoreSerializerContractResolver()
            {
                _ignores = new Dictionary<Type, HashSet<string>>();
                _renames = new Dictionary<Type, Dictionary<string, string>>();
            }

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

            public void RenameProperty(Type type, string propertyName, string newJsonPropertyName)
            {
                if (!_renames.ContainsKey(type))
                {
                    _renames[type] = new Dictionary<string, string>();
                }

                _renames[type][propertyName] = newJsonPropertyName;
            }

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

            private bool IsIgnored(Type type, string jsonPropertyName)
            {
                if (!_ignores.ContainsKey(type))
                {
                    return false;
                }

                return _ignores[type].Contains(jsonPropertyName);
            }

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