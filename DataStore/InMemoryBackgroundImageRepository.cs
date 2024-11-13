using EIR_9209_2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;

public class InMemoryBackgroundImageRepository : IInMemoryBackgroundImageRepository
{
    private readonly static ConcurrentDictionary<string, OSLImage> _backgroundImages = new();
    private readonly ILogger<InMemoryBackgroundImageRepository> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFileService _fileService;
    private readonly string fileName = "BackgroundImages.json";
    public InMemoryBackgroundImageRepository(ILogger<InMemoryBackgroundImageRepository> logger, IConfiguration configuration, IFileService fileService)
    {
        _fileService = fileService;
        _logger = logger;
        _configuration = configuration;
        // Load data from the first file into the first collection
        LoadDataFromFile().Wait();

    }
    public async Task<OSLImage?> Add(OSLImage backgroundImage)
    {
        bool saveToFile = false;
        try
        {

            if (_backgroundImages.TryAdd(backgroundImage.id, backgroundImage))
            {
                saveToFile = true;
                return await Task.FromResult(backgroundImage);
            }
            else
            {
                _logger.LogError($"Background Image {fileName} File list was not saved...");
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
                await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_backgroundImages.Values, Formatting.Indented));
            }
        }
    }
    public async Task<OSLImage?> Remove(string id)
    {
        bool saveToFile = false;
        try
        {
            if (_backgroundImages.TryRemove(id, out OSLImage backgroundImage))
            {
                saveToFile = true;
                return await Task.FromResult(backgroundImage);
            }
            else
            {
                _logger.LogError($"Background Image {fileName} File list was not saved...");
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
                await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_backgroundImages.Values, Formatting.Indented));
            }
        }
    }
    public async Task<OSLImage?> Update(OSLImage backgroundImage)
    {
        if (_backgroundImages.TryGetValue(backgroundImage.id, out OSLImage currentBackgroundImage) && _backgroundImages.TryUpdate(backgroundImage.id, backgroundImage, currentBackgroundImage))
        {
            await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_backgroundImages.Values, Formatting.Indented));
        }

        bool saveToFile = false;
        try
        {
            if (_backgroundImages.TryGetValue(backgroundImage.id, out OSLImage? currentimage) && _backgroundImages.TryUpdate(backgroundImage.id, backgroundImage, currentimage))
            {
                saveToFile = true;
                if (_backgroundImages.TryGetValue(backgroundImage.id, out OSLImage? osl))
                {

                    return await Task.FromResult(osl);
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
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_backgroundImages.Values, Formatting.Indented));
            }
        }
    }
    public OSLImage Get(string id)
    {
        _backgroundImages.TryGetValue(id, out OSLImage? backgroundImage);
        return backgroundImage;
    }
    public IEnumerable<OSLImage> GetAll() => _backgroundImages.Values;
    public async Task<bool> ProcessBackgroundImage(List<CoordinateSystem> coordinateSystems, CancellationToken stoppingToken)
    {
        bool saveToFile = false;
        try
        {
            if (coordinateSystems.Count > 0)
            {
                foreach (var coordinateSystem in coordinateSystems)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        saveToFile = false;
                        return false;
                    }
                    if (coordinateSystem.backgroundImages.Count > 0)
                    {
                        foreach (var backgroundImage in coordinateSystem.backgroundImages)
                        {
                            if (stoppingToken.IsCancellationRequested)
                            {
                                saveToFile = false;
                                return false;
                            }
                            if (_backgroundImages.ContainsKey(backgroundImage.id))
                            {
                                if (_backgroundImages.TryGetValue(backgroundImage.id, out OSLImage currentOSL))
                                {
                                    if (currentOSL.name != backgroundImage.name)
                                    {
                                        currentOSL.name = backgroundImage.name;
                                        saveToFile = true;
                                    }
                                    if (currentOSL.base64 != backgroundImage.base64)
                                    {
                                        currentOSL.base64 = backgroundImage.base64;
                                        saveToFile = true;
                                    }
                                    if (currentOSL.widthMeter != backgroundImage.widthMeter)
                                    {
                                        currentOSL.widthMeter = backgroundImage.widthMeter;
                                        saveToFile = true;
                                    }
                                    if (currentOSL.heightMeter != backgroundImage.heightMeter)
                                    {
                                        currentOSL.heightMeter = backgroundImage.heightMeter;
                                        saveToFile = true;
                                    }
                                    if (currentOSL.xMeter != backgroundImage.xMeter)
                                    {
                                        currentOSL.xMeter = backgroundImage.xMeter;
                                        saveToFile = true;
                                    }
                                    if (currentOSL.yMeter != backgroundImage.yMeter)
                                    {
                                        currentOSL.yMeter = backgroundImage.yMeter;
                                        saveToFile = true;
                                    }
                                    if (currentOSL.origoX != backgroundImage.origoX)
                                    {
                                        currentOSL.origoX = backgroundImage.origoX;
                                        saveToFile = true;
                                    }
                                    if (currentOSL.origoY != backgroundImage.origoY)
                                    {
                                        currentOSL.origoY = backgroundImage.origoY;
                                        saveToFile = true;
                                    }
                                    if (currentOSL.metersPerPixelX != backgroundImage.metersPerPixelX)
                                    {
                                        currentOSL.metersPerPixelX = backgroundImage.metersPerPixelX;
                                        saveToFile = true;
                                    }
                                    if (currentOSL.metersPerPixelY != backgroundImage.metersPerPixelY)
                                    {
                                        currentOSL.metersPerPixelY = backgroundImage.metersPerPixelY;
                                        saveToFile = true;
                                    }
                                    if (currentOSL.rotation != backgroundImage.rotation)
                                    {
                                        currentOSL.rotation = backgroundImage.rotation;
                                        saveToFile = true;
                                    }
                                    if (currentOSL.alpha != backgroundImage.alpha)
                                    {
                                        currentOSL.alpha = backgroundImage.alpha;
                                        saveToFile = true;
                                    }
                                    if (currentOSL.visible != backgroundImage.visible)
                                    {
                                        currentOSL.visible = backgroundImage.visible;
                                        saveToFile = true;
                                    }
                                    if (currentOSL.fileName != backgroundImage.fileName)
                                    {
                                        currentOSL.fileName = backgroundImage.fileName;
                                        saveToFile = true;
                                    }

                                }
                            }
                            else
                            {
                                _backgroundImages.TryAdd(backgroundImage.id, new OSLImage {
                                    coordinateSystemId = coordinateSystem.id,
                                    id = backgroundImage.id,
                                    name = backgroundImage.name,
                                    base64 = backgroundImage.base64,
                                    widthMeter = backgroundImage.widthMeter,
                                    heightMeter = backgroundImage.heightMeter,
                                    xMeter = backgroundImage.xMeter,
                                    yMeter = backgroundImage.yMeter,
                                    origoX = backgroundImage.origoX,
                                    origoY = backgroundImage.origoY,
                                    metersPerPixelX = backgroundImage.metersPerPixelX,
                                    metersPerPixelY = backgroundImage.metersPerPixelY,
                                    rotation = backgroundImage.rotation,
                                    alpha = backgroundImage.alpha,
                                    visible = backgroundImage.visible,
                                    fileName = backgroundImage.fileName,
                                    updateStatus = backgroundImage.updateStatus

                                });
                                saveToFile = true;
                            }
                       
                        }
                    }
                
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
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_backgroundImages.Select(x => x.Value).ToList(), Formatting.Indented));
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
                var data = JsonConvert.DeserializeObject<List<OSLImage>>(fileContent) ?? new List<OSLImage>();
                if (data.Count > 0)
                {
                    foreach (OSLImage item in data.Select(r => r).ToList())
                    {
                        _backgroundImages.TryAdd(item.id, item);
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

    public Task<bool> ResetBackgroundImageList()
    {
        try
        {
            _backgroundImages.Clear();
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Task.FromResult(true);
        }
    }

    public Task<bool> SetupBackgroundImageList()
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

    public async Task<bool> ProcessCiscoSpacesBackgroundImage(JToken? map, CancellationToken stoppingToken)
    {
        bool saveToFile = false;
        try
        {
            if (map.HasValues)
            {
                OSLImage bkg = new OSLImage
                {
                    widthMeter = ((JObject)map).ContainsKey("width") ? Convert.ToDouble(map["width"].ToString()) : 0.0,
                    heightMeter = ((JObject)map).ContainsKey("length") ? Convert.ToDouble(map["length"].ToString()) : 0.0,
                    origoX = ((JObject)map).ContainsKey("imageWidth") ? Convert.ToDouble(map["imageWidth"].ToString()) : 0.0,
                    origoY = ((JObject)map).ContainsKey("imageHeight") ? Convert.ToDouble(map["imageHeight"].ToString()) : 0.0,
                    base64 = ((JObject)map).ContainsKey("imagePath") ? map["imagePath"].ToString() : "",
                    name = ((JObject)map).ContainsKey("mongoId") ? map["mongoId"].ToString() : "",
                    coordinateSystemId = ((JObject)map).ContainsKey("name") ? map["name"].ToString() : "",
                    id  = ((JObject)map).ContainsKey("id") ? map["id"].ToString() : "",
                };
                if (_backgroundImages.ContainsKey(bkg.id))
                {
                    if (_backgroundImages.TryGetValue(bkg.id, out OSLImage currentOSL))
                    {
                        //check if width value are the same
                        if (currentOSL.widthMeter != bkg.widthMeter)
                        {
                            currentOSL.widthMeter = bkg.widthMeter;
                            saveToFile = true;
                        }
                        //check if high
                        if (currentOSL.heightMeter != bkg.heightMeter)
                        {
                            currentOSL.heightMeter = bkg.heightMeter;
                            saveToFile = true;
                        }
                        //check if origoY
                        if (currentOSL.origoY != bkg.origoY)
                        {
                            currentOSL.origoY = bkg.origoY;
                            saveToFile = true;
                        }
                        //check if origoX
                        if (currentOSL.origoX != bkg.origoX)
                        {
                            currentOSL.origoX = bkg.origoX;
                            saveToFile = true;
                        }
                        //check if base64
                        if (currentOSL.base64 != bkg.base64)
                        {
                            currentOSL.base64 = bkg.base64;
                            saveToFile = true;
                        }
                        //check if name
                        if (currentOSL.name != bkg.name)
                        {
                            currentOSL.name = bkg.name;
                            saveToFile = true;
                        }
                    }
                }
                else
                {
                    if (_backgroundImages.TryAdd(bkg.id, bkg))
                    {
                        saveToFile = true;
                    }
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
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_backgroundImages.Select(x => x.Value).ToList(), Formatting.Indented));
            }
        }
    }
}