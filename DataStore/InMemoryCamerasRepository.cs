using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using MimeKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static EIR_9209_2.Models.GeoMarker;

namespace EIR_9209_2.DataStore
{
    public class InMemoryCamerasRepository : IInMemoryCamerasRepository
    {
        private readonly ConcurrentDictionary<string, CameraMarker> _cameraMarkers = new();
        private readonly ILogger<InMemoryCamerasRepository> _logger;
        protected readonly IHubContext<HubServices> _hubServices;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IFileService _fileService;
        private readonly string filePath = "";
        private readonly string fileName = "";
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

            fileName = $"{_configuration[key: "InMemoryCollection:CollectionCameras"]}.json";
            filePath = Path.Combine(_configuration[key: "ApplicationConfiguration:BaseDrive"],
                _configuration[key: "ApplicationConfiguration:BaseDirectory"],
                _configuration[key: "ApplicationConfiguration:NassCode"],
                _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"],
                $"{fileName}");
            //// Load data from the first file into the first collection
            _ = LoadDataFromFile(filePath);

        }
        public async Task Add(CameraMarker camera)
        {
            bool saveToFile = false;
            try
            {
                if (_cameraMarkers.TryGetValue(camera.Id, out CameraMarker curcamera))
                {
                    curcamera.CameraDirection = camera.CameraDirection;
                    curcamera.Geometry = camera.Geometry;
                    saveToFile = true;
                    JObject PositionGeoJson = new JObject
                    {
                        ["type"] = "Feature",
                        ["geometry"] = new JObject
                        {
                            ["type"] = "Point",
                            ["coordinates"] = curcamera.Geometry.Coordinates.Any() ? new JArray(curcamera.Geometry.Coordinates[0], curcamera.Geometry.Coordinates[1]) : new JArray(0, 0)
                        },
                        ["properties"] = new JObject
                        {
                            ["id"] = curcamera.Id,
                            ["cameraDirection"] = curcamera.CameraDirection,
                            ["cameraData"] = curcamera.CameraData.ToJToken(),
                            ["base64Image"] = curcamera.Base64Image,
                            ["visible"] = true
                        }
                    };
                    await _hubServices.Clients.Group("Camera").SendAsync("updateCamera", PositionGeoJson.ToString());
                }
                else
                {
                    _logger.LogError($"Camera File list was not saved...");
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
                    _fileService.WriteFile(fileName, JsonConvert.SerializeObject(_cameraMarkers.Values, Formatting.Indented));
                }
            }
        }
        public object Get(string id)
        {
            if (_cameraMarkers.ContainsKey(id) && _cameraMarkers.TryGetValue(id, out CameraMarker camera))
            {
                return camera;
            }
            else
            {
                return new JObject { ["Message"] = "Tag not Found" };
            }
        }
        public List<CameraMarker> GetAll()
        {
            return _cameraMarkers.Values.Select(y => y).ToList();
        }
        public List<string> GetCameraListAll()
        {
            return _cameraMarkers.Values.OrderBy(y => y.CameraData.Description).Select(y => y.DisplayName).ToList();
        }

        async Task IInMemoryCamerasRepository.LoadCameraData(JToken result)
        {
            bool savetoFile = false;
            try
            {
                //loop through the result and update the tag position
                if (result is not null)
                {
                    List<Cameras> newCameras = result.ToObject<List<Cameras>>();
                    if (newCameras.Any())
                    {
                        foreach (Cameras item in newCameras)
                        {
                            if (!_cameraMarkers.ContainsKey(item.CameraName))
                            {
                                _cameraMarkers.TryAdd(item.CameraName, new CameraMarker
                                {
                                    Id = item.CameraName,
                                    DisplayName = item.CameraName + " / " + item.Description,
                                    CameraData = item
                                });
                            }
                            else
                            {
                                if (_cameraMarkers.TryGetValue(item.CameraName, out var curcamera))
                                {
                                    curcamera.CameraData = item;
                                }
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
                    _fileService.WriteFile(fileName, JsonConvert.SerializeObject(_cameraMarkers.Values, Formatting.Indented));
                }

            }
        }
        async Task IInMemoryCamerasRepository.LoadPictureData(byte[] image, string id, bool picload)
        {
            try
            {
                string imageBase64 = "";
                if (picload)
                {
                    imageBase64 = "data:image/jpeg;base64," + Convert.ToBase64String(image);
                } 
                else
                {
                    imageBase64 = "data:image/jpeg;base64," + Convert.ToBase64String(noimageresult);
                }
                if (_cameraMarkers.TryGetValue(id, out var curCamera))
                {
                    if (curCamera.Base64Image != imageBase64)
                    {
                        curCamera.Base64Image = imageBase64;
                    }
                    if (curCamera.Geometry.Coordinates.Any())
                    {
                        JObject PositionGeoJson = new JObject
                        {
                            ["type"] = "Feature",
                            ["geometry"] = new JObject
                            {
                                ["type"] = "Point",
                                ["coordinates"] = curCamera.Geometry.Coordinates.Any() ? new JArray(curCamera.Geometry.Coordinates[0], curCamera.Geometry.Coordinates[1]) : new JArray(0, 0)
                            },
                            ["properties"] = new JObject
                            {
                                ["id"] = curCamera.Id,
                                ["cameraDirection"] = curCamera.CameraDirection,
                                ["cameraData"] = curCamera.CameraData.ToJToken(),
                                ["base64Image"] = curCamera.Base64Image,
                                ["visible"] = true
                            }
                        };
                        await _hubServices.Clients.Group("Camera").SendAsync("updateCamera", PositionGeoJson.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
            finally
            {
                //save date to local file
                _fileService.WriteFile(fileName, JsonConvert.SerializeObject(_cameraMarkers.Values, Formatting.Indented));
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
                List<CameraMarker> data = JsonConvert.DeserializeObject<List<CameraMarker>>(fileContent);

                // Insert the data into the MongoDB collection
                if (data.Count != 0)
                {
                    foreach (CameraMarker item in data.Select(r => r).ToList())
                    {
                        _cameraMarkers.TryAdd(item.Id, item);
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

}
