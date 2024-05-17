using EIR_9209_2.Models;
using MongoDB.Driver.GeoJsonObjectModel;
using Newtonsoft.Json;
using System.Collections.Concurrent;

public class InMemoryGeoZonesRepository : IInMemoryGeoZonesRepository
{
    public readonly static ConcurrentDictionary<string, GeoZone> _geoZoneList = new();
    private readonly IFileService FileService;
    public InMemoryGeoZonesRepository(ILogger<InMemoryConnectionRepository> logger, IConfiguration configuration, IFileService fileService)
    {
        FileService = fileService;
        string BuildConnectionPath = Path.Combine(configuration[key: "ApplicationConfiguration:BaseDrive"], configuration[key: "ApplicationConfiguration:BaseDirectory"], configuration[key: "SiteIdentity:NassCode"], configuration[key: "ApplicationConfiguration:ConfigurationDirectory"], $"{configuration[key: "MongoDB:CollectionZones"]}.json");
        // Load data from the first file into the first collection
        Task.Run(async () => await LoadDataFromFile(BuildConnectionPath));

    }
    public void Add(GeoZone geoZone)
    {
        _geoZoneList.TryAdd(geoZone.Properties.Id, geoZone);
    }

    public void Remove(string geoZoneId)
    {
        _geoZoneList.TryRemove(geoZoneId, out _);
    }

    public GeoZone Get(string id)
    {
        _geoZoneList.TryGetValue(id, out GeoZone geoZone);
        return geoZone;
    }

    public IEnumerable<GeoZone> GetAll() => _geoZoneList.Values;

    public void Update(GeoZone geoZone)
    {
        if (_geoZoneList.TryGetValue(geoZone.Properties.Id, out GeoZone currentGeoZone))
        {
            _geoZoneList.TryUpdate(geoZone.Properties.Id, geoZone, currentGeoZone);
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
            List<GeoZone> data = JsonConvert.DeserializeObject<List<GeoZone>>(fileContent);

            // Insert the data into the MongoDB collection
            if (data.Any())
            {
                foreach (GeoZone item in data.Select(r => r).ToList())
                {
                    Add(item);
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            // Handle the FileNotFoundException here
            Console.WriteLine($"File not found: {ex.FileName}");
            // You can choose to throw an exception or take any other appropriate action
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            Console.WriteLine($"An error occurred when reading the file: {ex.Message}");
        }
        catch (JsonException ex)
        {
            // Handle errors when parsing the JSON
            Console.WriteLine($"An error occurred when parsing the JSON: {ex.Message}");
        }
    }
}